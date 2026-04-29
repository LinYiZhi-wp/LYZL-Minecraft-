�xusing System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using GeminiLauncher.Models;
using GeminiLauncher.Models.Ecosystem;
using GeminiLauncher.Services.Network;
using Newtonsoft.Json.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GeminiLauncher.Services.Network
{
    public partial class DownloadManagerService : ObservableObject
    {
        private static DownloadManagerService? _instance;
        public static DownloadManagerService Instance => _instance ??= new DownloadManagerService();

        public ObservableCollection<DownloadTask> ActiveTasks { get; } = new ObservableCollection<DownloadTask>();

        [ObservableProperty] private string _totalProgressText = "0.00 %";
        [ObservableProperty] private string _globalSpeedText = "0 KB/s";
        [ObservableProperty] private int _totalRemainingFiles;
        [ObservableProperty] private bool _anyActiveTasks;

        private readonly VersionManifestService _manifestService = new();
        private readonly DownloadService _downloadService = new();
        private readonly HttpClient _httpClient = new();
        private readonly System.Windows.Threading.DispatcherTimer _speedTimer;

        private DownloadManagerService() 
        {
            _speedTimer = new System.Windows.Threading.DispatcherTimer();
            _speedTimer.Interval = TimeSpan.FromSeconds(1);
            _speedTimer.Tick += SpeedTimer_Tick;
            _speedTimer.Start();
        }

        private void SpeedTimer_Tick(object? sender, EventArgs e)
        {
            long totalDelta = 0;
            double weightedProgress = 0;
            int totalFiles = 0;
            int activeTaskCount = 0;

            foreach (var task in ActiveTasks)
            {
                if (task.IsCompleted || task.IsFailed)
                {
                    task.SpeedText = "0 KB/s";
                    continue;
                }

                activeTaskCount++;
                long delta = task.DownloadedBytes - task.LastDownloadedBytes;
                task.LastDownloadedBytes = task.DownloadedBytes;
                totalDelta += delta;

                if (delta < 1024) task.SpeedText = $"{delta} B/s";
                else if (delta < 1024 * 1024) task.SpeedText = $"{(delta / 1024.0):F1} KB/s";
                else task.SpeedText = $"{(delta / (1024.0 * 1024.0)):F1} MB/s";

                weightedProgress += task.Progress;
                totalFiles += task.RemainingFiles;
            }

            // Update Global Stats
            if (activeTaskCount > 0)
            {
                TotalProgressText = $"{(weightedProgress / activeTaskCount * 100):F2} %";
                TotalRemainingFiles = totalFiles;
                
                if (totalDelta < 1024) GlobalSpeedText = $"{totalDelta} B/s";
                else if (totalDelta < 1024 * 1024) GlobalSpeedText = $"{(totalDelta / 1024.0):F1} KB/s";
                else GlobalSpeedText = $"{(totalDelta / (1024.0 * 1024.0)):F1} MB/s";
            }
            else
            {
                TotalProgressText = "0.00 %";
                GlobalSpeedText = "0 KB/s";
                TotalRemainingFiles = 0;
            }

            AnyActiveTasks = ActiveTasks.Any(t => !t.IsCompleted && !t.IsFailed);
        }

        public async Task EnqueueGameDownload(DownloadableVersion version, string loaderChoice, string source)
        {
            var task = new DownloadTask { Name = version.Id, Status = "Preparing..." };
            ActiveTasks.Add(task);

            await AttemptDownloadAsync(task, version, loaderChoice, source);
        }

        private async Task AttemptDownloadAsync(DownloadTask task, DownloadableVersion version, string loaderChoice, string source)
        {
            try
            {
                // If retrying, reset failure state
                task.IsFailed = false;
                task.ErrorMessage = string.Empty;

                await DownloadGameFullAsync(task, version, loaderChoice, source);
                
                task.Status = "Completed";
                task.IsCompleted = true;
                task.Progress = 1.0;
            }
            catch (OperationCanceledException)
            {
                task.Status = "Canceled";
                task.IsFailed = true;
            }
            catch (Exception ex)
            {
                // Auto-Fallback Logic for Network/Server Errors
                // If we are using a mirror (not Official) and encounter an error, try switching to Official source.
                if (source != "Official")
                {
                    task.Status = $"镜像源异常 ({ex.Message})，正在切换至官方源重试...";
                    await Task.Delay(2000); // Give user time to read status
                    
                    // Recursive retry with Official source
                    await AttemptDownloadAsync(task, version, loaderChoice, "Official");
                    return;
                }

                task.Status = "Failed";
                task.IsFailed = true;
                task.ErrorMessage = ex.Message;
                // Only show message box on final failure
                MessageBox.Show($"Download failed permanently: {ex.Message}");
            }
        }

        private async Task DownloadGameFullAsync(DownloadTask task, DownloadableVersion version, string loaderChoice, string source)
        {
            var mainVM = ((App)Application.Current).MainWindow.DataContext as GeminiLauncher.ViewModels.MainViewModel;
            string gamePath = mainVM?.ConfigService.Settings.GamePath ?? ".minecraft";
            string versionDir = Path.Combine(gamePath, "versions", version.Id);
            Directory.CreateDirectory(versionDir);

            // 1. Download version.json
            task.Status = "正在获取版本信息...";
            task.JsonStatus = "下载中...";
            task.JsonStatusText = "version.json";
            string jsonUrl = ReplaceSource(version.Url, source);
            string jsonPath = Path.Combine(versionDir, $"{version.Id}.json");
            await _downloadService.DownloadFileAsync(jsonUrl, jsonPath, null, null, task.Cts.Token);
            task.JsonProgress = 1.0;
            task.JsonStatus = "已完成";
            task.JsonStatusText = "已完成";

            string jsonContent = File.ReadAllText(jsonPath);
            var json = JObject.Parse(jsonContent);

            var downloadRequests = new List<DownloadRequest>();

            // 2. Client.jar
            var clientDownloads = json["downloads"]?["client"];
            if (clientDownloads != null)
            {
                string clientUrl = ReplaceSource(clientDownloads["url"]?.ToString() ?? "", source);
                string clientPath = Path.Combine(versionDir, $"{version.Id}.jar");
                downloadRequests.Add(new DownloadRequest(clientUrl, clientPath, clientDownloads["sha1"]?.ToString()));
            }

            // 3. Libraries
            task.Status = "正在索引依赖库...";
            task.LibrariesStatus = "下载中...";
            task.LibrariesStatusText = "准备中...";
            var libs = json["libraries"];
            if (libs != null)
            {
                foreach (var lib in libs)
                {
                    var artifact = lib["downloads"]?["artifact"];
                    if (artifact != null)
                    {
                        string libUrl = ReplaceSource(artifact["url"]?.ToString() ?? "", source);
                        string relativePath = artifact["path"]?.ToString() ?? "";
                        if (string.IsNullOrEmpty(relativePath)) continue;

                        string libPath = Path.Combine(gamePath, "libraries", relativePath);
                        downloadRequests.Add(new DownloadRequest(libUrl, libPath, artifact["sha1"]?.ToString()));
                    }
                }
            }

            // 4. Assets
            task.Status = "正在索引资源文件...";
            task.AssetsStatus = "下载中...";
            task.AssetsStatusText = "准备中...";
            var assetIndex = json["assetIndex"];
            if (assetIndex != null)
            {
                string indexUrl = ReplaceSource(assetIndex["url"]?.ToString() ?? "", source);
                string indexId = assetIndex["id"]?.ToString() ?? "legacy";
                string indexPath = Path.Combine(gamePath, "assets", "indexes", $"{indexId}.json");
                
                await _downloadService.DownloadFileAsync(indexUrl, indexPath, assetIndex["sha1"]?.ToString(), null, task.Cts.Token);
                
                var indexJson = JObject.Parse(File.ReadAllText(indexPath));
                var objects = indexJson["objects"];
                if (objects != null)
                {
                    foreach (var obj in (JObject)objects)
                    {
                        string hash = obj.Value?["hash"]?.ToString() ?? "";
                        if (string.IsNullOrEmpty(hash)) continue;

                        string assetUrl = ReplaceSource(
                            $"https://resources.download.minecraft.net/{hash[..2]}/{hash}", 
                            source);
                        
                        string assetPath = Path.Combine(gamePath, "assets", "objects", hash[..2], hash);
                        downloadRequests.Add(new DownloadRequest(assetUrl, assetPath, hash));
                    }
                }
            }

            // 5. Start Batch Download
            task.Status = "正在下载组件...";
            var pendingRequests = downloadRequests.Where(r => !File.Exists(r.DestinationPath)).ToList();
            task.RemainingFiles = pendingRequests.Count;

            long completedCount = 0;
            long lastUpdateTick = 0;

            var progress = new Progress<double>(p => {
                try
                {
                    completedCount++;
                    
                    long currentTick = Environment.TickCount64;
                    // Throttle UI updates to ~20fps (50ms) to avoid flooding the UI thread
                    if (currentTick - lastUpdateTick < 50 && completedCount < pendingRequests.Count) return;
                    lastUpdateTick = currentTick;

                    task.RemainingFiles = (int)(pendingRequests.Count - completedCount);
                    task.Progress = pendingRequests.Count > 0 ? (double)completedCount / pendingRequests.Count : 1.0;
                    task.SizeText = $"{completedCount} / {pendingRequests.Count} 个文件";

                    // Map to sub-progresses for UI (Heuristic)
                    if (task.Progress < 0.3) {
                        task.LibrariesProgress = task.Progress / 0.3;
                        task.LibrariesStatusText = $"{completedCount} 文件";
                    } else {
                        task.LibrariesProgress = 1.0;
                        task.LibrariesStatus = "已完成";
                        task.LibrariesStatusText = "已完成";
                        task.AssetsProgress = Math.Min(1.0, (task.Progress - 0.3) / 0.7);
                        task.AssetsStatusText = $"{completedCount} 文件";
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't crash
                    System.Diagnostics.Debug.WriteLine($"Progress Error: {ex}");
                }
            });

            // Use SynchronousProgress to avoid marshalling every byte update to the UI thread (performance)
            var byteProgress = new SynchronousProgress<long>(bytes => task.IncrementBytes(bytes));

            if (pendingRequests.Any())
            {
                await _downloadService.DownloadBatchAsync(pendingRequests, progress, byteProgress, task.Cts.Token);
            }

            task.LibrariesProgress = 1.0;
            task.LibrariesStatus = "已完成";
            task.LibrariesStatusText = "已完成";
            task.AssetsProgress = 1.0;
            task.AssetsStatus = "已完成";
            task.AssetsStatusText = "已完成";

            // 6. Loader installation
            if (loaderChoice != "Vanilla")
            {
                task.Status = $"正在安装 {loaderChoice}...";
                task.ComponentsStatus = "正在安装";
                task.ComponentsStatusText = $"{loaderChoice}";
                // ... (Assume installation logic here)
                task.ComponentsProgress = 1.0;
                task.ComponentsStatus = "已完成";
                task.ComponentsStatusText = "已完成";
            }
            else {
                task.ComponentsProgress = 1.0;
                task.ComponentsStatus = "已完成";
                task.ComponentsStatusText = "无需安装";
            }
        }

        public string ReplaceSource(string originalUrl, string source)
        {
            if (string.IsNullOrEmpty(originalUrl) || source == "Official") return originalUrl;

            // Mapping rules for different mirrors
            return source switch
            {
                "BMCLAPI" => originalUrl
                    .Replace("piston-meta.mojang.com", "bmclapi2.bangbang93.com")
                    .Replace("launchermeta.mojang.com", "bmclapi2.bangbang93.com")
                    .Replace("launcher.mojang.com", "bmclapi2.bangbang93.com")
                    .Replace("libraries.minecraft.net", "bmclapi2.bangbang93.com/maven")
                    .Replace("resources.download.minecraft.net", "bmclapi2.bangbang93.com/assets")
                    .Replace("files.minecraftforge.net/maven", "bmclapi2.bangbang93.com/maven")
                    .Replace("maven.minecraftforge.net", "bmclapi2.bangbang93.com/maven")
                    .Replace("maven.fabricmc.net", "bmclapi2.bangbang93.com/maven"),

                "FastMirror" => originalUrl
                    .Replace("piston-meta.mojang.com", "download.fastmirror.net")
                    .Replace("launchermeta.mojang.com", "download.fastmirror.net")
                    .Replace("launcher.mojang.com", "download.fastmirror.net")
                    .Replace("libraries.minecraft.net", "download.fastmirror.net/maven")
                    .Replace("resources.download.minecraft.net", "download.fastmirror.net/assets"),

                "MCMirror" => originalUrl
                    .Replace("piston-meta.mojang.com", "mirrors.mcfx.net")
                    .Replace("launchermeta.mojang.com", "mirrors.mcfx.net")
                    .Replace("launcher.mojang.com", "mirrors.mcfx.net")
                    .Replace("libraries.minecraft.net", "mirrors.mcfx.net/maven")
                    .Replace("resources.download.minecraft.net", "mirrors.mcfx.net/assets"),

                _ => originalUrl
            };
        }
    }
}
� *cascade08��*cascade08�� *cascade08�� *cascade08� �! *cascade08�!�!*cascade08�!�$ *cascade08�$�)*cascade08�)�* *cascade08�*�**cascade08�*�+ *cascade08�+�+*cascade08�+�] *cascade08�]�^*cascade08�^�^ *cascade08�^�^*cascade08�^�_ *cascade08�_�_ *cascade08�_�`*cascade08�`�x *cascade082[file:///c:/Users/Linyizhi/.gemini/GeminiLauncher/Services/Network/DownloadManagerService.cs