using System.Windows;
using System.Windows.Controls;

namespace GeminiLauncher.Views
{
    public partial class ResourcesPage : Page
    {
        public ResourcesPage()
        {
            InitializeComponent();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // If Shift is pressed, we assume horizontal scrolling is intended, so we let the ScrollViewer handle it.
            if (System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift)) return;

            // Otherwise, we bubble the event to the parent (the main page scrollviewer)
            // This fixes the issue where hovering over a horizontal list prevents vertical page scrolling.
            e.Handled = true;
            var eventArg = new System.Windows.Input.MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = sender;
            var parent = ((Control)sender).Parent as UIElement;
            parent?.RaiseEvent(eventArg);
        }
    }
}
