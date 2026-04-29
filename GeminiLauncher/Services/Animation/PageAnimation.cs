using System;
using System.Windows;
using System.Windows.Controls;
using GeminiLauncher.Services.Animation;

namespace GeminiLauncher.Services.Animation
{
    public static class PageAnimation
    {
        public static readonly DependencyProperty EnableEnterAnimationProperty =
            DependencyProperty.RegisterAttached(
                "EnableEnterAnimation",
                typeof(bool),
                typeof(PageAnimation),
                new PropertyMetadata(false, OnEnableEnterAnimationChanged));

        public static bool GetEnableEnterAnimation(DependencyObject obj) =>
            (bool)obj.GetValue(EnableEnterAnimationProperty);

        public static void SetEnableEnterAnimation(DependencyObject obj, bool value) =>
            obj.SetValue(EnableEnterAnimationProperty, value);

        private static void OnEnableEnterAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element && (bool)e.NewValue)
            {
                element.Loaded -= OnElementLoaded;
                element.Loaded += OnElementLoaded;
            }
        }

        private static void OnElementLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                PageTransition.Play(element, TransitionType.FadeSlideIn);
            }
        }

        public static readonly DependencyProperty StaggerChildrenProperty =
            DependencyProperty.RegisterAttached(
                "StaggerChildren",
                typeof(bool),
                typeof(PageAnimation),
                new PropertyMetadata(false, OnStaggerChildrenChanged));

        public static bool GetStaggerChildren(DependencyObject obj) =>
            (bool)obj.GetValue(StaggerChildrenProperty);

        public static void SetStaggerChildren(DependencyObject obj, bool value) =>
            obj.SetValue(StaggerChildrenProperty, value);

        private static void OnStaggerChildrenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Panel panel && (bool)e.NewValue)
            {
                panel.Loaded -= OnPanelLoaded;
                panel.Loaded += OnPanelLoaded;
            }
        }

        private static void OnPanelLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Panel panel)
            {
                PageTransition.PlayStaggeredIn(panel, staggerMs: 50);
            }
        }
    }
}
