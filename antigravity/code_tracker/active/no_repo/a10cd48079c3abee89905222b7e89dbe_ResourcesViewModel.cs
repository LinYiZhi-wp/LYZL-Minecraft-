ÊDusing System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeminiLauncher.Models.Ecosystem;
using GeminiLauncher.Services.Ecosystem;
using GeminiLauncher.Services;

namespace GeminiLauncher.ViewModels
{
    public partial class ResourcesViewModel : ObservableObject
    {
        private readonly ModrinthService _modrinthService;
        private readonly ModpackService _modpackService;
        private readonly ConfigService _configService;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private bool _isBusy = false;

        [ObservableProperty]
        private bool _isImporting;

        [ObservableProperty]
        private double _importProgress;

        [ObservableProperty]
        private string _importStatus = "Preparing...";

        [ObservableProperty]
        private bool _hasSearchResults;

        [ObservableProperty]
        private string _selectedCategory = "mod"; // mod, modpack, resourcepack, shader, datapack

        public ObservableCollection<ModProject> SearchResults { get; } = new ObservableCollection<ModProject>();
        public ObservableCollection<ModProject> TrendingMods { get; } = new ObservableCollection<ModProject>();
        public ObservableCollection<ModProject> NewestMods { get; } = new ObservableCollection<ModProject>();

        public ResourcesViewModel()
        {
            _modrinthService = new ModrinthService();
            _modpackService = new ModpackService();
            _configService = new ConfigService();

            // Auto-load featured content
            _ = LoadFeaturedContentAsync();
        }

        private async Task LoadFeaturedContentAsync()
        {
            try
            {
                TrendingMods.Clear();
                NewestMods.Clear();

                var trending = await _modrinthService.GetTrendingAsync(10, SelectedCategory);
                foreach (var mod in trending) TrendingMods.Add(mod);

                var newest = await _modrinthService.GetNewestAsync(10, SelectedCategory);
                foreach (var mod in newest) NewestMods.Add(mod);
            }
            catch { /* Silently fail â€” featured content is optional */ }
        }

        [RelayCommand]
        private void SwitchCategory(string category)
        {
            SelectedCategory = category;
            SearchQuery = string.Empty;
            HasSearchResults = false;
            _ = LoadFeaturedContentAsync();
        }

        [RelayCommand]
        private async Task Search()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                // If query is empty, we act as "View More" -> Show all results
                // Default behavior: search with empty query returns everything (usually)
            }

            IsBusy = true;
            SearchResults.Clear();

            try
            {
                // If query is empty/null, pass empty string to get "all" (relevance usually defaults to popular)
                var query = SearchQuery ?? "";
                var results = await _modrinthService.SearchProjectsAsync(query, 20, "relevance", SelectedCategory);
                foreach (var item in results)
                {
                    SearchResults.Add(item);
                }
                HasSearchResults = true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Search failed: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ViewMore()
        {
            SearchQuery = ""; // Clear query
            _ = Search(); // Trigger search to show "all"
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
                
                // Heuristic cleanup of version ID if needed
                if (!string.IsNullOrEmpty(targetVersion) && targetVersion.Contains(" "))
                    targetVersion = targetVersion.Split(' ')[0];

                if (string.IsNullOrEmpty(targetVersion))
                {
                     if (MessageBox.Show("No game version selected. Download latest?", "Version Check", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
                     targetVersion = null;
                }

                // Track visited projects to avoid cycles
                var visitedProjects = new System.Collections.Generic.HashSet<string>();
                await DownloadProjectRecursive(project.Id, targetVersion, visitedProjects);
                
                MessageBox.Show("Download complete!");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Download failed: {ex.Message}");
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

            // 1. Get versions
            var versions = await _modrinthService.GetVersionsAsync(projectId, gameVersion);
            var bestMatch = versions.FirstOrDefault();

            if (bestMatch == null)
            {
                // Optional: Log warning that dependency couldn't be found
                return;
            }

            // 2. Download file
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

            // 3. Process Dependencies
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
                    
                    // Create progress handlers
                    var progress = new System.Progress<double>(p => ImportProgress = p);
                    var status = new System.Progress<string>(s => ImportStatus = s);

                    await _modpackService.ImportMrPackAsync(dialog.FileName, 
                        string.IsNullOrWhiteSpace(_configService.Settings.GamePath) ? ".minecraft" : _configService.Settings.GamePath, 
                        progress, status);

                    MessageBox.Show("Modpack imported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Import failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsImporting = false;
                }
            }
        }
    }
}
ÊD2Qfile:///c:/Users/Linyizhi/.gemini/GeminiLauncher/ViewModels/ResourcesViewModel.cs