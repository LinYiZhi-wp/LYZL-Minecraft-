using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GeminiLauncher.Models;
using GeminiLauncher.Services;
using Microsoft.Win32;

namespace GeminiLauncher.Views
{
    public partial class VersionSelectorDialog : Window
    {
        private VersionDetectionService _detectionService;
        private List<GameDirectory> _directories;
        private List<GameVersion> _allVersions;
        private GameVersion? _selectedVersion;

        public GameVersion? SelectedVersion => _selectedVersion;
        public string? SelectedGamePath { get; private set; }

        public VersionSelectorDialog()
        {
            InitializeComponent();
            _detectionService = new VersionDetectionService();
            _directories = new List<GameDirectory>();
            _allVersions = new List<GameVersion>();
            
            LoadDirectories();
        }

        private void LoadDirectories()
        {
            // 自动检测游戏目录
            _directories = _detectionService.DetectGameDirectories();
            
            FoldersListView.ItemsSource = _directories;
            
            // 默认选中第一个
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
            // 检测该目录下的所有版本
            _allVersions = _detectionService.DetectVersions(gamePath);
            SelectedGamePath = gamePath;
            
            // 按类别分组显示
            DisplayVersionsByCategory();
        }

        private void DisplayVersionsByCategory()
        {
            VersionsPanel.Children.Clear();

            // 分类
            var moddableVersions = _allVersions.Where(v => v.Category == VersionCategory.Moddable).ToList();
            var vanillaVersions = _allVersions.Where(v => v.Category == VersionCategory.Vanilla).ToList();
            var brokenVersions = _allVersions.Where(v => v.Category == VersionCategory.Broken).ToList();

            // 添加可装Mod分组
            if (moddableVersions.Count > 0)
            {
                AddVersionCategory($"可装 Mod ({moddableVersions.Count})", moddableVersions);
            }

            // 添加常规版本分组
            if (vanillaVersions.Count > 0)
            {
                AddVersionCategory($"常规版本 ({vanillaVersions.Count})", vanillaVersions);
            }

            // 添加错误版本分组
            if (brokenVersions.Count > 0)
            {
                AddVersionCategory($"错误的版本 ({brokenVersions.Count})", brokenVersions);
            }

            // 如果没有版本
            if (_allVersions.Count == 0)
            {
                var noVersionsText = new TextBlock
                {
                    Text = "未检测到任何版本\n请先下载或安装游戏版本",
                    FontSize = 14,
                    Opacity = 0.6,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                };
                VersionsPanel.Children.Add(noVersionsText);
            }
        }

        private void AddVersionCategory(string categoryName, List<GameVersion> versions)
        {
            // 分组标题
            var categoryHeader = new Border
            {
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(35, 255, 255, 255)),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(18, 10, 18, 10),
                Margin = new Thickness(0, 5, 0, 12),
                BorderBrush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(20, 255, 255, 255)),
                BorderThickness = new Thickness(1)
            };

            var headerText = new TextBlock
            {
                Text = categoryName,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = System.Windows.Media.Brushes.White
            };

            categoryHeader.Child = headerText;
            VersionsPanel.Children.Add(categoryHeader);

            // 版本列表
            foreach (var version in versions)
            {
                var versionItem = CreateVersionItem(version);
                VersionsPanel.Children.Add(versionItem);
            }
        }

        private Border CreateVersionItem(GameVersion version)
        {
            var border = new Border
            {
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(22, 255, 255, 255)),
                CornerRadius = new CornerRadius(24),
                Padding = new Thickness(15, 12, 15, 12),
                Margin = new Thickness(0, 0, 0, 10),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = version
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var stackPanel = new StackPanel();

            // 版本名称（带图标）
            var nameText = new TextBlock
            {
                Text = $"{version.Icon} {version.DisplayName}",
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 5)
            };
            stackPanel.Children.Add(nameText);

            // 详细信息
            string details = version.Type == VersionType.Release ? "正式版" : "快照版";
            if (!string.IsNullOrEmpty(version.MinecraftVersion))
            {
                details += $" {version.MinecraftVersion}";
            }
            if (version.Loader != null)
            {
                details += $", {version.Loader} {version.LoaderVersion}";
            }

            var detailsText = new TextBlock
            {
                Text = details,
                FontSize = 12,
                Opacity = 0.7,
                Foreground = System.Windows.Media.Brushes.White
            };
            stackPanel.Children.Add(detailsText);

            Grid.SetColumn(stackPanel, 0);
            grid.Children.Add(stackPanel);

            // 设置按钮，改为圆润背景Border避免大方块
            var settingsButton = new Border
            {
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(35, 255, 255, 255)),
                CornerRadius = new CornerRadius(20),
                Width = 40,
                Height = 40,
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Text = "⚙️",
                    FontSize = 18,
                    Foreground = System.Windows.Media.Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 2)
                }
            };
            settingsButton.MouseLeftButtonUp += (s, e) =>
            {
                e.Handled = true; // 阻止事件冒泡
                OpenVersionSettings(version);
            };
            
            // 悬浮高亮效果
            settingsButton.MouseEnter += (s, e) => settingsButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 255, 255, 255));
            settingsButton.MouseLeave += (s, e) => settingsButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(35, 255, 255, 255));

            Grid.SetColumn(settingsButton, 1);
            grid.Children.Add(settingsButton);

            border.Child = grid;

            // 点击背景区域选中
            border.MouseLeftButtonDown += (s, e) =>
            {
                e.Handled = true;
                _selectedVersion = version;
                HighlightSelectedVersion(border);
            };

            // 双击确认
            border.MouseLeftButtonUp += (s, e) =>
            {
                if (e.ClickCount == 2)
                {
                    _selectedVersion = version;
                    DialogResult = true;
                    Close();
                }
            };

            return border;
        }

        private void OpenVersionSettings(GameVersion version)
        {
            // Convert to GameInstance for the dialog
            var gameInstance = new GameInstance
            {
                Id = version.Id,
                RootPath = version.GamePath,
                GameDir = version.GamePath, // Assuming no isolation logic here for now
                Type = version.Type.ToString().ToLower()
            };

            // Close this dialog first
            Close();

            // Navigate Main Window
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.RootFrame.Navigate(new VersionSettingsPage(gameInstance));
            }
        }

        private void HighlightSelectedVersion(Border selectedBorder)
        {
            // 重置所有项的背景和边框
            foreach (var child in VersionsPanel.Children)
            {
                if (child is Border border && border.Tag is GameVersion)
                {
                    border.Background = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromArgb(22, 255, 255, 255));
                    border.BorderBrush = System.Windows.Media.Brushes.Transparent;
                    border.BorderThickness = new Thickness(0);
                }
            }

            // 高亮选中项 - 使用主色调边框和稍亮的背景
            selectedBorder.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(45, 255, 255, 255));
            selectedBorder.BorderBrush = (System.Windows.Media.SolidColorBrush)Application.Current.Resources["iOS26.Accent"];
            selectedBorder.BorderThickness = new Thickness(2.5);
        }

        private void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            // 使用 OpenFileDialog 让用户选择.minecraft文件夹内的任意文件，然后取父目录
            var dialog = new OpenFileDialog
            {
                Title = "选择.minecraft文件夹中的任意文件（或直接输入路径）",
                Filter = "All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                string? selectedPath = System.IO.Path.GetDirectoryName(dialog.FileName);
                
                if (string.IsNullOrEmpty(selectedPath))
                    return;

                // 检查是否在.minecraft目录下，如果不是，尝试向上查找
                while (!string.IsNullOrEmpty(selectedPath) && 
                       !System.IO.Directory.Exists(System.IO.Path.Combine(selectedPath, "versions")))
                {
                    selectedPath = System.IO.Path.GetDirectoryName(selectedPath);
                }

                if (!string.IsNullOrEmpty(selectedPath) && 
                    System.IO.Directory.Exists(System.IO.Path.Combine(selectedPath, "versions")))
                {
                    // 添加到列表
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

        private void ImportModpack_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("整合包导入功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedVersion != null)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("请先选择一个版本", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ToggleFolderPanel_Click(object sender, RoutedEventArgs e)
        {
            if (FolderPanel.Visibility == Visibility.Collapsed || FolderPanel.Width == 0)
            {
                var sb = (System.Windows.Media.Animation.Storyboard)this.Resources["ExpandFolderPanel"];
                sb.Begin(this);
                ToggleFolderBtn.Content = "📁 隐藏";
            }
            else
            {
                var sb = (System.Windows.Media.Animation.Storyboard)this.Resources["CollapseFolderPanel"];
                sb.Begin(this);
                ToggleFolderBtn.Content = "📁 文件夹";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
