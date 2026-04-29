�Eusing System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace GeminiLauncher.Services.Network
{
    public class DownloadRequest
    {
        public string Url { get; set; } = string.Empty;
        public string DestinationPath { get; set; } = string.Empty;
        public string? Sha1 { get; set; }
        public long Size { get; set; }

        public DownloadRequest() { }
        public DownloadRequest(string url, string path, string? sha1 = null, long size = 0)
        {
            Url = url;
            DestinationPath = path;
            Sha1 = sha1;
            Size = size;
        }
    }

    public class DownloadService
    {
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _semaphore;

        public DownloadService(int maxConcurrency = 64)
        {
            // SocketsHttpHandler is better for connection pooling in .NET Core/5+
            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                MaxConnectionsPerServer = maxConcurrency
            };
            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) GeminiLauncher/1.0");
            
            _semaphore = new SemaphoreSlim(maxConcurrency);
        }

        public async Task<string> DownloadStringAsync(string url)
        {
            return await _httpClient.GetStringAsync(url);
        }

        public async Task DownloadFileAsync(string url, string path, string? expectedSha1 = null, IProgress<long>? progress = null, CancellationToken ct = default)
        {
            // Ensure directory exists
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            // Check if file exists and is valid
            if (File.Exists(path))
            {
                 if (string.IsNullOrEmpty(expectedSha1) || VerifyFile(path, expectedSha1))
                 {
                     return; 
                 }
                 // Invalid file, delete and redownload
                 File.Delete(path);
            }

            string partialPath = path + ".partial";
            
            // Retry logic
            int maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await DownloadInternalAsync(url, path, partialPath, progress, ct);
                    
                    // Verify after download
                    if (!string.IsNullOrEmpty(expectedSha1) && !VerifyFile(path, expectedSha1))
                    {
                        // Corruption? Delete and maybe retry without resume next time?
                        File.Delete(path);
                        if (File.Exists(partialPath)) File.Delete(partialPath);
                        throw new IOException($"SHA1 mismatch for {url}");
                    }
                    
                    return; // Success
                }
                catch (Exception)
                {
                    if (i == maxRetries - 1) throw; // Rethrow on last attempt
                    await Task.Delay(1000 * (i + 1), ct); // Backoff
                }
            }
        }
        
        private async Task DownloadInternalAsync(string url, string finalPath, string partialPath, IProgress<long>? progress, CancellationToken ct)
        {
            long startOffset = 0;
            if (File.Exists(partialPath))
            {
                startOffset = new FileInfo(partialPath).Length;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (startOffset > 0)
            {
                request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(startOffset, null);
            }

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            
            if (response.StatusCode == System.Net.HttpStatusCode.RequestedRangeNotSatisfiable)
            {
                 // Server doesn't like range, reset
                 startOffset = 0;
                 File.Delete(partialPath);
                 using var freshResponse = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                 freshResponse.EnsureSuccessStatusCode();
                 await SaveStream(freshResponse, partialPath, false, progress, ct);
            }
            else
            {
                response.EnsureSuccessStatusCode();
                await SaveStream(response, partialPath, startOffset > 0, progress, ct);
            }

            File.Move(partialPath, finalPath, true);
        }

        private async Task SaveStream(HttpResponseMessage response, string path, bool append, IProgress<long>? progress, CancellationToken ct)
        {
            var expectedLength = response.Content.Headers.ContentLength;
            using var contentStream = await response.Content.ReadAsStreamAsync(ct);
            using var fileStream = new FileStream(path, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read, 8192, true);
            
            var buffer = new byte[8192];
            int bytesRead;
            long totalSavedInThisSession = 0;
            
            while (true)
            {
                // Aggressive timeout for reading to prevent hanging at 99%
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(30)); 

                try
                {
                    bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                {
                    throw new TimeoutException("读取数据流超时（30秒内无数据）");
                }

                if (bytesRead <= 0) break;

                await fileStream.WriteAsync(buffer, 0, bytesRead, ct);
                totalSavedInThisSession += bytesRead;
                progress?.Report(bytesRead);
            }

            // Final safety check if we have Content-Length
            if (expectedLength.HasValue && totalSavedInThisSession < expectedLength.Value && !append)
            {
                throw new IOException($"下载不完整: 预期 {expectedLength.Value} 字节, 实际收到 {totalSavedInThisSession} 字节");
            }
        }

        private bool VerifyFile(string path, string expectedSha1)
        {
            try
            {
                using var sha1 = SHA1.Create();
                using var stream = File.OpenRead(path);
                var hash = sha1.ComputeHash(stream);
                var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                return hashString.Equals(expectedSha1, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public async Task DownloadBatchAsync(List<DownloadRequest> requests, IProgress<double> progress, IProgress<long>? byteProgress = null, CancellationToken ct = default)
        {
            int completedCount = 0;
            var tasks = new List<Task>();
            object lockObj = new object();

            foreach (var req in requests)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await _semaphore.WaitAsync(ct);
                    try
                    {
                        // Pass a progress that reports back to the manager for speed tracking
                        // (Note: This specific implementation would need to be very carefully tuned for perf)
                        await DownloadFileAsync(req.Url, req.DestinationPath, req.Sha1, byteProgress, ct);
                    }
                    finally
                    {
                        _semaphore.Release();
                        lock(lockObj)
                        {
                            completedCount++;
                            progress.Report((double)completedCount / requests.Count);
                        }
                    }
                }, ct));
            }
            
            await Task.WhenAll(tasks);
        }
    }
}
�; �;�<*cascade08
�<�A �A�A*cascade08
�A�E 2Tfile:///c:/Users/Linyizhi/.gemini/GeminiLauncher/Services/Network/DownloadService.cs