¶using System;
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

            // 1. Fetch Profile JSON
            string url = $"{FabricMetaUrl}/versions/loader/{mcVersion}/{loaderVersion}/profile/json";
            status?.Report($"Fetching Fabric metadata...");
            
            string jsonContent = await _downloadService.DownloadStringAsync(url);
            var json = JObject.Parse(jsonContent);

            // 2. Setup Directory
            if (!Directory.Exists(versionDir)) Directory.CreateDirectory(versionDir);

            // 3. Fix ID in JSON (Fabric meta returns "id": "fabric-loader-...")
            json["id"] = versionId;
            File.WriteAllText(jsonPath, json.ToString());

            // 4. Download Libraries
            var libraries = json["libraries"] as JArray;
            if (libraries != null)
            {
                var downloads = new List<DownloadRequest>();

                foreach (var lib in libraries)
                {
                    string name = lib["name"]?.ToString() ?? "";
                    string urlBase = lib["url"]?.ToString() ?? "https://maven.fabricmc.net/";
                    
                    if (string.IsNullOrEmpty(name)) continue;

                    // Parse Maven coordinates (group:name:version)
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
    }
}
Æ *cascade08ÆÌ*cascade08Ì¶ *cascade082Wfile:///c:/Users/Linyizhi/.gemini/GeminiLauncher/Services/Ecosystem/ModLoaderService.cs