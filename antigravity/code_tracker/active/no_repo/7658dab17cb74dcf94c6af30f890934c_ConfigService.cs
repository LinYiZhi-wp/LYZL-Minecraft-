‚using System;
using System.IO;
using Newtonsoft.Json;
using GeminiLauncher.Models;

namespace GeminiLauncher.Services
{
    public class AppConfig
    {
        public string JavaPath { get; set; } = string.Empty;
        public string GamePath { get; set; } = string.Empty; // .minecraft path
        public int MaxRam { get; set; } = 4096;
        public string DownloadSource { get; set; } = "Official"; // Official, BMCLAPI, MCBBS
        public Account? SelectedAccount { get; set; }
        public bool VersionIsolation { get; set; } = true;
        public string Language { get; set; } = "en-US";
        public string? LastSelectedVersionId { get; set; }
    }

    public class ConfigService
    {
        private const string ConfigFileName = "config.json";
        private readonly string _configPath;
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
            catch (Exception)
            {
                // Handle logging here later
            }
        }
    }
}
‚ *cascade082Jfile:///c:/Users/Linyizhi/.gemini/GeminiLauncher/Services/ConfigService.cs