using System;
using System.IO;
using Newtonsoft.Json;
using GeminiLauncher.Models;

namespace GeminiLauncher.Services
{
    public class AppConfig
    {
        public string JavaPath { get; set; } = string.Empty;
        public string GamePath { get; set; } = string.Empty;
        public int MaxRam { get; set; } = 4096;
        public int MinRam { get; set; } = 512;
        public bool AutoDetectMemory { get; set; } = true;
        public int WindowWidth { get; set; } = 854;
        public int WindowHeight { get; set; } = 480;
        public bool Fullscreen { get; set; } = false;
        public string DownloadSource { get; set; } = "Official";
        public Account? SelectedAccount { get; set; }
        public bool VersionIsolation { get; set; } = true;
        public string Language { get; set; } = "en-US";
        public string? LastSelectedVersionId { get; set; }

        public string? BackgroundImagePath { get; set; }
        public double BackgroundOpacity { get; set; } = 0.6;
        public double BlurEffectRadius { get; set; } = 0;

        public int LauncherVisibility { get; set; } = 0;
        public int ProcessPriority { get; set; } = 0;

        public int MaxDownloadThreads { get; set; } = 64;

        public string GlobalJvmArguments { get; set; } = string.Empty;
        public string GlobalGameArguments { get; set; } = string.Empty;
        public string CustomWindowTitle { get; set; } = string.Empty;

        public List<string> HiddenPageKeys { get; set; } = new List<string>();
    }

    public class ConfigService
    {
        private const string ConfigFileName = "config.json";
        private readonly string _configPath;
        private static readonly Lazy<ConfigService> _instance = new(() => new ConfigService(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static ConfigService Instance => _instance.Value;

        public AppConfig Settings { get; private set; }

        public ConfigService()
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
            Settings = LoadConfig();
        }

        private AppConfig LoadConfig()
        {
            if (!File.Exists(_configPath))
            {
                return new AppConfig();
            }

            try
            {
                string json = File.ReadAllText(_configPath);
                return JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
            }
            catch
            {
                return new AppConfig();
            }
        }

        public void SaveConfig()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                GeminiLauncher.Utilities.Logger.LogError(ex, "ConfigService.SaveConfig");
            }
        }
    }
}
