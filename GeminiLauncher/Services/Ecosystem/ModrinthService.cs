using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Linq;
using GeminiLauncher.Models.Ecosystem;

namespace GeminiLauncher.Services.Ecosystem
{
    public class ModrinthService
    {
        private static readonly Lazy<HttpClient> _lazyClient = new(() =>
        {
            var client = new HttpClient(new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                MaxConnectionsPerServer = 10
            });
            client.BaseAddress = new Uri("https://api.modrinth.com/v2/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("LYZL/2.0");
            client.Timeout = TimeSpan.FromSeconds(15);
            return client;
        }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private static HttpClient _httpClient => _lazyClient.Value;

        public async Task<List<ModProject>> SearchProjectsAsync(string query, int limit = 20, string sort = "relevance", string? projectType = null, int offset = 0)
        {
            try
            {
                var url = $"search?query={Uri.EscapeDataString(query)}&limit={limit}&index={sort}&offset={offset}";
                if (!string.IsNullOrEmpty(projectType))
                    url += $"&facets=[[\"project_type:{projectType}\"]]"; 

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var jsonStr = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(jsonStr);

                var projects = new List<ModProject>();
                var hits = json["hits"];
                if (hits != null)
                {
                    foreach (var hit in hits)
                    {
                    var rawType = hit["project_type"]?.ToString() ?? "mod";
                    projects.Add(new ModProject
                    {
                        Id = hit["project_id"]?.ToString() ?? "",
                        Name = hit["title"]?.ToString() ?? "",
                        Summary = hit["description"]?.ToString() ?? "",
                        IconUrl = hit["icon_url"]?.ToString() ?? "https://cdn.modrinth.com/assets/logo.png",
                        Author = hit["author"]?.ToString() ?? "",
                        Downloads = (long)(hit["downloads"] ?? 0),
                        Platform = ProjectPlatform.Modrinth,
                        Type = ParseProjectType(rawType),
                        WebUrl = $"https://modrinth.com/{rawType}/{hit["slug"] ?? hit["project_id"]}"
                    });
                }
                }
                return projects;
            }
            catch (Exception)
            {
                return new List<ModProject>();
            }
        }

        private static ProjectType ParseProjectType(string raw) => raw switch
        {
            "mod" => ProjectType.Mod,
            "modpack" => ProjectType.Modpack,
            "resourcepack" => ProjectType.ResourcePack,
            "shader" => ProjectType.Shader,
            "datapack" => ProjectType.DataPack,
            _ => ProjectType.Mod
        };

        /// <summary>
        /// Get trending/popular projects sorted by downloads
        /// </summary>
        public Task<List<ModProject>> GetTrendingAsync(int limit = 10, string? projectType = null)
            => SearchProjectsAsync("", limit, "downloads", projectType);

        /// <summary>
        /// Get newest projects sorted by newest
        /// </summary>
        public Task<List<ModProject>> GetNewestAsync(int limit = 10, string? projectType = null)
            => SearchProjectsAsync("", limit, "newest", projectType);

        public async Task<List<ModFile>> GetVersionsAsync(string projectId, string? gameVersion = null, string? loader = null)
        {
             // Build query filters
             var query = $"project/{projectId}/version";
             var filters = new List<string>();
             
             if (!string.IsNullOrEmpty(gameVersion)) filters.Add($"game_versions=[\"{gameVersion}\"]");
             if (!string.IsNullOrEmpty(loader)) filters.Add($"loaders=[\"{loader}\"]");
             
             if (filters.Any()) query += "?" + string.Join("&", filters);

             try
             {
                 var response = await _httpClient.GetAsync(query);
                 response.EnsureSuccessStatusCode();
                 var jsonStr = await response.Content.ReadAsStringAsync();
                 var jsonArray = JArray.Parse(jsonStr);

                 var files = new List<ModFile>();
                 foreach (var v in jsonArray)
                 {
                     var primaryFile = v["files"]?.FirstOrDefault(f => (bool?)f["primary"] == true) ?? v["files"]?.FirstOrDefault();
                     
                     if (primaryFile != null)
                     {
                         files.Add(new ModFile
                         {
                             FileId = v["id"]?.ToString() ?? "",
                             FileName = primaryFile["filename"]?.ToString() ?? "",
                             DownloadUrl = primaryFile["url"]?.ToString() ?? "",
                             Size = (long)(primaryFile["size"] ?? 0),
                             ReleaseDate = v["date_published"]?.ToString() ?? "",
                             GameVersions = v["game_versions"]?.ToObject<List<string>>() ?? new List<string>(),
                             Loaders = v["loaders"]?.ToObject<List<string>>() ?? new List<string>(),
                             Dependencies = v["dependencies"]?.Select(d => new ModDependency
                             {
                                  ProjectId = d["project_id"]?.ToString() ?? string.Empty,
                                  VersionId = d["version_id"]?.ToString() ?? string.Empty,
                                  FileName = d["filename"]?.ToString() ?? string.Empty,
                                  DependencyType = d["dependency_type"]?.ToString() ?? "required"
                             }).ToList() ?? new List<ModDependency>()
                         });
                     }
                 }
                 return files;
             }
             catch
             {
                 return new List<ModFile>();
             }
        }
        public async Task<ModFile?> GetVersionByHashAsync(string hash, string algorithm = "sha1")
        {
            try
            {
                var response = await _httpClient.GetAsync($"version_file/{hash}?algorithm={algorithm}");
                if (!response.IsSuccessStatusCode) return null;

                var jsonStr = await response.Content.ReadAsStringAsync();
                var v = JObject.Parse(jsonStr);
                
                var primaryFile = v["files"]?.FirstOrDefault(f => (bool?)f["primary"] == true) ?? v["files"]?.FirstOrDefault();
                if (primaryFile == null) return null;

                return new ModFile
                {
                    FileId = v["id"]?.ToString() ?? "",
                    ProjectId = v["project_id"]?.ToString() ?? "",
                    FileName = primaryFile["filename"]?.ToString() ?? "",
                    DownloadUrl = primaryFile["url"]?.ToString() ?? "",
                    Size = (long)(primaryFile["size"] ?? 0),
                    // We can add hashes here if needed for index
                    Hashes = new Dictionary<string, string>
                    {
                        { "sha1", primaryFile["hashes"]?["sha1"]?.ToString() ?? "" },
                        { "sha512", primaryFile["hashes"]?["sha512"]?.ToString() ?? "" }
                    }
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
