using System.Windows;
using GeminiLauncher.Controls;

namespace GeminiLauncher
{
    public partial class App : Application
    {
        public App()
        {
            // Global Exception Handling
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            System.AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            GeminiLauncher.Utilities.Logger.Initialize();
            GeminiLauncher.Utilities.Logger.Log("Application Started");
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            GeminiLauncher.Utilities.Logger.LogError(e.Exception, "DispatcherUnhandledException");
            iOS26Dialog.Show($"发生未处理的异常 (UI):\n{e.Exception.Message}\n\n详情请查看 GeminiLauncher.log", "程序崩溃", DialogIcon.Error);
            e.Handled = true; // Prevent crash
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
        {
             GeminiLauncher.Utilities.Logger.LogError(e.Exception, "TaskScheduler.UnobservedTaskException");
             e.SetObserved(); // Prevent crash
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
             if (e.ExceptionObject is System.Exception ex)
             {
                 GeminiLauncher.Utilities.Logger.LogError(ex, "CurrentDomain.UnhandledException");
                 iOS26Dialog.Show($"发生严重错误:\n{ex.Message}\n\n程序即将退出。", "致命错误", DialogIcon.Error);
             }
        }
        public static void SwitchLanguage(string languageCode)
        {
            var dict = new ResourceDictionary();
            
            switch (languageCode)
            {
                case "zh-CN":
                    dict.Source = new System.Uri("Resources/Languages/zh-CN.xaml", System.UriKind.Relative);
                    break;
                case "en-US":
                default:
                    dict.Source = new System.Uri("Resources/Languages/en-US.xaml", System.UriKind.Relative);
                    break;
            }
            
            // Remove old language dictionary
            var oldDict = Current.Resources.MergedDictionaries.FirstOrDefault(d => 
                d.Source != null && d.Source.OriginalString.Contains("Languages/"));
            
            if (oldDict != null)
            {
                Current.Resources.MergedDictionaries.Remove(oldDict);
            }
            
            // Add new language dictionary
            Current.Resources.MergedDictionaries.Add(dict);
        }
    }
}
