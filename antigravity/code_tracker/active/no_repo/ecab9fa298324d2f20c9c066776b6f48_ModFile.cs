úusing System.Collections.Generic;

namespace GeminiLauncher.Models.Ecosystem
{
    public class ModFile
    {
        public string FileId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public Dictionary<string, string> Hashes { get; set; } = new Dictionary<string, string>();
        public List<string> Loaders { get; set; } = new List<string>(); // Forge, Fabric...
        public List<string> GameVersions { get; set; } = new List<string>();
        public long Size { get; set; }
        public string ReleaseDate { get; set; } = string.Empty;
        public List<ModDependency> Dependencies { get; set; } = new List<ModDependency>();
    }

    public class ModDependency
    {
        public string ProjectId { get; set; } = string.Empty;
        public string VersionId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string DependencyType { get; set; } = "required";
    }
}
ú*cascade082Lfile:///c:/Users/Linyizhi/.gemini/GeminiLauncher/Models/Ecosystem/ModFile.cs