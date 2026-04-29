Ïusing System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace GeminiLauncher.Services
{
    public class UpdateService
    {
        // TODO: Replace with real repo
        private const string RepoOwner = "LinYiZhi"; 
        private const string RepoName = "LinLaunch";
        public const string CurrentVersion = "1.0.0";

        public async Task CheckForUpdatesAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("LinLaunch");
                
                // Simulated check for prototype
                // var response = await client.GetStringAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
                // var json = JObject.Parse(response);
                // string latestTag = json["tag_name"]?.ToString();
                 
                await Task.Delay(1500); // Simulate network
                string latestTag = "1.0.0"; // Simulate same version
                // To test update, change this to "1.0.1" manually
                
                if (latestTag != CurrentVersion)
                {
                     if (MessageBox.Show($"New version {latestTag} is available! Update now?", "Update Available", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                     {
                         // Mock URL
                         await PerformUpdateAsync("https://github.com/LinYiZhi/LinLaunch/releases/download/v1.0.1/LinLaunch.exe"); 
                     }
                }
                else
                {
                    MessageBox.Show($"You are using the latest version ({CurrentVersion}).", "Up to date", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update check failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task PerformUpdateAsync(string downloadUrl)
        {
            string currentExe = Process.GetCurrentProcess().MainModule?.FileName ?? throw new InvalidOperationException("Cannot determine current process path.");
            string backupExe = currentExe + ".bak";

            try
            {
                // 1. Rename current executable to .bak
                if (File.Exists(backupExe)) File.Delete(backupExe);
                File.Move(currentExe, backupExe);

                // 2. Download new executable
                // In real scenario, use DownloadService or HttpClient
                // For simulation, we'll just copy the backup back to pretend we downloaded it
                // await new HttpClient().DownloadFileAsync(downloadUrl, currentExe);
                
                await Task.Delay(2000); // Simulate download
                File.Copy(backupExe, currentExe); // Restore for now so we don't break the app in dev

                MessageBox.Show("Update downloaded successfully! The application will now restart.", "Update Complete");

                // 3. Restart
                Process.Start(currentExe);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                // Restore if failed
                if (File.Exists(backupExe) && !File.Exists(currentExe))
                {
                    File.Move(backupExe, currentExe);
                }
                MessageBox.Show($"Update failed: {ex.Message}");
            }
        }
    }
}
Ï*cascade082Jfile:///c:/Users/Linyizhi/.gemini/GeminiLauncher/Services/UpdateService.cs