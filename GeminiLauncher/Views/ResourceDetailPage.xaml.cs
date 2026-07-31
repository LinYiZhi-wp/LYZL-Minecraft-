using System;
using System.Windows;
using System.Windows.Controls;
using GeminiLauncher.Models.Ecosystem;
using GeminiLauncher.ViewModels;

namespace GeminiLauncher.Views
{
    public partial class ResourceDetailPage : Page
    {
        private readonly ResourceDetailViewModel _viewModel;

        public ResourceDetailPage(ModProject project, string? gameVersion = null)
        {
            InitializeComponent();
            _viewModel = DataContext as ResourceDetailViewModel;
            if (_viewModel == null) return;

            Loaded += async (s, e) =>
            {
                try
                {
                    await _viewModel.InitializeAsync(project, gameVersion);
                }
                catch { }
            };
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void OpenBrowser_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OpenInBrowserCommand.Execute(null);
        }
    }
}
