using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeminiLauncher.Models.Ecosystem;
using GeminiLauncher.Services.Ecosystem;
using GeminiLauncher.Services;
using GeminiLauncher.Controls;

namespace GeminiLauncher.ViewModels
{
    public partial class ResourcesViewModel : ObservableObject
    {
        private readonly ModrinthService _modrinthService;
        private readonly ModpackService _modpackService;
        private readonly ConfigService _configService;
        private CancellationTokenSource? _searchDebounceCts;
        private CancellationTokenSource? _featuredLoadCts;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private bool _isBusy = false;

        [ObservableProperty]
        private bool _isFeaturedLoading = true;

        [ObservableProperty]
        private bool _isImporting;

        [ObservableProperty]
        private double _importProgress;

        [ObservableProperty]
        private string _importStatus = "Preparing...";

        [ObservableProperty]
        private bool _hasSearchResults;

        [ObservableProperty]
        private string _selectedCategory = "mod";

        [ObservableProperty]
        private bool _isFullPageView;

        [ObservableProperty]
        private bool _isSidebarCollapsed = true;

        [ObservableProperty]
        private bool _isSearchEmpty;

        public ObservableCollection<ModProject> SearchResults { get; } = new ObservableCollection<ModProject>();
        public ObservableCollection<ModProject> TrendingMods { get; } = new ObservableCollection<ModProject>();
        public ObservableCollection<ModProject> NewestMods { get; } = new ObservableCollection<ModProject>();

        public ResourcesViewModel()
        {
            _modrinthService = new ModrinthService();
            _modpackService = new ModpackService();
            _configService = ConfigService.Instance;

            IsFeaturedLoading = true;
            _ = LoadInitialAsync();
        }

        private async Task LoadInitialAsync()
        {
            await Task.Delay(100);

            if (PreloadService.IsPreloaded && PreloadService.CachedTrendingMods.Count > 0)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TrendingMods.Clear();
                    foreach (var mod in PreloadService.CachedTrendingMods) TrendingMods.Add(mod);

                    NewestMods.Clear();
                    foreach (var mod in PreloadService.CachedNewestMods) NewestMods.Add(mod);

                    IsFeaturedLoading = false;
                });
                return;
            }

            _featuredLoadCts?.Cancel();
            _featuredLoadCts = new CancellationTokenSource();
            try { await LoadFeaturedContentAsync(_featuredLoadCts.Token); }
            catch (OperationCanceledException) { }
        }

        private async Task LoadFeaturedContentAsync(CancellationToken ct = default)
        {
            IsFeaturedLoading = true;
            try
            {
                ct.ThrowIfCancellationRequested();

                var trendingTask = _modrinthService.GetTrendingAsync(10, SelectedCategory);
                var newestTask = _modrinthService.GetNewestAsync(10, SelectedCategory);

                await Task.WhenAll(trendingTask, newestTask);
                ct.ThrowIfCancellationRequested();

                var trending = trendingTask.Result;
                var newest = newestTask.Result;

                var allProjects = trending.Concat(newest).ToList();

                var imageTask = PreloadImagesAsync(allProjects, ct);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    TrendingMods.Clear();
                    foreach (var mod in trending) TrendingMods.Add(mod);

                    NewestMods.Clear();
                    foreach (var mod in newest) NewestMods.Add(mod);

                    IsFeaturedLoading = false;
                });

                await imageTask;
            }
            catch (OperationCanceledException) { }
            catch
            {
                Application.Current.Dispatcher.Invoke(() => IsFeaturedLoading = false);
            }
        }

        private async Task PreloadImagesAsync(List<ModProject> projects, CancellationToken ct)
        {
            try
            {
                var tasks = projects.Select(async mod =>
                {
                    if (ct.IsCancellationRequested) return;
                    if (!string.IsNullOrEmpty(mod.IconUrl))
                    {
                        var img = await ImageCache.GetOrLoadAsync(mod.IconUrl, 180);
                        if (img != null && !ct.IsCancellationRequested)
                        {
                            Application.Current.Dispatcher.Invoke(() => mod.IconImage = img);
                        }
                    }
                });
                await Task.WhenAll(tasks);
            }
            catch { }
        }

        [RelayCommand]
        private void SwitchCategory(string category)
        {
            SelectedCategory = category;
            SearchQuery = string.Empty;
            HasSearchResults = false;
            _featuredLoadCts?.Cancel();
            _ = LoadFeaturedContentAsync();
        }

        [RelayCommand]
        private async Task Search()
        {
            _searchDebounceCts?.Cancel();
            _searchDebounceCts = new CancellationTokenSource();
            var ct = _searchDebounceCts.Token;

            try
            {
                await Task.Delay(400, ct);
            }
            catch (OperationCanceledException) { return; }

            await ExecuteSearchAsync(ct);
        }

        private async Task ExecuteSearchAsync(CancellationToken ct)
        {
            IsFullPageView = true;
            var mainVM = Application.Current.MainWindow.DataContext as MainViewModel;
            if (mainVM != null) mainVM.IsGlobalResourcesOverlayActive = true;

            IsBusy = true;
            HasSearchResults = false;
            IsSearchEmpty = false;
            SearchResults.Clear();

            try
            {
                var query = SearchQuery ?? "";
                var results = await _modrinthService.SearchProjectsAsync(query, 20, "relevance", SelectedCategory);

                ct.ThrowIfCancellationRequested();

                if (results.Count == 0)
                {
                    IsSearchEmpty = true;
                    return;
                }

                await PreloadImagesAsync(results, ct);

                foreach (var item in results)
                    SearchResults.Add(item);

                HasSearchResults = true;
            }
            catch (OperationCanceledException) { }
            catch (System.Exception ex)
            {
                IsSearchEmpty = true;
                iOS26Dialog.Show($"搜索失败: {ex.Message}", "错误", DialogIcon.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ViewMore()
        {
            _searchDebounceCts?.Cancel();
            _searchDebounceCts = new CancellationTokenSource();

            SearchQuery = "";
            _ = ExecuteSearchAsync(_searchDebounceCts.Token);
        }

        [RelayCommand]
        private void GoBack()
        {
            IsFullPageView = false;
            var mainVM = Application.Current.MainWindow.DataContext as MainViewModel;
            if (mainVM != null) mainVM.IsGlobalResourcesOverlayActive = false;

            SearchQuery = "";
            SearchResults.Clear();
            HasSearchResults = false;
        }

        [RelayCommand]
        private async Task LoadMore()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                int offset = SearchResults.Count;
                var query = SearchQuery ?? "";
                var results = await _modrinthService.SearchProjectsAsync(query, 20, "relevance", SelectedCategory, offset);

                var imageTask = PreloadImagesAsync(results, CancellationToken.None);

                foreach (var item in results)
                    SearchResults.Add(item);

                await imageTask;
            }
            catch { }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task DownloadMod(ModProject project)
        {
            if (project == null) return;

            try
            {
                IsBusy = true;
                var mainVM = ((App)Application.Current).MainWindow.DataContext as MainViewModel;
                string? targetVersion = mainVM?.SelectedVersion?.Id;

                if (!string.IsNullOrEmpty(targetVersion) && targetVersion.Contains(" "))
                    targetVersion = targetVersion.Split(' ')[0];

                if (string.IsNullOrEmpty(targetVersion))
                {
                    if (iOS26Dialog.Show("未选择游戏版本，是否下载最新版本？", "版本检查", DialogIcon.Warning, DialogButtons.YesNo) != true) return;
                    targetVersion = null;
                }

                var visitedProjects = new System.Collections.Generic.HashSet<string>();
                await DownloadProjectRecursive(project.Id, targetVersion, visitedProjects);

                iOS26Dialog.Show("下载完成！", "成功", DialogIcon.Success);
            }
            catch (System.Exception ex)
            {
                iOS26Dialog.Show($"下载失败: {ex.Message}", "错误", DialogIcon.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DownloadProjectRecursive(string projectId, string? gameVersion, System.Collections.Generic.HashSet<string> visited)
        {
            if (visited.Contains(projectId)) return;
            visited.Add(projectId);

            var versions = await _modrinthService.GetVersionsAsync(projectId, gameVersion);
            var bestMatch = versions.FirstOrDefault();

            if (bestMatch == null) return;

            var mainVM = ((App)Application.Current).MainWindow.DataContext as MainViewModel;
            string gamePath = mainVM?.ConfigService.Settings.GamePath ?? ".minecraft";
            string modsDir = System.IO.Path.Combine(gamePath, "mods");
            System.IO.Directory.CreateDirectory(modsDir);
            string dest = System.IO.Path.Combine(modsDir, bestMatch.FileName);

            if (!System.IO.File.Exists(dest))
            {
                var downloadService = new GeminiLauncher.Services.Network.DownloadService();
                await downloadService.DownloadFileAsync(bestMatch.DownloadUrl, dest, null);
            }

            if (bestMatch.Dependencies != null)
            {
                foreach (var dep in bestMatch.Dependencies)
                {
                    if (dep.DependencyType == "required" && !string.IsNullOrEmpty(dep.ProjectId))
                    {
                        await DownloadProjectRecursive(dep.ProjectId, gameVersion, visited);
                    }
                }
            }
        }

        [RelayCommand]
        private async Task ImportModpack()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Modrinth Modpack (*.mrpack)|*.mrpack",
                Title = "Import Modpack"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    IsImporting = true;
                    ImportProgress = 0;
                    ImportStatus = "Initializing...";

                    var progress = new System.Progress<double>(p => ImportProgress = p);
                    var status = new System.Progress<string>(s => ImportStatus = s);

                    await _modpackService.ImportMrPackAsync(dialog.FileName,
                        string.IsNullOrWhiteSpace(_configService.Settings.GamePath) ? ".minecraft" : _configService.Settings.GamePath,
                        progress, status);

                    iOS26Dialog.Show("整合包导入成功！", "成功", DialogIcon.Success);
                }
                catch (System.Exception ex)
                {
                    iOS26Dialog.Show($"导入失败: {ex.Message}", "错误", DialogIcon.Error);
                }
                finally
                {
                    IsImporting = false;
                }
            }
        }
    }
}
