using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GeminiLauncher.Models.Ecosystem
{
    public class ModFile : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string FileId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public Dictionary<string, string> Hashes { get; set; } = new Dictionary<string, string>();
        public List<string> Loaders { get; set; } = new List<string>();
        public List<string> GameVersions { get; set; } = new List<string>();
        public long Size { get; set; }
        public string ReleaseDate { get; set; } = string.Empty;
        public List<ModDependency> Dependencies { get; set; } = new List<ModDependency>();

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ModDependency
    {
        public string ProjectId { get; set; } = string.Empty;
        public string VersionId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string DependencyType { get; set; } = "required";
    }
}
