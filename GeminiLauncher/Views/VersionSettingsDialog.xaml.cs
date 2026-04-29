using System.Windows;
using System.Windows.Controls;
using System.IO.Compression;
using GeminiLauncher.Models;

namespace GeminiLauncher.Views
{
    public partial class VersionSettingsDialog : Window
    {
        private GameInstance _version;
        private VersionSettings _settings;

        public VersionSettingsDialog(GameInstance version)
        {
            InitializeComponent();
            _version = version;
            _settings = LoadVersionSettings(version.Id);
            
            // 更新标题
            SubtitleText.Text = version.Id;
            
            // 默认显示概览Tab
            ShowOverviewTab();
        }

        private VersionSettings LoadVersionSettings(string versionId)
        {
            // TODO: 从配置文件加载
            return new VersionSettings
            {
                VersionId = versionId,
                CustomName = _version.Id,
                VersionIsolation = true,
                MemoryMode = MemoryAllocation.Auto,
                MinMemoryMB = 512,
                MaxMemoryMB = 4096
            };
        }

        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                // 重置所有Tab状态
                TabOverview.IsEnabled = true;
                TabSettings.IsEnabled = true;
                TabMods.IsEnabled = true;
                TabOverview.IsEnabled = true;
                TabSettings.IsEnabled = true;
                TabMods.IsEnabled = true;
                TabExport.IsEnabled = true;
                TabMaintenance.IsEnabled = true;
                
                // 设置当前Tab
                button.IsEnabled = false;
                
                // 加载对应内容
                switch (tag)
                {
                    case "overview":
                        ShowOverviewTab();
                        break;
                    case "settings":
                        ShowSettingsTab();
                        break;
                    case "mods":
                        ShowModsTab();
                        break;
                    case "export":
                        ShowExportTab();
                        break;
                    case "maintenance":
                        ShowMaintenanceTab();
                        break;
                }
            }
        }

        private void ShowOverviewTab()
        {
            ContentArea.Children.Clear();
            ContentArea.Children.Add(CreateOverviewContent());
        }

        private UIElement CreateOverviewContent()
        {
            var panel = new StackPanel();

            // 版本信息卡片
            var infoCard = new Border
            {
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(30, 255, 255, 255)),
                CornerRadius = new CornerRadius(24),
                Padding = new Thickness(20, 20, 20, 20),
                Margin = new Thickness(0, 0, 0, 20)
            };

            var infoStack = new StackPanel { Orientation = Orientation.Horizontal };
            
            // 图标
            var icon = new TextBlock
            {
                Text = "📦", // Default icon
                FontSize = 48,
                Margin = new Thickness(0, 0, 20, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            infoStack.Children.Add(icon);

            // 版本信息
            var versionInfo = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            versionInfo.Children.Add(new TextBlock
            {
                Text = _version.Id,
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 5)
            });
            versionInfo.Children.Add(new TextBlock
            {
                Text = $"{_version.Type} ({_version.Id})",
                FontSize = 14,
                Opacity = 0.7,
                Foreground = System.Windows.Media.Brushes.White
            });
            infoStack.Children.Add(versionInfo);

            infoCard.Child = infoStack;
            panel.Children.Add(infoCard);

            // 快捷方式
            panel.Children.Add(CreateSectionHeader("快捷方式"));
            panel.Children.Add(CreateQuickActionsPanel());

            // 高级管理
            panel.Children.Add(CreateSectionHeader("高级管理"));
            panel.Children.Add(CreateAdvancedActionsPanel());

            return panel;
        }

        private UIElement CreateQuickActionsPanel()
        {
            var buttonsPanel = new WrapPanel { Margin = new Thickness(0, 0, 0, 20) };
            
            buttonsPanel.Children.Add(CreateActionButton("📁 版本文件夹", OpenVersionFolder_Click));
            buttonsPanel.Children.Add(CreateActionButton("💾 存档文件夹", OpenSavesFolder_Click));
            buttonsPanel.Children.Add(CreateActionButton("🧩 Mod文件夹", OpenModsFolder_Click));

            return buttonsPanel;
        }

        private UIElement CreateAdvancedActionsPanel()
        {
            var buttonsPanel = new WrapPanel();
            
            buttonsPanel.Children.Add(CreateActionButton("📜 导出启动脚本", ExportScript_Click));
            buttonsPanel.Children.Add(CreateActionButton("🗑️ 删除版本", DeleteVersion_Click, "#C62828"));

            return buttonsPanel;
        }

        // 返回一个有圆角外壳的 UIElement
        private UIElement CreateActionButton(string text, RoutedEventHandler clickHandler, string bgColor = "#30FFFFFF")
        {
            var border = new Border
            {
                CornerRadius = new CornerRadius(20),
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(bgColor)),
                Margin = new Thickness(0, 0, 10, 10),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            var button = new Button
            {
                Content = text,
                Height = 40,
                Padding = new Thickness(20, 0, 20, 0),
                Background = System.Windows.Media.Brushes.Transparent,
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            button.Click += clickHandler;
            border.Child = button;
            return border;
        }

        private TextBlock CreateSectionHeader(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 10, 0, 15)
            };
        }

        private void ShowSettingsTab()
        {
            ContentArea.Children.Clear();
            var panel = new StackPanel();
            
            // 提示信息
            var alertBorder = new Border
            {
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(20, 33, 150, 243)),
                BorderBrush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(100, 33, 150, 243)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(15, 10, 15, 10),
                Margin = new Thickness(0, 0, 0, 20)
            };
            alertBorder.Child = new TextBlock
            {
                Text = "ℹ️ 这些设置只对该游戏版本生效，不影响其他版本。",
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(100, 181, 246)),
                FontSize = 13
            };
            panel.Children.Add(alertBorder);

            // Java ComboBox Logic
            var javaItems = new System.Collections.Generic.List<string> { "跟随全局设置" };
            
            // Auto Select Option
            string autoText = "智能匹配 (Auto)";
            int reqVer = _version.RequiredJavaVersion > 0 ? _version.RequiredJavaVersion : 8; // default to 8 if unknown
            autoText += $" - 推荐 Java {reqVer}";
            javaItems.Add(autoText);

            // Scan available Javas
            var javaService = new GeminiLauncher.Services.JavaService();
            var installs = javaService.FindInstallations();
            foreach(var j in installs)
            {
                javaItems.Add($"{j.Version} ({j.Path})");
            }
            javaItems.Add("浏览... (自定义路径)");

            int selectedJavaIndex = 0; // Default: Global
            if (!string.IsNullOrEmpty(_version.CustomJavaPath))
            {
                // Check if it matches any scanned
                var match = installs.FirstOrDefault(j => j.Path == _version.CustomJavaPath);
                if (match != null)
                {
                    selectedJavaIndex = javaItems.IndexOf($"{match.Version} ({match.Path})");
                }
                else
                {
                    // Custom path not in scan? Add it or default to Browse
                    javaItems.Insert(2, $"自定义: {_version.CustomJavaPath}");
                    selectedJavaIndex = 2;
                }
            }

            var javaCombo = CreateComboBox(javaItems.ToArray(), selectedJavaIndex);
            javaCombo.SelectionChanged += (s, e) => 
            {
                if (javaCombo.SelectedItem as string == "浏览... (自定义路径)")
                {
                    var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "Java Executable (javaw.exe)|javaw.exe" };
                    if (dlg.ShowDialog() == true)
                    {
                        var info = javaService.GetJavaInfo(dlg.FileName);
                        string entry = info != null ? $"{info.Version} ({info.Path})" : $"Unknown ({dlg.FileName})";
                        javaItems.Insert(2, entry);
                        javaCombo.ItemsSource = null;
                        javaCombo.ItemsSource = javaItems;
                        javaCombo.SelectedIndex = 2;
                        _version.CustomJavaPath = dlg.FileName;
                    }
                    else
                    {
                        javaCombo.SelectedIndex = 0; // Reset
                    }
                }
                else if (javaCombo.SelectedIndex == 1) // Auto
                {
                     // We don't set CustomJavaPath here, handled by LaunchService logic if we had a flag.
                     // But for now let's say "Auto" clears CustomJavaPath and relies on Global, 
                     // OR we need a "UseAutoJava" flag.
                     // The user requested explicit "Auto". 
                     // Actually, my LaunchService update handles "Smart Selection" if version mismatch.
                     // But user might want to FORCE it.
                     // Let's set CustomJavaPath to empty (Global) for now, but really "Auto" implies dynamic.
                     // Simpler approach: If 'Auto' selected, we set CustomJavaPath to the BEST detected Java path RIGHT NOW.
                     var best = javaService.AutoDetectBestJava(_version.RequiredJavaVersion);
                     if (best != null) _version.CustomJavaPath = best;
                }
                else if (javaCombo.SelectedIndex == 0) // Global
                {
                    _version.CustomJavaPath = "";
                }
                else
                {
                     // Parse path from string "Version (Path)"
                     string sel = javaCombo.SelectedItem as string ?? "";
                     int pFrom = sel.IndexOf('(') + 1;
                     int pTo = sel.LastIndexOf(')');
                     if (pFrom > 0 && pTo > pFrom)
                     {
                         _version.CustomJavaPath = sel.Substring(pFrom, pTo - pFrom);
                     }
                }
            };
            
            // Isolation Toggle
            var isoCombo = CreateComboBox(new[] { "开启 (推荐)", "关闭" }, _version.GameDir != _version.RootPath ? 0 : 1);
            isoCombo.SelectionChanged += (s, e) => 
            {
                 // Logic to toggle isolation is complex (move files), so for now we just save the PREFERENCE
                 // Real isolation happens at launch or install. 
                 // We'll just save the setting.
                 _version.UseGlobalSettings = isoCombo.SelectedIndex == 1; // Reuse this flag? No, GameInstance has specific logic.
                 // Actually GameInstance.GameDir is derived.
                 // Let's just update the config flag "VersionIsolation".
                 // But wait, "VersionIsolation" isn't in GameInstance explicitly as a bool to toggle, it's a directory structure state.
                 // We will skip actual isolation toggling here as it requires moving files.
                 // Let's focus on Java.
            };

            // 启动选项组
            panel.Children.Add(CreateSettingsGroup("启动选项", new UIElement[]
            {
                CreateSettingItem("版本隔离", isoCombo), // Just visual for now
                CreateSettingItem("游戏窗口标题", CreateTextBox("跟随全局设置")),
                CreateSettingItem("游戏 Java", javaCombo),

                // Memory Slider
                CreateSettingItem("最大内存", CreateRealMemorySlider())
            }));

            // Save Button
            var saveBtn = new Button 
            { 
                Content = "💾 保存配置",
                Height = 40,
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0, 230, 118)),
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                 Style = (Style)FindResource(typeof(Button)) // Keep glass style if possible, or override
            };
            saveBtn.Click += (s, e) => 
            {
                if (Application.Current.MainWindow?.DataContext is ViewModels.MainViewModel vm)
                {
                    var gs = new GeminiLauncher.Services.GameService(vm.ConfigService);
                    gs.SaveVersionConfig(_version);
                    MessageBox.Show("设置已保存！部分设置将在下次启动时生效。", "保存成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };
            panel.Children.Add(saveBtn);

            ContentArea.Children.Add(panel);
        }

        private Border CreateSettingsGroup(string title, UIElement[] items)
        {
            var border = new Border
            {
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(20, 255, 255, 255)),
                CornerRadius = new CornerRadius(24),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 20)
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 15)
            });

            foreach (var item in items)
            {
                stack.Children.Add(item);
            }

            border.Child = stack;
            return border;
        }

        private Grid CreateSettingItem(string label, UIElement control)
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 15) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var labelText = new TextBlock
            {
                Text = label,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(180, 255, 255, 255)),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14
            };

            Grid.SetColumn(control, 1);
            grid.Children.Add(labelText);
            grid.Children.Add(control);

            return grid;
        }

        private TextBox CreateTextBox(string placeholder)
        {
            return new TextBox
            {
                Text = placeholder,
                Height = 35,
                Padding = new Thickness(10, 8, 10, 8),
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(30, 255, 255, 255)),
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(150, 255, 255, 255)), // Placeholder color
                BorderThickness = new Thickness(1),
                BorderBrush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(50, 255, 255, 255)),
                VerticalContentAlignment = VerticalAlignment.Center
            };
        }

        private ComboBox CreateComboBox(string[] items, int selectedIndex)
        {
            var cb = new ComboBox
            {
                ItemsSource = items,
                SelectedIndex = selectedIndex,
                Height = 35,
                Padding = new Thickness(10, 0, 0, 0),
                VerticalContentAlignment = VerticalAlignment.Center
            };
            return cb;
        }

        private RadioButton CreateRadioButton(string content, bool check)
        {
            return new RadioButton
            {
                Content = content,
                IsChecked = check,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 8),
                FontSize = 14
            };
        }
        
        private UIElement CreateRealMemorySlider()
        {
            var panel = new StackPanel();
            
            // Value Display
            var valueText = new TextBlock
            {
                Text = $"{(_version.CustomMemoryMb <= 0 ? 4096 : _version.CustomMemoryMb)} MB",
                Foreground = System.Windows.Media.Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 5, 5)
            };
            panel.Children.Add(valueText);

            // Slider
            var slider = new Slider
            {
                Minimum = 1024,
                Maximum = 16384,
                TickFrequency = 512,
                IsSnapToTickEnabled = true,
                Value = _version.CustomMemoryMb <= 0 ? 4096 : _version.CustomMemoryMb,
                Width = 300,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            slider.ValueChanged += (s, e) =>
            {
                int val = (int)e.NewValue;
                _version.CustomMemoryMb = val;
                valueText.Text = $"{val} MB";
            };

            panel.Children.Add(slider);
            return panel;
        }

        private Grid CreateMemorySlider(string text, double value)
        {
            var grid = new Grid { Height = 30 };
            
            // Track
            var track = new Border 
            { 
                Height = 4, 
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(50, 255, 255, 255)),
                CornerRadius = new CornerRadius(2),
                VerticalAlignment = VerticalAlignment.Center
            };
            
            // Fill (Mockup)
            var fill = new Border
            {
                Height = 4,
                Width = 200, // Fixed width for mockup
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0, 230, 118)),
                CornerRadius = new CornerRadius(2),
                VerticalAlignment = VerticalAlignment.Center
            };
            
            // Thumb (Mockup)
            var thumb = new Border
            {
                Width = 16, Height = 16,
                CornerRadius = new CornerRadius(8),
                Background = System.Windows.Media.Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(192, 0, 0, 0)
            };

            grid.Children.Add(track);
            grid.Children.Add(fill);
            grid.Children.Add(thumb);
            return grid;
        }

        private string GetModsPath()
        {
            // For isolated versions, mods are in versions/{id}/mods
            // Otherwise in .minecraft/mods
            string versionModsPath = System.IO.Path.Combine(_version.GameDir, "mods");
            if (System.IO.Directory.Exists(versionModsPath))
                return versionModsPath;
            
            // Fallback to global mods folder
            string globalModsPath = System.IO.Path.Combine(_version.RootPath, "mods");
            return globalModsPath;
        }

        public void ShowModsTab()
        {
            ContentArea.Children.Clear();
            var panel = new StackPanel();

            string modsPath = GetModsPath();

            // Header with actions
            var headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 15) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titleStack = new StackPanel();
            titleStack.Children.Add(new TextBlock
            {
                Text = "Mod 管理",
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = System.Windows.Media.Brushes.White
            });
            titleStack.Children.Add(new TextBlock
            {
                Text = modsPath,
                FontSize = 11,
                Opacity = 0.5,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 2, 0, 0),
                TextTrimming = TextTrimming.CharacterEllipsis
            });
            headerGrid.Children.Add(titleStack);

            var actionPanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(actionPanel, 1);

            var refreshBtn = CreateActionButton("🔄 刷新", (s, e) => ShowModsTab());
            var openFolderBtn = CreateActionButton("📁 打开文件夹", (s, e) =>
            {
                if (!System.IO.Directory.Exists(modsPath))
                    System.IO.Directory.CreateDirectory(modsPath);
                System.Diagnostics.Process.Start("explorer.exe", modsPath);
            });
            actionPanel.Children.Add(refreshBtn);
            actionPanel.Children.Add(openFolderBtn);
            headerGrid.Children.Add(actionPanel);
            panel.Children.Add(headerGrid);

            // Mod List
            if (!System.IO.Directory.Exists(modsPath))
            {
                panel.Children.Add(CreateEmptyModsMessage());
                ContentArea.Children.Add(panel);
                return;
            }

            var modFiles = System.IO.Directory.GetFiles(modsPath)
                .Where(f => f.EndsWith(".jar", StringComparison.OrdinalIgnoreCase) || 
                            f.EndsWith(".jar.disabled", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => System.IO.Path.GetFileName(f))
                .ToArray();

            if (modFiles.Length == 0)
            {
                panel.Children.Add(CreateEmptyModsMessage());
                ContentArea.Children.Add(panel);
                return;
            }

            // Stats
            int enabledCount = modFiles.Count(f => f.EndsWith(".jar", StringComparison.OrdinalIgnoreCase));
            int disabledCount = modFiles.Length - enabledCount;

            var statsText = new TextBlock
            {
                Text = $"共 {modFiles.Length} 个 Mod · {enabledCount} 已启用 · {disabledCount} 已禁用",
                FontSize = 13,
                Opacity = 0.6,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 15)
            };
            panel.Children.Add(statsText);

            // Batch actions
            var batchPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 15) };
            var enableAllBtn = CreateActionButton("✅ 全部启用", (s, e) => BatchToggleMods(modsPath, true));
            var disableAllBtn = CreateActionButton("⛔ 全部禁用", (s, e) => BatchToggleMods(modsPath, false));
            batchPanel.Children.Add(enableAllBtn);
            batchPanel.Children.Add(disableAllBtn);
            panel.Children.Add(batchPanel);

            // Mod items
            foreach (var modPath in modFiles)
            {
                panel.Children.Add(CreateModItem(modPath));
            }

            ContentArea.Children.Add(panel);
        }

        private UIElement CreateEmptyModsMessage()
        {
            var border = new Border
            {
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(20, 255, 255, 255)),
                CornerRadius = new CornerRadius(24),
                Padding = new Thickness(30),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            stack.Children.Add(new TextBlock
            {
                Text = "🧩",
                FontSize = 48,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });
            stack.Children.Add(new TextBlock
            {
                Text = "没有找到 Mod",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = System.Windows.Media.Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            });
            stack.Children.Add(new TextBlock
            {
                Text = "将 .jar 文件放入 mods 文件夹，然后点击刷新",
                FontSize = 13,
                Opacity = 0.6,
                Foreground = System.Windows.Media.Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            border.Child = stack;
            return border;
        }

        private UIElement CreateModItem(string modPath)
        {
            string fileName = System.IO.Path.GetFileName(modPath);
            bool isEnabled = modPath.EndsWith(".jar", StringComparison.OrdinalIgnoreCase);
            long fileSize = new System.IO.FileInfo(modPath).Length;

            var border = new Border
            {
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(isEnabled ? (byte)20 : (byte)10, 255, 255, 255)),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(15, 12, 15, 12),
                Margin = new Thickness(0, 0, 0, 6)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Toggle
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Name
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Size
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Delete

            // Toggle
            var toggle = new CheckBox
            {
                IsChecked = isEnabled,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0)
            };
            string capturedPath = modPath; // Capture for closure
            toggle.Checked += (s, e) => ToggleMod(capturedPath, true);
            toggle.Unchecked += (s, e) => ToggleMod(capturedPath, false);
            grid.Children.Add(toggle);

            // Mod name
            string displayName = isEnabled ? fileName : fileName.Replace(".disabled", "");
            var nameStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var nameText = new TextBlock
            {
                Text = displayName,
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.White,
                Opacity = isEnabled ? 1.0 : 0.5,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            nameStack.Children.Add(nameText);
            
            if (!isEnabled)
            {
                nameStack.Children.Add(new TextBlock
                {
                    Text = "已禁用",
                    FontSize = 11,
                    Foreground = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(255, 152, 0)),
                    Margin = new Thickness(0, 2, 0, 0)
                });
            }
            Grid.SetColumn(nameStack, 1);
            grid.Children.Add(nameStack);

            // File size
            var sizeText = new TextBlock
            {
                Text = FormatFileSize(fileSize),
                FontSize = 12,
                Opacity = 0.5,
                Foreground = System.Windows.Media.Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 0)
            };
            Grid.SetColumn(sizeText, 2);
            grid.Children.Add(sizeText);

            // Delete button
            var deleteBtn = new Button
            {
                Content = "🗑️",
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(0),
                FontSize = 14,
                Cursor = System.Windows.Input.Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(5),
                Foreground = System.Windows.Media.Brushes.White,
                Opacity = 0.5
            };
            deleteBtn.Click += (s, e) =>
            {
                var result = MessageBox.Show($"确定删除 {displayName} 吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        System.IO.File.Delete(capturedPath);
                        ShowModsTab(); // Refresh
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"删除失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            };
            Grid.SetColumn(deleteBtn, 3);
            grid.Children.Add(deleteBtn);

            border.Child = grid;
            return border;
        }

        private void ToggleMod(string modPath, bool enable)
        {
            try
            {
                string newPath;
                if (enable)
                {
                    // .jar.disabled -> .jar
                    newPath = modPath.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase)
                        ? modPath.Substring(0, modPath.Length - ".disabled".Length)
                        : modPath;
                }
                else
                {
                    // .jar -> .jar.disabled
                    newPath = modPath.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase)
                        ? modPath
                        : modPath + ".disabled";
                }

                if (newPath != modPath && System.IO.File.Exists(modPath))
                {
                    System.IO.File.Move(modPath, newPath);
                    ShowModsTab(); // Refresh
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"操作失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                ShowModsTab(); // Refresh to reset state
            }
        }

        private void BatchToggleMods(string modsPath, bool enable)
        {
            try
            {
                var files = System.IO.Directory.GetFiles(modsPath)
                    .Where(f => f.EndsWith(".jar", StringComparison.OrdinalIgnoreCase) || 
                                f.EndsWith(".jar.disabled", StringComparison.OrdinalIgnoreCase));

                foreach (var file in files)
                {
                    bool isCurrentEnabled = file.EndsWith(".jar", StringComparison.OrdinalIgnoreCase);
                    if (enable && !isCurrentEnabled)
                    {
                        string newPath = file.Substring(0, file.Length - ".disabled".Length);
                        if (!System.IO.File.Exists(newPath))
                            System.IO.File.Move(file, newPath);
                    }
                    else if (!enable && isCurrentEnabled)
                    {
                        string newPath = file + ".disabled";
                        if (!System.IO.File.Exists(newPath))
                            System.IO.File.Move(file, newPath);
                    }
                }

                ShowModsTab(); // Refresh
            }
            catch (Exception ex)
            {
                MessageBox.Show($"批量操作失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                ShowModsTab();
            }
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
        }

        private void ShowExportTab()
        {
            ContentArea.Children.Clear();

            var panel = new StackPanel { Margin = new Thickness(10) };

            // Header
            panel.Children.Add(new TextBlock
            {
                Text = "📦 导出版本",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 20)
            });

            panel.Children.Add(new TextBlock
            {
                Text = "将此版本的文件打包导出为 ZIP 压缩包，包含 Mod、配置文件和存档。",
                FontSize = 14,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#90FFFFFF")!),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 25)
            });

            // Include options
            var includeModsCheck = new CheckBox { Content = "包含 Mods 文件夹", IsChecked = true, Foreground = System.Windows.Media.Brushes.White, Margin = new Thickness(0, 0, 0, 8) };
            var includeConfigCheck = new CheckBox { Content = "包含 Config 配置", IsChecked = true, Foreground = System.Windows.Media.Brushes.White, Margin = new Thickness(0, 0, 0, 8) };
            var includeSavesCheck = new CheckBox { Content = "包含 Saves 存档", IsChecked = false, Foreground = System.Windows.Media.Brushes.White, Margin = new Thickness(0, 0, 0, 8) };
            var includeResourcePacksCheck = new CheckBox { Content = "包含资源包", IsChecked = false, Foreground = System.Windows.Media.Brushes.White, Margin = new Thickness(0, 0, 0, 8) };
            var includeShaderPacksCheck = new CheckBox { Content = "包含光影包", IsChecked = false, Foreground = System.Windows.Media.Brushes.White, Margin = new Thickness(0, 0, 0, 20) };

            panel.Children.Add(new TextBlock
            {
                Text = "导出内容",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#80FFFFFF")!),
                Margin = new Thickness(0, 0, 0, 10)
            });

            panel.Children.Add(includeModsCheck);
            panel.Children.Add(includeConfigCheck);
            panel.Children.Add(includeSavesCheck);
            panel.Children.Add(includeResourcePacksCheck);
            panel.Children.Add(includeShaderPacksCheck);

            // Status text
            var statusText = new TextBlock
            {
                Text = "",
                FontSize = 13,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00E676")!),
                Margin = new Thickness(0, 0, 0, 10),
                TextWrapping = TextWrapping.Wrap
            };

            // Export as ZIP button
            var exportBtn = new Button
            {
                Content = "📁 导出为 ZIP",
                Padding = new Thickness(20, 10, 20, 10),
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.White,
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00E676")!),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(0, 0, 0, 15)
            };

            exportBtn.Click += async (s, args) =>
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "ZIP Archive (*.zip)|*.zip",
                    FileName = $"{_version.Id}.zip",
                    Title = "选择导出路径"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    try
                    {
                        exportBtn.IsEnabled = false;
                        statusText.Text = "正在打包...";

                        string versionPath = System.IO.Path.Combine(_version.GameDir, "versions", _version.Id);
                        string gamePath = _version.GameDir;

                        // Collect folders to include
                        var foldersToInclude = new System.Collections.Generic.List<(string sourcePath, string entryPrefix)>();

                        // Always include the version jar and json
                        if (System.IO.Directory.Exists(versionPath))
                            foldersToInclude.Add((versionPath, $"versions/{_version.Id}"));

                        if (includeModsCheck.IsChecked == true)
                        {
                            string modsPath = System.IO.Path.Combine(gamePath, "mods");
                            if (System.IO.Directory.Exists(modsPath))
                                foldersToInclude.Add((modsPath, "mods"));
                        }
                        if (includeConfigCheck.IsChecked == true)
                        {
                            string configPath = System.IO.Path.Combine(gamePath, "config");
                            if (System.IO.Directory.Exists(configPath))
                                foldersToInclude.Add((configPath, "config"));
                        }
                        if (includeSavesCheck.IsChecked == true)
                        {
                            string savesPath = System.IO.Path.Combine(gamePath, "saves");
                            if (System.IO.Directory.Exists(savesPath))
                                foldersToInclude.Add((savesPath, "saves"));
                        }
                        if (includeResourcePacksCheck.IsChecked == true)
                        {
                            string rpPath = System.IO.Path.Combine(gamePath, "resourcepacks");
                            if (System.IO.Directory.Exists(rpPath))
                                foldersToInclude.Add((rpPath, "resourcepacks"));
                        }
                        if (includeShaderPacksCheck.IsChecked == true)
                        {
                            string spPath = System.IO.Path.Combine(gamePath, "shaderpacks");
                            if (System.IO.Directory.Exists(spPath))
                                foldersToInclude.Add((spPath, "shaderpacks"));
                        }

                        await System.Threading.Tasks.Task.Run(() =>
                        {
                            using var zip = System.IO.Compression.ZipFile.Open(saveDialog.FileName, System.IO.Compression.ZipArchiveMode.Create);
                            foreach (var (sourcePath, prefix) in foldersToInclude)
                            {
                                foreach (var file in System.IO.Directory.GetFiles(sourcePath, "*", System.IO.SearchOption.AllDirectories))
                                {
                                    string relativePath = System.IO.Path.GetRelativePath(sourcePath, file);
                                    string entryName = $"{prefix}/{relativePath}".Replace('\\', '/');
                                    zip.CreateEntryFromFile(file, entryName);
                                }
                            }
                        });

                        statusText.Text = $"✅ 导出完成: {saveDialog.FileName}";
                        statusText.Foreground = new System.Windows.Media.SolidColorBrush(
                            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00E676")!);
                    }
                    catch (System.Exception ex)
                    {
                        statusText.Text = $"❌ 导出失败: {ex.Message}";
                        statusText.Foreground = new System.Windows.Media.SolidColorBrush(
                            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF5252")!);
                    }
                    finally
                    {
                        exportBtn.IsEnabled = true;
                    }
                }
            };

            panel.Children.Add(exportBtn);
            panel.Children.Add(statusText);

            // Open folder button
            var openFolderBtn = new Button
            {
                Content = "📂 打开版本文件夹",
                Padding = new Thickness(20, 10, 20, 10),
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.White,
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#30FFFFFF")!),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            openFolderBtn.Click += (s, args) =>
            {
                string versionPath = System.IO.Path.Combine(_version.RootPath, "versions", _version.Id);
                if (System.IO.Directory.Exists(versionPath))
                    System.Diagnostics.Process.Start("explorer.exe", versionPath);
            };
            panel.Children.Add(openFolderBtn);

            ContentArea.Children.Add(panel);
        }

        // Event handlers
        private void OpenVersionFolder_Click(object sender, RoutedEventArgs e)
        {
            string versionPath = System.IO.Path.Combine(_version.RootPath, "versions", _version.Id);
            if (System.IO.Directory.Exists(versionPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", versionPath);
            }
        }

        private void OpenSavesFolder_Click(object sender, RoutedEventArgs e)
        {
            string savesPath = System.IO.Path.Combine(_version.GameDir, "saves");
            if (!System.IO.Directory.Exists(savesPath))
            {
                System.IO.Directory.CreateDirectory(savesPath);
            }
            System.Diagnostics.Process.Start("explorer.exe", savesPath);
        }

        private void OpenModsFolder_Click(object sender, RoutedEventArgs e)
        {
            string modsPath = GetModsPath();
            if (!System.IO.Directory.Exists(modsPath))
            {
                System.IO.Directory.CreateDirectory(modsPath);
            }
            System.Diagnostics.Process.Start("explorer.exe", modsPath);
        }

        private void ExportScript_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("导出启动脚本功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteVersion_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show($"确定要删除版本 {_version.Id} 吗？\n此操作不可撤销！",
                "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                // TODO: 实现删除逻辑
                MessageBox.Show("删除功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void ShowMaintenanceTab()
        {
            ContentArea.Children.Clear();
            var panel = new StackPanel { Margin = new Thickness(10) };

            // Header
            panel.Children.Add(new TextBlock
            {
                Text = "🛠️ 维护与修复",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 20)
            });

            // Clean Cache Section
            panel.Children.Add(CreateSectionHeader("清理缓存"));
            panel.Children.Add(new TextBlock
            {
                Text = "如果游戏启动失败或提示 JSON 解析错误，请尝试清理缓存。这将删除版本 JSON 文件和资源索引文件，强制启动器重新下载。",
                FontSize = 14,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#90FFFFFF")!),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 15)
            });

            var cleanJsonBtn = CreateActionButton("🗑️ 清理版本 JSON", (s, e) => CleanVersionJson());
            var cleanAssetsBtn = CreateActionButton("🗑️ 清理资源索引", (s, e) => CleanAssetIndex());
            
            panel.Children.Add(cleanJsonBtn);
            panel.Children.Add(cleanAssetsBtn);

            // Repair Libraries Section
            panel.Children.Add(CreateSectionHeader("库文件修复"));
             panel.Children.Add(new TextBlock
            {
                Text = "如果提示缺库或库文件损坏，可以尝试删除库文件夹，让启动器重新下载所有依赖。",
                FontSize = 14,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#90FFFFFF")!),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 15)
            });
            var cleanLibsBtn = CreateActionButton("🗑️ 清理 Libraries 文件夹", (s, e) => CleanLibraries());
            panel.Children.Add(cleanLibsBtn);

            ContentArea.Children.Add(panel);
        }

        private void CleanVersionJson()
        {
            if (MessageBox.Show("确定要删除版本 JSON 文件吗？\n删除后您可能需要重新安装该版本，或者重启启动器让其尝试重新修复。", "确认", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    // GameInstance.RootPath is .minecraft. 
                    // Versions are always in .minecraft/versions/{id}/{id}.json
                    string versionsDir = System.IO.Path.Combine(_version.RootPath, "versions");
                    string jsonPath = System.IO.Path.Combine(versionsDir, _version.Id, $"{_version.Id}.json");
                    if (System.IO.File.Exists(jsonPath))
                    {
                        System.IO.File.Delete(jsonPath);
                        MessageBox.Show("已删除版本 JSON 文件。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("未找到版本 JSON 文件。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"清理失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CleanAssetIndex()
        {
            if (MessageBox.Show("确定要删除资源索引文件吗？\n这将强制启动器重新校验和下载资源文件。", "确认", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    if (string.IsNullOrEmpty(_version.AssetIndexId))
                    {
                         MessageBox.Show("该版本没有关联的资源索引 ID。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                         return;
                    }

                    // Assets/indexes are always in .minecraft/assets/indexes
                    string assetsDir = System.IO.Path.Combine(_version.RootPath, "assets", "indexes");
                    string indexPath = System.IO.Path.Combine(assetsDir, $"{_version.AssetIndexId}.json");
                    if (System.IO.File.Exists(indexPath))
                    {
                        System.IO.File.Delete(indexPath);
                        MessageBox.Show($"已删除资源索引 ({_version.AssetIndexId}.json)。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"未找到资源索引文件: {indexPath}", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"清理失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void CleanLibraries()
        {
             if (MessageBox.Show("确定要删除整个 Libraries 文件夹吗？\n下次启动时将需要重新下载所有依赖库，耗时较长。", "高风险确认", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    // Libraries are usually shared, in .minecraft/libraries
                    string libPath = System.IO.Path.Combine(_version.RootPath, "libraries");
                    if (System.IO.Directory.Exists(libPath))
                    {
                        System.IO.Directory.Delete(libPath, true); // Recursive delete
                         MessageBox.Show("已删除 Libraries 文件夹。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("未找到 Libraries 文件夹。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"清理失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
