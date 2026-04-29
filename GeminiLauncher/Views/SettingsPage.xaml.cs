using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using GeminiLauncher.ViewModels;
using GeminiLauncher.Controls;

namespace GeminiLauncher.Views
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            // Use MainViewModel from Application
            this.DataContext = ((App)Application.Current).MainWindow.DataContext;
            
            // Initialize fields from Config
            if (DataContext is ViewModels.MainViewModel vm)
            {
                var cfg = vm.ConfigService.Settings;

                if (!string.IsNullOrEmpty(cfg.GamePath))
                    GamePathBox.Text = cfg.GamePath;

                if (!string.IsNullOrEmpty(cfg.JavaPath))
                {
                   JavaPathBox.Text = cfg.JavaPath;
                   UpdateJavaVersionInfo(cfg.JavaPath);
                }

                // Restore memory slider
                MemorySlider.Value = cfg.MaxRam > 0 ? cfg.MaxRam : 4096;

                // Restore version isolation toggle
                VersionIsolationToggle.IsChecked = cfg.VersionIsolation;

                // Restore language selection
                if (cfg.Language == "zh-CN")
                    ChineseRadio.IsChecked = true;
                else
                    EnglishRadio.IsChecked = true;

                // Restore Download Source
                foreach (ComboBoxItem item in DownloadSourceCombo.Items)
                {
                    if (item.Tag?.ToString() == cfg.DownloadSource)
                    {
                        DownloadSourceCombo.SelectedItem = item;
                        break;
                    }
                }

                // Restore Hidden Pages
                if (cfg.HiddenPageKeys.Contains("download")) HideDownloadCheck.IsChecked = true;
                if (cfg.HiddenPageKeys.Contains("settings")) HideSettingsCheck.IsChecked = true;
            }
            
            // Initialize memory slider value display
            UpdateMemoryValueText();

            // Initialize PCL-style memory settings
            UpdateSystemMemoryInfo();
            UpdateMemorySlidersState();
        }

        private void AddOfflineAccount_Click(object sender, RoutedEventArgs e)
        {
            var username = OfflineUsernameBox.Text;
            if (!string.IsNullOrWhiteSpace(username))
            {
                var vm = DataContext as ViewModels.MainViewModel;
                vm?.AccountManager.LoginOffline(username);
                OfflineUsernameBox.Text = string.Empty;
            }
        }

        private async void AddMicrosoftAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vm = DataContext as MainViewModel;
                if (vm == null) return;
                await vm.AccountManager.LoginMicrosoft();
            }
            catch (System.Exception ex)
            {
                iOS26Dialog.Show($"Login Failed: {ex.Message}", "错误", DialogIcon.Error);
            }
        }

        private void RemoveAccount_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is GeminiLauncher.Models.Account account)
            {
                if (iOS26Dialog.Show($"确定要删除账号 '{account.Username}' 吗？",
                    "确认删除", DialogIcon.Warning, DialogButtons.YesNo) == true)
                {
                    if (DataContext is MainViewModel vm)
                    {
                        vm.AccountManager.RemoveAccount(account);
                    }
                }
            }
        }

        private void BrowseGamePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select Minecraft folder (choose any file in the folder)",
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Folder Selection"
            };

            if (dialog.ShowDialog() == true)
            {
                var folderPath = Path.GetDirectoryName(dialog.FileName);
                if (!string.IsNullOrEmpty(folderPath))
                {
                    GamePathBox.Text = folderPath;
                    if (DataContext is ViewModels.MainViewModel vm)
                    {
                        vm.ConfigService.Settings.GamePath = folderPath;
                        vm.ConfigService.SaveConfig();
                    }
                }
            }
        }

        private void BrowseJavaPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Java Executable (javaw.exe;java.exe)|javaw.exe;java.exe|All Files (*.*)|*.*",
                Title = "Select Java Executable"
            };

            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                JavaPathBox.Text = path;
                UpdateJavaVersionInfo(path);
                
                if (DataContext is ViewModels.MainViewModel vm)
                {
                    vm.ConfigService.Settings.JavaPath = path;
                    vm.ConfigService.SaveConfig();
                }
            }
        }

        private void JavaAutoSearchCombo_DropDownOpened(object? sender, EventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null) return;

            var javaService = new GeminiLauncher.Services.JavaService();
            javaService.ClearCache();
            var installations = javaService.FindInstallations();

            combo.Items.Clear();
            if (installations.Any())
            {
                foreach (var inst in installations)
                {
                    string bitTag = inst.Is64Bit ? "64-bit" : "32-bit";
                    var item = new ComboBoxItem
                    {
                        Content = $"{inst.Version} ({bitTag}) — {inst.Path}",
                        Tag = inst
                    };
                    combo.Items.Add(item);
                }
            }
            else
            {
                var item = new ComboBoxItem
                {
                    Content = "未找到 Java 安装",
                    IsEnabled = false
                };
                combo.Items.Add(item);
            }
        }

        private void JavaAutoSearchCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo?.SelectedItem is not ComboBoxItem item) return;
            if (item.Tag is not GeminiLauncher.Services.JavaInstallation inst) return;

            JavaPathBox.Text = inst.Path;
            UpdateJavaVersionInfo(inst.Path);

            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.ConfigService.Settings.JavaPath = inst.Path;
                vm.ConfigService.SaveConfig();
            }
        }

        private void UpdateJavaVersionInfo(string javaPath)
        {
            try
            {
                var javaService = new GeminiLauncher.Services.JavaService();
                var info = javaService.GetJavaInfo(javaPath);
                if (info != null)
                {
                    string bitTag = info.Is64Bit ? "64-bit" : "32-bit";
                    JavaVersionText.Text = $"✓ {info.Version} ({bitTag})";
                }
                else
                {
                    JavaVersionText.Text = "✓ Java detected";
                }

                JavaVersionText.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00E676")!);
            }
            catch
            {
                JavaVersionText.Text = "⚠️ Unable to detect version";
                JavaVersionText.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFC107")!);
            }
        }

        private void MemorySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateMemoryValueText();

            // Persist memory setting
            if (IsLoaded && DataContext is ViewModels.MainViewModel vm)
            {
                vm.ConfigService.Settings.MaxRam = (int)MemorySlider.Value;
                vm.ConfigService.SaveConfig();
            }
        }

        private void UpdateMemoryValueText()
        {
            if (MemoryValueText != null && MemorySlider != null)
            {
                int valueMB = (int)MemorySlider.Value;
                MemoryValueText.Text = $"{valueMB} MB";

                // Smart memory warning
                if (MemoryWarningText != null)
                {
                    try
                    {
                        var gcInfo = System.GC.GetGCMemoryInfo();
                        long totalPhysicalMB = gcInfo.TotalAvailableMemoryBytes / (1024 * 1024);
                        double ratio = (double)valueMB / totalPhysicalMB;

                        if (valueMB < 1024)
                        {
                            MemoryWarningText.Text = "⚠️ 分配过低，可能导致游戏崩溃";
                            MemoryWarningText.Foreground = new System.Windows.Media.SolidColorBrush(
                                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF5252")!);
                        }
                        else if (ratio > 0.8)
                        {
                            MemoryWarningText.Text = $"⚠️ 超过物理内存 80%（{totalPhysicalMB} MB），可能导致卡死";
                            MemoryWarningText.Foreground = new System.Windows.Media.SolidColorBrush(
                                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF5252")!);
                        }
                        else
                        {
                            MemoryWarningText.Text = "✓ 推荐范围";
                            MemoryWarningText.Foreground = new System.Windows.Media.SolidColorBrush(
                                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00E676")!);
                        }
                    }
                    catch
                    {
                        MemoryWarningText.Text = "";
                    }
                }
            }
        }

        private async void VersionIsolationToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.ConfigService.Settings.VersionIsolation = VersionIsolationToggle.IsChecked == true;
                vm.ConfigService.SaveConfig();
                
                // Reload versions to update game directories
                await vm.LoadVersionsAsync();
            }
        }

        private void Language_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            
            if (DataContext is ViewModels.MainViewModel vm)
            {
                if (EnglishRadio.IsChecked == true)
                {
                    App.SwitchLanguage("en-US");
                    vm.ConfigService.Settings.Language = "en-US";
                }
                else if (ChineseRadio.IsChecked == true)
                {
                    App.SwitchLanguage("zh-CN");
                    vm.ConfigService.Settings.Language = "zh-CN";
                }
                vm.ConfigService.SaveConfig();
            }
        }

        private void BrowseBackgroundImage_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.PickBackgroundImageCommand.Execute(null);
            }
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded) return;
            if (DataContext is ViewModels.MainViewModel vm)
            {
                // ViewModel property bound TwoWay, just need to sync to config and save
                vm.ConfigService.Settings.BackgroundOpacity = vm.BackgroundOpacity;
                vm.ConfigService.SaveConfig();
            }
        }

        private void BlurSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded) return;
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.ConfigService.Settings.BlurEffectRadius = vm.BlurEffectRadius;
                vm.ConfigService.SaveConfig();
            }
        }

        private void LauncherVisibility_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.ConfigService.SaveConfig();
            }
        }

        private void ProcessPriority_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.ConfigService.SaveConfig();
            }
        }
        private void DownloadSource_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (DataContext is ViewModels.MainViewModel vm && DownloadSourceCombo.SelectedItem is ComboBoxItem item)
            {
                vm.ConfigService.Settings.DownloadSource = item.Tag?.ToString() ?? "Official";
                vm.ConfigService.SaveConfig();
            }
        }

        private void MaxThreads_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded) return;
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.ConfigService.SaveConfig();
            }
        }
        private void AdvancedSetting_Changed(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.ConfigService.SaveConfig();
            }
        }

        private void HidePage_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            if (DataContext is ViewModels.MainViewModel vm && sender is CheckBox chk && chk.Tag is string key)
            {
                if (chk.IsChecked == true)
                {
                    if (!vm.ConfigService.Settings.HiddenPageKeys.Contains(key))
                        vm.ConfigService.Settings.HiddenPageKeys.Add(key);
                }
                else
                {
                    vm.ConfigService.Settings.HiddenPageKeys.Remove(key);
                }

                vm.ConfigService.SaveConfig();

                // Notify MainWindow to refresh visibility
                if (Application.Current.MainWindow is MainWindow window)
                {
                    window.RefreshNavigationVisibility();
                }
            }
        }

        private void AutoDetectMemory_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.ConfigService.SaveConfig();
                UpdateSystemMemoryInfo();
                UpdateMemorySlidersState();
            }
        }

        private void MaxRam_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded) return;
            if (DataContext is ViewModels.MainViewModel vm)
            {
                int maxRam = (int)MaxRamSlider.Value;
                int minRam = (int)MinRamSlider.Value;

                // Ensure min <= max
                if (minRam > maxRam)
                {
                    minRam = Math.Min(512, maxRam / 4);
                    MinRamSlider.Value = minRam;
                }

                vm.ConfigService.Settings.MaxRam = maxRam;
                vm.ConfigService.SaveConfig();
                UpdateSystemMemoryInfo();
            }
        }

        private void MinRam_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded) return;
            if (DataContext is ViewModels.MainViewModel vm)
            {
                int maxRam = (int)MaxRamSlider.Value;
                int minRam = (int)MinRamSlider.Value;

                // Ensure min <= max
                if (minRam > maxRam)
                {
                    minRam = maxRam;
                    MinRamSlider.Value = minRam;
                }

                vm.ConfigService.Settings.MinRam = minRam;
                vm.ConfigService.SaveConfig();
            }
        }

        private void UpdateSystemMemoryInfo()
        {
            try
            {
                long totalMem = GeminiLauncher.Services.LaunchService.GetTotalSystemMemoryMB();
                long recommended = totalMem * 50 / 100;
                recommended = Math.Max(1024, Math.Min(recommended, totalMem - 1024));

                SystemMemoryInfoText.Text = $"系统: {totalMem} MB | 推荐: {recommended} MB";
            }
            catch
            {
                SystemMemoryInfoText.Text = "";
            }
        }

        private void UpdateMemorySlidersState()
        {
            bool autoDetect = AutoDetectMemoryToggle.IsChecked == true;

            // When auto-detect is enabled, sliders are still visible but show recommended values
            // Users can still adjust them as upper limits
            MaxRamSlider.IsEnabled = true;
            MinRamSlider.IsEnabled = true;

            if (autoDetect)
            {
                try
                {
                    long totalMem = GeminiLauncher.Services.LaunchService.GetTotalSystemMemoryMB();
                    long recommended = totalMem * 50 / 100;
                    recommended = Math.Max(1024, Math.Min(recommended, totalMem - 1024));

                    // Set slider maximum to system memory
                    MaxRamSlider.Maximum = totalMem;

                    // If current value is 0 or default, set to recommended
                    if (MaxRamSlider.Value <= 0 || MaxRamSlider.Value == 4096)
                    {
                        MaxRamSlider.Value = (int)Math.Max(2048, recommended);
                    }
                }
                catch { }
            }
            else
            {
                MaxRamSlider.Maximum = 32768;
            }
        }
    }
}
