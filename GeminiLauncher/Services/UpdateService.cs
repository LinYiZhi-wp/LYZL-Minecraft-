using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using GeminiLauncher.Controls;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GeminiLauncher.Services
{
    public class UpdateService
    {
        // TODO: Replace with real repo
        private const string RepoOwner = "LinYiZhi"; 
        private const string RepoName = "LYZL";
        public const string CurrentVersion = "1.0.0";

        public async Task CheckForUpdatesAsync()
        {
            try
            {
                string msg = (string)System.Windows.Application.Current.FindResource("Msg_UpdateNotImplemented");
                iOS26Dialog.Show(string.Format(msg, CurrentVersion), (string)System.Windows.Application.Current.FindResource("Title_Warning"), DialogIcon.Info);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                iOS26Dialog.Show($"更新检查失败: {ex.Message}", "错误", DialogIcon.Error);
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

                iOS26Dialog.Show("更新下载完成！应用即将重启。", "更新完成", DialogIcon.Success);

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
                iOS26Dialog.Show($"更新失败: {ex.Message}", "错误", DialogIcon.Error);
            }
        }
    }
}
