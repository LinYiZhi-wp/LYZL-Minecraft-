�Uusing System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using GeminiLauncher.Models;
using GeminiLauncher.Services;
using Microsoft.Win32;

namespace GeminiLauncher.Views
{
    public partial class VersionSelectorPage : Page
    {
        private VersionDetectionService _detectionService;
        private List<GameDirectory> _directories;
        private List<GameVersion> _allVersions;
        private GameVersion? _selectedVersion;
        private Action<GameVersion> _onVersionSelected;

        public VersionSelectorPage(Action<GameVersion> onVersionSelected)
        {
            InitializeComponent();
            _onVersionSelected = onVersionSelected;
            _detectionService = new VersionDetectionService();
            _directories = new List<GameDirectory>();
            _allVersions = new List<GameVersion>();
            
            LoadDirectories();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                // Fallback: Navigate specifically to HomePage if history is empty
                NavigationService.Navigate(new HomePage());
            }
        }

        private void LoadDirectories()
        {
            // Auto detect
            _directories = _detectionService.DetectGameDirectories();
            FoldersListView.ItemsSource = _directories;
            
            if (_directories.Count > 0)
            {
                FoldersListView.SelectedIndex = 0;
            }
        }

        private void FoldersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FoldersListView.SelectedItem is GameDirectory selectedDir)
            {
                LoadVersionsForDirectory(selectedDir.Path);
            }
        }

        private void LoadVersionsForDirectory(string gamePath)
        {
            _allVersions = _detectionService.DetectVersions(gamePath);
            DisplayVersionsByCategory();
        }

        private void DisplayVersionsByCategory()
        {
            VersionsPanel.Children.Clear();

            var moddableVersions = _allVersions.Where(v => v.Category == VersionCategory.Moddable).ToList();
            var vanillaVersions = _allVersions.Where(v => v.Category == VersionCategory.Vanilla).ToList();
            var brokenVersions = _allVersions.Where(v => v.Category == VersionCategory.Broken).ToList();

            if (moddableVersions.Count > 0) AddVersionCategory($"可装 Mod ({moddableVersions.Count})", moddableVersions);
            if (vanillaVersions.Count > 0) AddVersionCategory($"常规版本 ({vanillaVersions.Count})", vanillaVersions);
            if (brokenVersions.Count > 0) AddVersionCategory($"错误的版本 ({brokenVersions.Count})", brokenVersions);

            if (_allVersions.Count == 0)
            {
                var noVersionsText = new TextBlock
                {
                    Text = "未检测到任何版本\n请先下载或安装游戏版本",
                    FontSize = 14, Opacity = 0.6, Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center, TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                };
                VersionsPanel.Children.Add(noVersionsText);
            }
        }

        private void AddVersionCategory(string categoryName, List<GameVersion> versions)
        {
            var categoryHeader = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(15, 12, 15, 12),
                Margin = new Thickness(0, 10, 0, 10)
            };
            categoryHeader.Child = new TextBlock
            {
                Text = categoryName, FontSize = 14, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White
            };
            VersionsPanel.Children.Add(categoryHeader);

            foreach (var version in versions)
            {
                VersionsPanel.Children.Add(CreateVersionItem(version));
            }
        }

        private Border CreateVersionItem(GameVersion version)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15, 12, 15, 12),
                Margin = new Thickness(0, 0, 0, 8),
                Tag = version,
                Cursor = Cursors.Hand
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock
            {
                Text = $"{version.Icon} {version.DisplayName}",
                FontSize = 15, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 5)
            });

            string details = version.Type == VersionType.Release ? "正式版" : "快照版";
            if (!string.IsNullOrEmpty(version.MinecraftVersion)) details += $" {version.MinecraftVersion}";
            if (version.Loader != null) details += $", {version.Loader} {version.LoaderVersion}";

            stackPanel.Children.Add(new TextBlock
            {
                Text = details, FontSize = 12, Opacity = 0.7, Foreground = Brushes.White
            });

            Grid.SetColumn(stackPanel, 0);
            grid.Children.Add(stackPanel);

            // Settings Button
            var settingsButton = new Button
            {
                Content = "⚙️", FontSize = 18, Width = 35, Height = 35,
                Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                Foreground = Brushes.White, BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand, Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            settingsButton.Click += (s, e) =>
            {
                e.Handled = true;
                OpenVersionSettings(version);
            };
            Grid.SetColumn(settingsButton, 1);
            grid.Children.Add(settingsButton);

            border.Child = grid;

            // Click handling
            border.MouseLeftButtonDown += (s, e) =>
            {
                 // Highlight
                 HighlightSelectedVersion(border);
                 _selectedVersion = version;
                 
                 // Double click check
                 if (e.ClickCount == 2)
                 {
                     ConfirmSelection();
                 }
            };
            
            return border;
        }

        private void OpenVersionSettings(GameVersion version)
        {
            var gameInstance = new GameInstance
            {
                Id = version.Id,
                RootPath = version.GamePath,
                GameDir = version.GamePath, 
                Type = version.Type.ToString().ToLower()
            };
            NavigationService.Navigate(new VersionSettingsPage(gameInstance));
        }

        private void HighlightSelectedVersion(Border selectedBorder)
        {
            foreach (var child in VersionsPanel.Children)
            {
                if (child is Border border && border.Tag is GameVersion)
                {
                    border.Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
                }
            }
            selectedBorder.Background = new SolidColorBrush(Color.FromArgb(40, 0, 230, 118)); 
        }

        private void ConfirmSelection()
        {
            if (_selectedVersion != null)
            {
                _onVersionSelected?.Invoke(_selectedVersion);
                if (NavigationService.CanGoBack) NavigationService.GoBack();
            }
        }

        private void ToggleFolderPanel_Click(object sender, RoutedEventArgs e)
        {
            if (FolderPanel.Visibility == Visibility.Collapsed)
            {
                FolderPanel.Visibility = Visibility.Visible;
                ToggleFolderBtn.Content = "📁 隐藏";
            }
            else
            {
                FolderPanel.Visibility = Visibility.Collapsed;
                ToggleFolderBtn.Content = "📁 文件夹";
            }
        }

        private void AddFolder_Click(object sender, RoutedEventArgs e)
        {
             var dialog = new OpenFileDialog
            {
                Title = "选择.minecraft文件夹中的任意文件",
                Filter = "All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                string? selectedPath = System.IO.Path.GetDirectoryName(dialog.FileName);
                if (string.IsNullOrEmpty(selectedPath)) return;

                while (!string.IsNullOrEmpty(selectedPath) && 
                       !System.IO.Directory.Exists(System.IO.Path.Combine(selectedPath, "versions")))
                {
                    selectedPath = System.IO.Path.GetDirectoryName(selectedPath);
                }

                if (!string.IsNullOrEmpty(selectedPath) && 
                    System.IO.Directory.Exists(System.IO.Path.Combine(selectedPath, "versions")))
                {
                    var newDir = new GameDirectory
                    {
                        Name = System.IO.Path.GetFileName(selectedPath),
                        Path = selectedPath,
                        IsDefault = false,
                        Source = DirectorySource.Manual
                    };
                    
                    _directories.Add(newDir);
                    FoldersListView.ItemsSource = null;
                    FoldersListView.ItemsSource = _directories;
                    FoldersListView.SelectedItem = newDir;
                }
                else
                {
                    MessageBox.Show("未找到有效的游戏目录\n请选择.minecraft文件夹", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
�
 *cascade08�
�*cascade08��U *cascade082Rfile:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Views/VersionSelectorPage.xaml.cs