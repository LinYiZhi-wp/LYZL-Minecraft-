using System.Windows;
using System.Windows.Controls;
using GeminiLauncher.Services.Animation;

namespace GeminiLauncher.Views
{
    public partial class DownloadPage : Page
    {
        public DownloadPage()
        {
            InitializeComponent();
        }

        private void OpenDownloadManager_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null)
            {
                NavigationService.Navigate(new DownloadManagerPage());
            }
        }
    }
}
