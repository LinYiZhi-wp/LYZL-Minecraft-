�
namespace GeminiLauncher.Models
{
    public class ExportOptions
    {
        public string ExportPath { get; set; } = string.Empty;
        public string ModpackName { get; set; } = string.Empty;
        public string ModpackVersion { get; set; } = string.Empty;
        
        // Export selection
        public bool IncludeGameCore { get; set; } = true;     // .minecraft/versions/{id}
        public bool IncludeGameSettings { get; set; } = true; // options.txt
        public bool IncludeSaves { get; set; } = false;       // saves/
        
        // PCL-style options
        public bool IncludeLauncher { get; set; } = true;     // Placeholder
        public bool IncludeLauncherSettings { get; set; } = true; // Placeholder
        
        // Implicitly include mods/resourcepacks/shaderpacks/screenshots if they exist in the game directory?
        // Or should I add explicit flags?
        // Based on UI text "游戏本体设置" usually implies options.txt.
        // "游戏本体" usually implies version jar/json + libraries + natives (maybe?) + mods (if modded profile).
        
        // Let's explicitly add flags for clarity in service logic
        public bool IncludeMods { get; set; } = true;
        public bool IncludeResourcePacks { get; set; } = true;
        public bool IncludeShaderPacks { get; set; } = true;
    }
}
�
*cascade082Hfile:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Models/ExportOptions.cs