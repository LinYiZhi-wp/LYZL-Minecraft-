using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace GeminiLauncher.Models.Ecosystem
{
    public enum ProjectPlatform
    {
        Modrinth,
        CurseForge
    }

    public enum ProjectType
    {
        Mod,
        Modpack,
        ResourcePack,
        Shader,
        DataPack
    }

    public class ModProject
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public BitmapImage? IconImage { get; set; }
        public string Author { get; set; } = string.Empty;
        public long Downloads { get; set; }
        public ProjectPlatform Platform { get; set; }
        public ProjectType Type { get; set; } = ProjectType.Mod;
        public string WebUrl { get; set; } = string.Empty;
    }
}
