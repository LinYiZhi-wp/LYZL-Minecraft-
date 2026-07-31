using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using GeminiLauncher.Services.Network;

namespace GeminiLauncher.Services.Ecosystem
{
    public class ModLoaderService
    {
        private readonly DownloadService _downloadService;
        private const string FabricMetaUrl = "https://meta.fabricmc.net/v2";

        public ModLoaderService()
        {
            _downloadService = new DownloadService();
        }

        public async Task<JObject> InstallFabricAsync(string mcVersion, string loaderVersion, string dotMinecraftPath, IProgress<double>? progress = null, IProgress<string>? status = null)
        {
            string versionId = $"{mcVersion}-fabric-{loaderVersion}";
            string versionDir = Path.Combine(dotMinecraftPath, "versions", versionId);
            string jsonPath = Path.Combine(versionDir, $"{versionId}.json");

            string url = $"{FabricMetaUrl}/versions/loader/{mcVersion}/{loaderVersion}/profile/json";
            status?.Report($"Fetching Fabric metadata...");

            string jsonContent = await _downloadService.DownloadStringAsync(url);
            var json = JObject.Parse(jsonContent);

            if (!Directory.Exists(versionDir)) Directory.CreateDirectory(versionDir);

            json["id"] = versionId;
            File.WriteAllText(jsonPath, json.ToString());

            var libraries = json["libraries"] as JArray;
            if (libraries != null)
            {
                var downloads = new List<DownloadRequest>();

                foreach (var lib in libraries)
                {
                    string name = lib["name"]?.ToString() ?? "";
                    string urlBase = lib["url"]?.ToString() ?? "https://maven.fabricmc.net/";

                    if (string.IsNullOrEmpty(name)) continue;

                    var parts = name.Split(':');
                    if (parts.Length < 3) continue;

                    string group = parts[0].Replace('.', '/');
                    string artifact = parts[1];
                    string version = parts[2];
                    string path = $"{group}/{artifact}/{version}/{artifact}-{version}.jar";
                    string destPath = Path.Combine(dotMinecraftPath, "libraries", path);

                    if (!File.Exists(destPath))
                    {
                        downloads.Add(new DownloadRequest
                        {
                            Url = $"{urlBase}{path}",
                            DestinationPath = destPath
                        });
                    }
                }

                if (downloads.Any())
                {
                    status?.Report($"Downloading {downloads.Count} libraries...");
                    await _downloadService.DownloadBatchAsync(downloads, progress ?? new Progress<double>());
                }
            }

            return json;
        }

        private const string BMCLAPI_FORGE = "https://bmclapi2.bangbang93.com";

        public async Task InstallForgeAsync(string mcVersion, string forgeVersion, string dotMinecraftPath, IProgress<double>? progress = null, IProgress<string>? status = null)
        {
            status?.Report("正在获取Forge安装信息...");

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

                var versionsUrl = $"{BMCLAPI_FORGE}/forge/minecraft/{mcVersion}";
                var versionsJson = await client.GetStringAsync(versionsUrl);
                var versions = JArray.Parse(versionsJson);

                JToken? forgeData = null;
                foreach (var v in versions)
                {
                    if (v["version"]?.ToString() == forgeVersion)
                    {
                        forgeData = v;
                        break;
                    }
                }

                if (forgeData == null)
                {
                    foreach (var v in versions.OrderByDescending(x =>
                    {
                        int.TryParse(x["build"]?.ToString(), out int b);
                        return b;
                    }))
                    {
                        string ver = v["version"]?.ToString() ?? "";
                        if (ver.Contains(forgeVersion) || forgeVersion.Contains(ver.Split(' ').FirstOrDefault() ?? ""))
                        {
                            forgeData = v;
                            break;
                        }
                    }
                }

                if (forgeData == null)
                    throw new Exception($"找不到Forge版本 {forgeVersion}");

                // forgeVersionId is the full version string from BMCLAPI, e.g. "1.20.1-47.2.0"
                string forgeVersionId = forgeData["version"]?.ToString() ?? forgeVersion;

                // Extract the forge build number from the version ID (e.g. "47.2.0" from "1.20.1-47.2.0")
                string forgeBuild = forgeVersionId;
                int dashIdx = forgeVersionId.IndexOf('-');
                if (dashIdx > 0)
                    forgeBuild = forgeVersionId.Substring(dashIdx + 1);

                status?.Report($"正在下载Forge {forgeVersionId}...");

                // forgeVersionId already includes mcVersion prefix, so use it directly
                var installerUrl = $"{BMCLAPI_FORGE}/maven/net/minecraftforge/forge/{forgeVersionId}/forge-{forgeVersionId}-installer.jar";
                string installerPath = Path.Combine(dotMinecraftPath, "temp_forge_installer.jar");

                await _downloadService.DownloadFileAsync(installerUrl, installerPath);

                status?.Report("正在运行Forge安装程序...");

                await RunForgeInstallerAsync(installerPath, dotMinecraftPath, progress, status);

                if (File.Exists(installerPath))
                    File.Delete(installerPath);

                // The Forge installer creates the version folder as {mcVersion}-forge-{forgeBuild}
                // e.g. "1.20.1-forge-47.2.0"
                string forgeVersionDir = Path.Combine(dotMinecraftPath, "versions", $"{mcVersion}-forge-{forgeBuild}");
                string forgeJsonPath = Path.Combine(forgeVersionDir, $"{mcVersion}-forge-{forgeBuild}.json");

                // Fallback: try original forgeVersionId format (for older Forge)
                if (!File.Exists(forgeJsonPath))
                {
                    forgeVersionDir = Path.Combine(dotMinecraftPath, "versions", $"{mcVersion}-forge-{forgeVersionId}");
                    forgeJsonPath = Path.Combine(forgeVersionDir, $"{mcVersion}-forge-{forgeVersionId}.json");
                }

                if (File.Exists(forgeJsonPath))
                {
                    var forgeJson = JObject.Parse(File.ReadAllText(forgeJsonPath));
                    forgeJson["id"] = $"{mcVersion}-forge-{forgeBuild}";
                    File.WriteAllText(forgeJsonPath, forgeJson.ToString());
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[ForgeInstall] Warning: version JSON not found at {forgeJsonPath}");
                    // List what's in the versions folder for debugging
                    var versionsDir = Path.Combine(dotMinecraftPath, "versions");
                    if (Directory.Exists(versionsDir))
                    {
                        var dirs = Directory.GetDirectories(versionsDir);
                        foreach (var d in dirs.Where(x => Path.GetFileName(x).Contains("forge")))
                            System.Diagnostics.Debug.WriteLine($"[ForgeInstall] Found dir: {Path.GetFileName(d)}");
                    }
                }

                status?.Report("Forge安装完成！");
                progress?.Report(1.0);
            }
            catch (Exception ex)
            {
                status?.Report($"Forge安装失败: {ex.Message}");
                throw;
            }
        }

        private async Task RunForgeInstallerAsync(string installerPath, string dotMinecraftPath, IProgress<double>? progress, IProgress<string>? status)
        {
            status?.Report("正在处理Forge安装程序...");

            var javaPath = FindJavaPath();
            if (string.IsNullOrEmpty(javaPath))
                throw new Exception("未找到Java运行环境");

            var tempDir = Path.Combine(Path.GetTempPath(), $"forge_install_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);

            try
            {
                using var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = javaPath,
                        Arguments = $"-jar \"{installerPath}\" --installClient \"{dotMinecraftPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = tempDir
                    }
                };

                var outputBuilder = new System.Text.StringBuilder();
                var errorBuilder = new System.Text.StringBuilder();

                process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        outputBuilder.AppendLine(e.Data);
                };
                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        errorBuilder.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (!process.HasExited)
                {
                    await Task.Delay(500);
                    progress?.Report(0.5);
                }

                await process.WaitForExitAsync();

                var output = outputBuilder.ToString();
                var error = errorBuilder.ToString();
                System.Diagnostics.Debug.WriteLine($"[ForgeInstaller] ExitCode: {process.ExitCode}");
                System.Diagnostics.Debug.WriteLine($"[ForgeInstaller] Output: {output}");
                System.Diagnostics.Debug.WriteLine($"[ForgeInstaller] Error: {error}");

                if (process.ExitCode != 0)
                {
                    var errorMsg = string.IsNullOrEmpty(error) ? output : error;
                    throw new Exception($"Forge安装程序返回错误码: {process.ExitCode}\n{errorMsg}");
                }

                progress?.Report(1.0);
            }
            finally
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }

        private string? FindJavaPath()
        {
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrEmpty(javaHome))
            {
                var javapath = Path.Combine(javaHome, "bin", "java.exe");
                if (File.Exists(javapath)) return javapath;
            }

            var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>();
            foreach (var path in paths)
            {
                var javapath = Path.Combine(path, "java.exe");
                if (File.Exists(javapath)) return javapath;
            }

            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var javaDir = Path.Combine(programFiles, "Java");
            if (Directory.Exists(javaDir))
            {
                foreach (var dir in Directory.GetDirectories(javaDir))
                {
                    var javapath = Path.Combine(dir, "bin", "java.exe");
                    if (File.Exists(javapath)) return javapath;
                }
            }

            return null;
        }
    }
}
