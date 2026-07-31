using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GeminiLauncher.Models.Ecosystem
{
    public enum DownloadSource
    {
        Official,
        Modrinth,
        BMCLAPI,
        FastMirror
    }

    public class ResourceDetail : ObservableObject
    {
        private string _id = string.Empty;
        private string _name = string.Empty;
        private string _summary = string.Empty;
        private string _description = string.Empty;
        private string _iconUrl = string.Empty;
        private System.Windows.Media.Imaging.BitmapImage? _iconImage;
        private string _author = string.Empty;
        private long _downloads;
        private int _followers;
        private DateTime _dateCreated;
        private DateTime _dateModified;
        private string _license = string.Empty;
        private ProjectPlatform _platform;
        private ProjectType _type;
        private string _webUrl = string.Empty;
        private List<string> _categories = new();
        private List<string> _gameVersions = new();
        private List<string> _loaders = new();
        private List<ModFile> _versions = new();
        private List<string> _galleryImages = new();
        private bool _isLoadingDetails;
        private bool _isDownloading;
        private double _downloadProgress;
        private string _downloadStatus = "Ready";
        private string _downloadSpeedText = "";
        private long _downloadedBytes;
        private long _totalBytes;
        private ModFile? _selectedVersion;
        private DownloadSource _preferredSource = DownloadSource.Modrinth;
        private bool _isClientSideOnly;
        private bool _isServerSideOnly;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Summary { get => _summary; set => SetProperty(ref _summary, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        public string IconUrl { get => _iconUrl; set => SetProperty(ref _iconUrl, value); }
        public System.Windows.Media.Imaging.BitmapImage? IconImage { get => _iconImage; set => SetProperty(ref _iconImage, value); }
        public string Author { get => _author; set => SetProperty(ref _author, value); }
        public long Downloads { get => _downloads; set => SetProperty(ref _downloads, value); }
        public int Followers { get => _followers; set => SetProperty(ref _followers, value); }
        public DateTime DateCreated { get => _dateCreated; set => SetProperty(ref _dateCreated, value); }
        public DateTime DateModified { get => _dateModified; set => SetProperty(ref _dateModified, value); }
        public string License { get => _license; set => SetProperty(ref _license, value); }
        public ProjectPlatform Platform { get => _platform; set => SetProperty(ref _platform, value); }
        public ProjectType Type { get => _type; set => SetProperty(ref _type, value); }
        public string WebUrl { get => _webUrl; set => SetProperty(ref _webUrl, value); }
        public List<string> Categories { get => _categories; set => SetProperty(ref _categories, value); }
        public List<string> GameVersions { get => _gameVersions; set => SetProperty(ref _gameVersions, value); }
        public List<string> Loaders { get => _loaders; set => SetProperty(ref _loaders, value); }
        public List<ModFile> Versions { get => _versions; set => SetProperty(ref _versions, value); }
        public List<string> GalleryImages { get => _galleryImages; set => SetProperty(ref _galleryImages, value); }
        public bool IsLoadingDetails { get => _isLoadingDetails; set => SetProperty(ref _isLoadingDetails, value); }
        public bool IsDownloading { get => _isDownloading; set => SetProperty(ref _isDownloading, value); }
        public double DownloadProgress { get => _downloadProgress; set => SetProperty(ref _downloadProgress, value); }
        public string DownloadStatus { get => _downloadStatus; set => SetProperty(ref _downloadStatus, value); }
        public string DownloadSpeedText { get => _downloadSpeedText; set => SetProperty(ref _downloadSpeedText, value); }
        public long DownloadedBytes { get => _downloadedBytes; set => SetProperty(ref _downloadedBytes, value); }
        public long TotalBytes { get => _totalBytes; set => SetProperty(ref _totalBytes, value); }
        public ModFile? SelectedVersion { get => _selectedVersion; set => SetProperty(ref _selectedVersion, value); }
        public DownloadSource PreferredSource { get => _preferredSource; set => SetProperty(ref _preferredSource, value); }
        public bool IsClientSideOnly { get => _isClientSideOnly; set => SetProperty(ref _isClientSideOnly, value); }
        public bool IsServerSideOnly { get => _isServerSideOnly; set => SetProperty(ref _isServerSideOnly, value); }

        public string DownloadsFormatted
        {
            get
            {
                if (_downloads >= 1_000_000) return $"{_downloads / 1_000_000.0:F1}M";
                if (_downloads >= 1_000) return $"{_downloads / 1000.0:F1}K";
                return _downloads.ToString();
            }
        }

        public string FollowersFormatted
        {
            get
            {
                if (_followers >= 1000) return $"{_followers / 1000.0:F1}K";
                return _followers.ToString();
            }
        }

        public string SizeDisplay
        {
            get
            {
                if (_totalBytes < 1024) return $"{_totalBytes} B";
                if (_totalBytes < 1024 * 1024) return $"{_totalBytes / 1024.0:F1} KB";
                return $"{_totalBytes / (1024.0 * 1024.0):F1} MB";
            }
        }

        public string DownloadProgressText => $"{(_downloadProgress * 100):F0}%";

        public string TimeRemainingDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(_downloadSpeedText) || _downloadProgress <= 0 || _downloadProgress >= 1.0) return "";

                var speedMatch = System.Text.RegularExpressions.Regex.Match(_downloadSpeedText, @"[\d.]+");
                if (!speedMatch.Success) return "";

                if (!double.TryParse(speedMatch.Value, out double speedVal)) return "";
                double speedMBs = speedVal < 100 ? speedVal / 1024.0 : speedVal / (1024.0 * 1024.0);
                if (_downloadSpeedText.Contains("KB")) speedMBs = speedVal / 1024.0;
                else if (_downloadSpeedText.Contains("MB")) speedMBs = speedVal;
                else if (_downloadSpeedText.Contains("B/s") && !_downloadSpeedText.Contains("K") && !_downloadSpeedText.Contains("M")) speedMBs = speedVal / (1024.0 * 1024.0);

                if (speedMBs <= 0) return "--:--";

                long remainingBytes = (long)(_totalBytes * (1 - _downloadProgress));
                double secondsRemaining = remainingBytes / (1024.0 * 1024.0) / speedMBs;

                if (secondsRemaining < 60) return $"{(int)secondsRemaining}s";
                if (secondsRemaining < 3600) return $"{(int)(secondsRemaining / 60)}m {(int)(secondsRemaining % 60)}s";

                int hours = (int)(secondsRemaining / 3600);
                int mins = (int)((secondsRemaining % 3600) / 60);
                return $"{hours}h {mins}m";
            }
        }
    }
}
