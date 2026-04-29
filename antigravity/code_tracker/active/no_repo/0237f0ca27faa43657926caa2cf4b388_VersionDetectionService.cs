�Wusing System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using GeminiLauncher.Models;

namespace GeminiLauncher.Services
{
    /// <summary>
    /// 版本检测服务 - 自动扫描和解析游戏版本
    /// </summary>
    public class VersionDetectionService
    {
        /// <summary>
        /// 自动检测所有可用的游戏目录
        /// </summary>
        public List<GameDirectory> DetectGameDirectories()
        {
            var directories = new List<GameDirectory>();

            // 1. 当前默认目录（从配置或默认路径）
            string currentPath = GetCurrentGamePath();
            if (!string.IsNullOrEmpty(currentPath) && Directory.Exists(currentPath))
            {
                directories.Add(new GameDirectory
                {
                    Name = "当前文件夹",
                    Path = currentPath,
                    IsDefault = true,
                    Source = DirectorySource.Current
                });
            }

            // 2. 官方启动器目录
            string officialPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ".minecraft"
            );
            if (Directory.Exists(officialPath) && officialPath != currentPath)
            {
                directories.Add(new GameDirectory
                {
                    Name = "官方启动器文件夹",
                    Path = officialPath,
                    IsDefault = false,
                    Source = DirectorySource.Auto
                });
            }

            // 3. TODO: 从配置文件加载用户自定义目录

            return directories;
        }

        /// <summary>
        /// 扫描指定游戏目录下的所有版本
        /// </summary>
        public List<GameVersion> DetectVersions(string gamePath)
        {
            var versions = new List<GameVersion>();
            string versionsPath = Path.Combine(gamePath, "versions");

            if (!Directory.Exists(versionsPath))
            {
                return versions;
            }

            // 遍历所有版本文件夹
            foreach (var versionDir in Directory.GetDirectories(versionsPath))
            {
                string versionId = Path.GetFileName(versionDir);
                string versionJsonPath = Path.Combine(versionDir, $"{versionId}.json");

                if (!File.Exists(versionJsonPath))
                {
                    // version.json不存在，标记为错误版本
                    versions.Add(new GameVersion
                    {
                        Id = versionId,
                        DisplayName = versionId,
                        Category = VersionCategory.Broken,
                        Icon = "⚠️",
                        GamePath = gamePath
                    });
                    continue;
                }

                try
                {
                    var version = ParseVersionJson(versionJsonPath, versionId, gamePath);
                    versions.Add(version);
                }
                catch
                {
                    // 解析失败，标记为错误版本
                    versions.Add(new GameVersion
                    {
                        Id = versionId,
                        DisplayName = versionId,
                        Category = VersionCategory.Broken,
                        Icon = "⚠️",
                        GamePath = gamePath
                    });
                }
            }

            return versions;
        }

        /// <summary>
        /// 解析version.json文件
        /// </summary>
        private GameVersion ParseVersionJson(string jsonPath, string versionId, string gamePath)
        {
            string jsonContent = File.ReadAllText(jsonPath);
            using JsonDocument doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            var version = new GameVersion
            {
                Id = versionId,
                GamePath = gamePath
            };

            // 获取MC版本号
            if (root.TryGetProperty("id", out var idElement))
            {
                version.MinecraftVersion = idElement.GetString() ?? versionId;
            }

            // 检测版本类型
            if (root.TryGetProperty("type", out var typeElement))
            {
                string typeStr = typeElement.GetString() ?? "release";
                version.Type = typeStr.ToLower() switch
                {
                    "snapshot" => VersionType.Snapshot,
                    "old_alpha" => VersionType.OldAlpha,
                    "old_beta" => VersionType.OldBeta,
                    _ => VersionType.Release
                };
            }

            // 检测Mod加载器
            DetectModLoader(root, versionId, version);

            // 设置分类和图标
            if (version.Loader != null)
            {
                version.Category = VersionCategory.Moddable;
                version.Icon = version.Loader switch
                {
                    ModLoader.Forge => "🔧",
                    ModLoader.Fabric => "🧵",
                    ModLoader.NeoForge => "🧊",
                    ModLoader.Quilt => "🪡",
                    _ => "📦"
                };
            }
            else
            {
                version.Category = VersionCategory.Vanilla;
                version.Icon = "📦";
            }

            // 生成显示名称
            version.DisplayName = GenerateDisplayName(version);

            return version;
        }

        /// <summary>
        /// 检测Mod加载器类型和版本
        /// </summary>
        private void DetectModLoader(JsonElement root, string versionId, GameVersion version)
        {
            // 方法1: 从ID检测
            string lowerVersionId = versionId.ToLower();
            
            if (lowerVersionId.Contains("forge"))
            {
                version.Loader = ModLoader.Forge;
                version.LoaderVersion = ExtractLoaderVersionFromId(versionId, "forge");
                return;
            }
            
            if (lowerVersionId.Contains("fabric"))
            {
                version.Loader = ModLoader.Fabric;
                version.LoaderVersion = ExtractLoaderVersionFromId(versionId, "fabric");
                return;
            }
            
            if (lowerVersionId.Contains("neoforge"))
            {
                version.Loader = ModLoader.NeoForge;
                version.LoaderVersion = ExtractLoaderVersionFromId(versionId, "neoforge");
                return;
            }

            if (lowerVersionId.Contains("quilt"))
            {
                version.Loader = ModLoader.Quilt;
                version.LoaderVersion = ExtractLoaderVersionFromId(versionId, "quilt");
                return;
            }

            // 方法2: 从libraries检测
            if (root.TryGetProperty("libraries", out var libraries))
            {
                foreach (var lib in libraries.EnumerateArray())
                {
                    if (lib.TryGetProperty("name", out var nameElement))
                    {
                        string libName = nameElement.GetString() ?? "";
                        
                        if (libName.Contains("net.minecraftforge:forge"))
                        {
                            version.Loader = ModLoader.Forge;
                            version.LoaderVersion = ExtractVersionFromMavenName(libName);
                            return;
                        }
                        
                        if (libName.Contains("net.fabricmc:fabric-loader"))
                        {
                            version.Loader = ModLoader.Fabric;
                            version.LoaderVersion = ExtractVersionFromMavenName(libName);
                            return;
                        }
                        
                        if (libName.Contains("net.neoforged:forge") || libName.Contains("net.neoforged:neoforge"))
                        {
                            version.Loader = ModLoader.NeoForge;
                            version.LoaderVersion = ExtractVersionFromMavenName(libName);
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 从版本ID提取Mod加载器版本
        /// </summary>
        private string ExtractLoaderVersionFromId(string versionId, string loaderName)
        {
            // 例如: "1.21.8-forge-58.0.3" -> "58.0.3"
            var parts = versionId.Split('-');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].ToLower() == loaderName && i + 1 < parts.Length)
                {
                    return parts[i + 1];
                }
            }
            return "";
        }

        /// <summary>
        /// 从Maven名称提取版本号
        /// </summary>
        private string ExtractVersionFromMavenName(string mavenName)
        {
            // 例如: "net.minecraftforge:forge:1.21.8-58.0.3" -> "58.0.3"
            var parts = mavenName.Split(':');
            if (parts.Length >= 3)
            {
                var versionPart = parts[2];
                // 可能是 "1.21.8-58.0.3"，提取最后的部分
                var dashIndex = versionPart.LastIndexOf('-');
                if (dashIndex > 0)
                {
                    return versionPart.Substring(dashIndex + 1);
                }
                return versionPart;
            }
            return "";
        }

        /// <summary>
        /// 生成显示名称
        /// </summary>
        private string GenerateDisplayName(GameVersion version)
        {
            if (version.Loader != null)
            {
                return $"{version.Id}";
            }
            return version.Id;
        }

        /// <summary>
        /// 获取当前游戏路径（从配置或默认）
        /// </summary>
        private string GetCurrentGamePath()
        {
            // TODO: 从配置文件读取
            // 暂时返回默认路径
            string defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ".minecraft"
            );
            
            // 如果存在D:\pc\2\.minecraft，优先使用（开发测试）
            string devPath = @"D:\pc\2\.minecraft";
            if (Directory.Exists(devPath))
            {
                return devPath;
            }

            return defaultPath;
        }
    }
}
�W*cascade082Tfile:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Services/VersionDetectionService.cs