using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeminiLauncher.Models;
using GeminiLauncher.Services.Network;
using GeminiLauncher.Services;

namespace GeminiLauncher.ViewModels
{
    public partial class DownloadViewModel : ObservableObject
    {
        private readonly VersionManifestService _manifestService;
        private ObservableCollection<DownloadableVersion> _allVersions;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedFilter = "Release";

        // Download Sources
        public ObservableCollection<string> DownloadSources { get; } = new ObservableCollection<string>(VersionManifestService.AvailableSources);

        [ObservableProperty]
        private string _selectedSource = "BMCLAPI";

        public ICollectionView VersionsView { get; private set; }

        public DownloadViewModel()
        {
            _manifestService = new VersionManifestService();
            _allVersions = new ObservableCollection<DownloadableVersion>();
            VersionsView = CollectionViewSource.GetDefaultView(_allVersions);
            VersionsView.Filter = FilterVersions;

            if (PreloadService.IsPreloaded && PreloadService.CachedVersionList.Count > 0)
            {
                foreach (var v in PreloadService.CachedVersionList)
                    _allVersions.Add(v);
                VersionsView.Refresh();
                IsLoading = false;
            }
            else
            {
                LoadVersionsCommand.Execute(null);
            }
        }

        partial void OnSelectedSourceChanged(string value)
        {
            // Reload versions when source changes (if we were supporting dynamic switching of manifest source)
            // For now, VersionManifestService logic might need update or we just pass it
            LoadVersionsCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadVersions()
        {
            IsLoading = true;
            try
            {
                // Pass source to service
                var versions = await _manifestService.GetVersionsAsync(SelectedSource);
                
                _allVersions.Clear();
                foreach (var v in versions)
                {
                    _allVersions.Add(v);
                }
                VersionsView.Refresh();
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            VersionsView.Refresh();
        }

        partial void OnSelectedFilterChanged(string value)
        {
            VersionsView.Refresh();
        }

        private bool FilterVersions(object obj)
        {
            if (obj is not DownloadableVersion version) return false;

            // 1. Type Filter
            bool typeMatch = SelectedFilter switch
            {
                "Release" => version.Type == "release",
                "Snapshot" => version.Type == "snapshot",
                "Old" => version.Type.StartsWith("old_"),
                _ => true
            };

            if (!typeMatch) return false;

            // 2. Search Filter
            if (string.IsNullOrWhiteSpace(SearchText)) return true;

            return version.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }

        [RelayCommand]
        private void SwitchFilter(string filter)
        {
            SelectedFilter = filter;
        }

        [RelayCommand]
        private void DownloadVersion(DownloadableVersion version)
        {
            if (version == null) return;

            if (Application.Current.MainWindow is MainWindow mw)
                mw.RootFrame.Navigate(new GeminiLauncher.Views.LoaderSelectionPage(version));
        }

        private string GetString(string key)
        {
            if (Application.Current.TryFindResource(key) is string s)
            {
                return s;
            }
            return $"[{key}]";
        }

        /// <summary>
        /// Shows an action sheet for selecting a loader. Returns "Vanilla", "Fabric", "NeoForge", or null if cancelled.
        /// </summary>
        private string? ShowLoaderActionSheet(string versionId)
        {
            var dlg = new Window
            {
                Title = "选择安装方式",
                Width = 400, Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                ResizeMode = ResizeMode.NoResize
            };

            string? result = null;

            var outerBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8202028")!),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(24),
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 40, ShadowDepth = 0, Opacity = 0.5 }
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = $"下载 {versionId}",
                FontSize = 20, FontWeight = FontWeights.Bold,
                Foreground = Brushes.White, Margin = new Thickness(0, 0, 0, 4)
            });
            stack.Children.Add(new TextBlock
            {
                Text = "选择安装方式",
                FontSize = 13, Opacity = 0.6,
                Foreground = Brushes.White, Margin = new Thickness(0, 0, 0, 16)
            });

            Button MakeOption(string emoji, string label, string sub, string value, bool recommended = false)
            {
                var btn = new Button
                {
                    Margin = new Thickness(0, 0, 0, 8),
                    Padding = new Thickness(16, 12, 16, 12),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(recommended ? "#2000E676" : "#20FFFFFF")!),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(recommended ? "#5000E676" : "#30FFFFFF")!),
                    BorderThickness = new Thickness(1),
                    Foreground = Brushes.White,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    HorizontalContentAlignment = HorizontalAlignment.Left
                };
                btn.Resources.Add(typeof(Border), new Style(typeof(Border)) { Setters = { new Setter(Border.CornerRadiusProperty, new CornerRadius(12)) } });

                var sp = new StackPanel { Orientation = Orientation.Horizontal };
                sp.Children.Add(new TextBlock { Text = emoji, FontSize = 18, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0) });
                var textSp = new StackPanel();
                var headerSp = new StackPanel { Orientation = Orientation.Horizontal };
                headerSp.Children.Add(new TextBlock { Text = label, FontSize = 15, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White });
                if (recommended)
                    headerSp.Children.Add(new TextBlock { Text = " 推荐", FontSize = 11, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00E676")!), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) });
                textSp.Children.Add(headerSp);
                textSp.Children.Add(new TextBlock { Text = sub, FontSize = 11, Opacity = 0.6, Foreground = Brushes.White });
                sp.Children.Add(textSp);
                btn.Content = sp;

                btn.Click += (s, e) => { result = value; dlg.DialogResult = true; };
                return btn;
            }

            stack.Children.Add(MakeOption("☁️", "仅原版 (Vanilla)", "纯净原版，无任何加载器", "Vanilla"));
            stack.Children.Add(MakeOption("🧵", "原版 + Fabric", "轻量级 Mod 加载器，最流行的选择", "Fabric", true));
            stack.Children.Add(MakeOption("⚒️", "原版 + NeoForge", "功能强大的 Mod 加载器", "NeoForge"));

            // Cancel button
            var cancelBtn = new Button
            {
                Content = "取消", Margin = new Thickness(0, 8, 0, 0),
                Padding = new Thickness(0, 8, 0, 8),
                Background = Brushes.Transparent, Foreground = Brushes.White,
                BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                FontSize = 14, Opacity = 0.7
            };
            cancelBtn.Click += (s, e) => { dlg.DialogResult = false; };
            stack.Children.Add(cancelBtn);

            outerBorder.Child = stack;
            dlg.Content = outerBorder;

            return dlg.ShowDialog() == true ? result : null;
        }
    }
}
