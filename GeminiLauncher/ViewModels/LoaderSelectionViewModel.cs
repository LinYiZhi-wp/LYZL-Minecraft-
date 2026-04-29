using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeminiLauncher.Models;
using GeminiLauncher.Services;
using GeminiLauncher.Services.Ecosystem;
using GeminiLauncher.Services.Network;

namespace GeminiLauncher.ViewModels
{
    public partial class LoaderSelectionViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _mcVersion = string.Empty;

        [ObservableProperty]
        private DownloadableVersion? _selectedVersion;

        [ObservableProperty]
        private bool _isForgeLoading;

        [ObservableProperty]
        private bool _isFabricLoading;

        [ObservableProperty]
        private bool _isOptifineLoading;

        [ObservableProperty]
        private bool _forgeExpanded;

        [ObservableProperty]
        private bool _fabricExpanded;

        [ObservableProperty]
        private bool _optifineExpanded;

        [ObservableProperty]
        private string _forgeSelectedVersion = string.Empty;

        [ObservableProperty]
        private string _fabricSelectedVersion = string.Empty;

        [ObservableProperty]
        private string _optifineSelectedVersion = string.Empty;

        [ObservableProperty]
        private LoaderVersionItem? _selectedForgeItem;

        [ObservableProperty]
        private LoaderVersionItem? _selectedFabricItem;

        [ObservableProperty]
        private LoaderVersionItem? _selectedOptifineItem;

        [ObservableProperty]
        private bool _isDownloading;

        [ObservableProperty]
        private string _downloadStatus = string.Empty;

        [ObservableProperty]
        private double _downloadProgress;

        public ObservableCollection<LoaderVersionItem> ForgeVersions { get; } = new();
        public ObservableCollection<LoaderVersionItem> FabricVersions { get; } = new();
        public ObservableCollection<LoaderVersionItem> OptiFineVersions { get; } = new();

        private readonly LoaderApiService _loaderApiService = new();
        private bool _forgeLoaded, _fabricLoaded, _optifineLoaded;

        public LoaderSelectionViewModel()
        {
        }

        public void Initialize(DownloadableVersion version)
        {
            SelectedVersion = version;
            McVersion = version.Id;
        }

        partial void OnSelectedForgeItemChanged(LoaderVersionItem? value)
        {
            ForgeSelectedVersion = value?.Version ?? "";
        }

        partial void OnSelectedFabricItemChanged(LoaderVersionItem? value)
        {
            FabricSelectedVersion = value?.Version ?? "";
        }

        partial void OnSelectedOptifineItemChanged(LoaderVersionItem? value)
        {
            OptifineSelectedVersion = value?.Version ?? "";
        }

        [RelayCommand]
        private void ToggleForge()
        {
            ForgeExpanded = !ForgeExpanded;
            if (ForgeExpanded && !_forgeLoaded)
            {
                _forgeLoaded = true;
                IsForgeLoading = true;
                Task.Run(() =>
                {
                    try { return _loaderApiService.GetForgeVersions(McVersion); }
                    catch { return new System.Collections.Generic.List<LoaderVersionItem>(); }
                }).ContinueWith(t =>
                {
                    if (!Application.Current.Dispatcher.HasShutdownStarted)
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ForgeVersions.Clear();
                            foreach (var v in t.Result) ForgeVersions.Add(v);
                            if (ForgeVersions.Count > 0) SelectedForgeItem = ForgeVersions[0];
                            IsForgeLoading = false;
                        });
                });
            }
        }

        [RelayCommand]
        private void ToggleFabric()
        {
            FabricExpanded = !FabricExpanded;
            if (FabricExpanded && !_fabricLoaded)
            {
                _fabricLoaded = true;
                IsFabricLoading = true;
                Task.Run(() =>
                {
                    try { return _loaderApiService.GetFabricVersions(McVersion); }
                    catch { return new System.Collections.Generic.List<LoaderVersionItem>(); }
                }).ContinueWith(t =>
                {
                    if (!Application.Current.Dispatcher.HasShutdownStarted)
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            FabricVersions.Clear();
                            foreach (var v in t.Result) FabricVersions.Add(v);
                            if (FabricVersions.Count > 0) SelectedFabricItem = FabricVersions[0];
                            IsFabricLoading = false;
                        });
                });
            }
        }

        [RelayCommand]
        private void ToggleOptifine()
        {
            OptifineExpanded = !OptifineExpanded;
            if (OptifineExpanded && !_optifineLoaded)
            {
                _optifineLoaded = true;
                IsOptifineLoading = true;
                Task.Run(() =>
                {
                    try { return _loaderApiService.GetOptiFineVersions(McVersion); }
                    catch { return new System.Collections.Generic.List<LoaderVersionItem>(); }
                }).ContinueWith(t =>
                {
                    if (!Application.Current.Dispatcher.HasShutdownStarted)
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            OptiFineVersions.Clear();
                            foreach (var v in t.Result) OptiFineVersions.Add(v);
                            if (OptiFineVersions.Count > 0) SelectedOptifineItem = OptiFineVersions[0];
                            IsOptifineLoading = false;
                        });
                });
            }
        }

        [RelayCommand]
        private async Task StartDownload()
        {
            if (IsDownloading || SelectedVersion == null) return;
            IsDownloading = true;
            DownloadStatus = "正在准备下载...";

            try
            {
                string loaderChoice = "Vanilla";
                string loaderVersion = "";

                if (!string.IsNullOrEmpty(ForgeSelectedVersion))
                {
                    loaderChoice = "Forge";
                    loaderVersion = ForgeSelectedVersion;
                }
                else if (!string.IsNullOrEmpty(FabricSelectedVersion))
                {
                    loaderChoice = "Fabric";
                    loaderVersion = FabricSelectedVersion;
                }
                else if (!string.IsNullOrEmpty(OptifineSelectedVersion))
                {
                    loaderChoice = "OptiFine";
                    loaderVersion = OptifineSelectedVersion;
                }

                await DownloadManagerService.Instance.EnqueueGameDownloadWithLoader(
                    SelectedVersion, loaderChoice, loaderVersion,
                    ConfigService.Instance.Settings.DownloadSource);

                DownloadStatus = "下载已加入队列";
                DownloadProgress = 1.0;
            }
            catch (Exception ex)
            {
                DownloadStatus = $"下载失败: {ex.Message}";
            }
            finally { IsDownloading = false; }
        }

        [RelayCommand]
        private void GoBack()
        {
            if (Application.Current.MainWindow is MainWindow mw)
                mw.RootFrame.GoBack();
        }
    }

    public class LoaderVersionItem
    {
        public string Version { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public bool IsRecommended { get; set; }
        public bool IsLatest { get; set; }
    }
}
