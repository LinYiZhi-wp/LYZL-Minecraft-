using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace GeminiLauncher.Services.Animation
{
    public enum TransitionType
    {
        SlideIn,
        SlideOut,
        FadeSlideIn,
        FadeSlideOut,
        ScaleIn,
        ScaleOut,
        FadeIn,
        FadeOut,
        SlideUp,
        SlideDown,
        None
    }

    public static class TransitionConfig
    {
        public static Duration DefaultDuration => TimeSpan.FromSeconds(0.35);
        public static Duration FastDuration => TimeSpan.FromSeconds(0.2);
        public static Duration SlowDuration => TimeSpan.FromSeconds(0.5);

        public static double SlideDistance => 60;
        public static double SlideDistanceSubtle => 30;
        public static double ScaleFrom => 0.92;

        public static double PageEnterSlideX => 24;
        public static double PageExitSlideX => 16;
        public static Duration PageEnterDuration => TimeSpan.FromSeconds(0.3);
        public static Duration PageExitDuration => TimeSpan.FromSeconds(0.15);

        public static IEasingFunction SmoothEase => new CubicEase { EasingMode = EasingMode.EaseOut };
        public static IEasingFunction DecelerateEase => new CubicEase { EasingMode = EasingMode.EaseOut };
        public static IEasingFunction AccelerateEase => new CubicEase { EasingMode = EasingMode.EaseIn };
        public static IEasingFunction SpringEase => new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.4 };
        public static IEasingFunction JellyEase => new ElasticEase { EasingMode = EasingMode.EaseOut, Springiness = 5, Oscillations = 1 };
        public static IEasingFunction PageEnterEase => new CubicEase { EasingMode = EasingMode.EaseOut };
        public static IEasingFunction PageExitEase => new CubicEase { EasingMode = EasingMode.EaseIn };
    }

    public static class PageTransition
    {
        private const string ScaleName = "_ptScale";
        private const string TranslateName = "_ptTranslate";

        private static Storyboard? _currentStoryboard;

        public static void Play(FrameworkElement target, TransitionType type, Action? onCompleted = null)
        {
            _currentStoryboard?.Stop();

            var (scale, translate) = EnsureTransforms(target);

            var sb = new Storyboard();
            var duration = TransitionConfig.DefaultDuration;

            switch (type)
            {
                case TransitionType.FadeSlideIn:
                    AddFadeSlideIn(sb, target, scale, translate, duration);
                    break;
                case TransitionType.FadeSlideOut:
                    AddFadeSlideOut(sb, target, scale, translate, duration);
                    break;
                case TransitionType.SlideIn:
                    AddSlideIn(sb, target, translate, duration);
                    break;
                case TransitionType.SlideOut:
                    AddSlideOut(sb, target, translate, duration);
                    break;
                case TransitionType.ScaleIn:
                    AddScaleIn(sb, target, scale, duration);
                    break;
                case TransitionType.ScaleOut:
                    AddScaleOut(sb, target, scale, duration);
                    break;
                case TransitionType.FadeIn:
                    AddFadeIn(sb, target, duration);
                    break;
                case TransitionType.FadeOut:
                    AddFadeOut(sb, target, duration);
                    break;
                case TransitionType.SlideUp:
                    AddSlideUp(sb, target, translate, duration);
                    break;
                case TransitionType.SlideDown:
                    AddSlideDown(sb, target, translate, duration);
                    break;
            }

            if (onCompleted != null)
                sb.Completed += (s, e) => onCompleted();

            _currentStoryboard = sb;
            sb.Begin(target);
        }

        public static void PlayPageEnter(Page page, bool isForward = true)
        {
            page.Opacity = 0;

            var sb = new Storyboard();

            var fade = new DoubleAnimation(0, 1, TransitionConfig.PageEnterDuration)
            {
                EasingFunction = TransitionConfig.PageEnterEase
            };
            Storyboard.SetTarget(fade, page);
            Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
            sb.Children.Add(fade);

            sb.Begin(page);
        }

        public static void PlayPageExit(Page page, bool isForward = true, Action? onCompleted = null)
        {
            var sb = new Storyboard();

            var fade = new DoubleAnimation(1, 0, TransitionConfig.PageExitDuration)
            {
                EasingFunction = TransitionConfig.PageExitEase
            };
            Storyboard.SetTarget(fade, page);
            Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
            sb.Children.Add(fade);

            if (onCompleted != null)
                sb.Completed += (s, e) => onCompleted();

            sb.Begin(page);
        }

        public static void PlayContainerEnter(FrameworkElement container, bool isForward = true)
        {
            var scale = new ScaleTransform(0.97, 0.97);
            var translate = new TranslateTransform(isForward ? 16 : -16, 0);
            var group = new TransformGroup();
            group.Children.Add(scale);
            group.Children.Add(translate);
            container.RenderTransform = group;
            container.RenderTransformOrigin = new Point(0.5, 0.5);

            var ease = TransitionConfig.PageEnterEase;
            var duration = TransitionConfig.PageEnterDuration;

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.97, 1.0, duration) { EasingFunction = ease });
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.97, 1.0, duration) { EasingFunction = ease });
            translate.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(translate.X, 0, duration) { EasingFunction = ease });

            var timer = new DispatcherTimer { Interval = duration.TimeSpan };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                scale.ScaleX = scale.ScaleY = 1.0;
                translate.X = 0;
            };
            timer.Start();
        }

        public static void PlayContainerExit(FrameworkElement container, bool isForward = true, Action? onCompleted = null)
        {
            var scale = new ScaleTransform(1.0, 1.0);
            var translate = new TranslateTransform(0, 0);
            var group = new TransformGroup();
            group.Children.Add(scale);
            group.Children.Add(translate);
            container.RenderTransform = group;
            container.RenderTransformOrigin = new Point(0.5, 0.5);

            var ease = TransitionConfig.PageExitEase;
            var duration = TransitionConfig.PageExitDuration;

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.0, 0.98, duration) { EasingFunction = ease });
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.0, 0.98, duration) { EasingFunction = ease });
            translate.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(0, isForward ? -12 : 12, duration) { EasingFunction = ease });

            var timer = new DispatcherTimer { Interval = duration.TimeSpan };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                scale.ScaleX = scale.ScaleY = 1.0;
                translate.X = 0;
                onCompleted?.Invoke();
            };
            timer.Start();
        }

        public static void PlayStaggeredIn(Panel container, double staggerMs = 40)
        {
            for (int i = 0; i < container.Children.Count; i++)
            {
                if (container.Children[i] is FrameworkElement child)
                {
                    var (_, translate) = EnsureTransforms(child);

                    child.Opacity = 0;
                    translate.Y = 20;

                    var delay = TimeSpan.FromMilliseconds(i * staggerMs);

                    var sb = new Storyboard();

                    var fade = new DoubleAnimation(0, 1, TransitionConfig.DefaultDuration)
                    {
                        BeginTime = delay,
                        EasingFunction = TransitionConfig.DecelerateEase
                    };
                    Storyboard.SetTarget(fade, child);
                    Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
                    sb.Children.Add(fade);

                    var slide = new DoubleAnimation(20, 0, TransitionConfig.DefaultDuration)
                    {
                        BeginTime = delay,
                        EasingFunction = TransitionConfig.SmoothEase
                    };
                    Storyboard.SetTarget(slide, translate);
                    Storyboard.SetTargetProperty(slide, new PropertyPath("Y"));
                    sb.Children.Add(slide);

                    sb.Begin(child);
                }
            }
        }

        public static void PlayExpandCollapse(FrameworkElement target, bool expand, Action? onCompleted = null)
        {
            var (_, translate) = EnsureTransforms(target);

            if (expand)
            {
                target.Opacity = 0;
                translate.Y = -8;

                var sb = new Storyboard();

                var fade = new DoubleAnimation(0, 1, TransitionConfig.FastDuration)
                {
                    EasingFunction = TransitionConfig.DecelerateEase
                };
                Storyboard.SetTarget(fade, target);
                Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
                sb.Children.Add(fade);

                var slide = new DoubleAnimation(-8, 0, TransitionConfig.DefaultDuration)
                {
                    EasingFunction = TransitionConfig.SpringEase
                };
                Storyboard.SetTarget(slide, translate);
                Storyboard.SetTargetProperty(slide, new PropertyPath("Y"));
                sb.Children.Add(slide);

                if (onCompleted != null)
                    sb.Completed += (s, e) => onCompleted();
                sb.Begin(target);
            }
            else
            {
                var sb = new Storyboard();

                var fade = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.15))
                {
                    EasingFunction = TransitionConfig.AccelerateEase
                };
                Storyboard.SetTarget(fade, target);
                Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
                sb.Children.Add(fade);

                var slide = new DoubleAnimation(0, -8, TimeSpan.FromSeconds(0.15))
                {
                    EasingFunction = TransitionConfig.AccelerateEase
                };
                Storyboard.SetTarget(slide, translate);
                Storyboard.SetTargetProperty(slide, new PropertyPath("Y"));
                sb.Children.Add(slide);

                if (onCompleted != null)
                    sb.Completed += (s, e) => onCompleted();
                sb.Begin(target);
            }
        }

        public static void PlayScaleBounce(FrameworkElement target, double from = 0.9, double to = 1.0)
        {
            var (scale, _) = EnsureTransforms(target);

            var sb = new Storyboard();

            var scaleX = new DoubleAnimation(from, to, TransitionConfig.DefaultDuration)
            {
                EasingFunction = TransitionConfig.SpringEase
            };
            Storyboard.SetTarget(scaleX, scale);
            Storyboard.SetTargetProperty(scaleX, new PropertyPath("ScaleX"));
            sb.Children.Add(scaleX);

            var scaleY = new DoubleAnimation(from, to, TransitionConfig.DefaultDuration)
            {
                EasingFunction = TransitionConfig.SpringEase
            };
            Storyboard.SetTarget(scaleY, scale);
            Storyboard.SetTargetProperty(scaleY, new PropertyPath("ScaleY"));
            sb.Children.Add(scaleY);

            sb.Begin(target);
        }

        #region Private Helpers

        private static (ScaleTransform scale, TranslateTransform translate) EnsureTransforms(FrameworkElement element)
        {
            ScaleTransform scale;
            TranslateTransform translate;

            if (element.RenderTransform is TransformGroup group && group.Children.Count >= 2 &&
                group.Children[0] is ScaleTransform st && group.Children[1] is TranslateTransform tt)
            {
                scale = st;
                translate = tt;
            }
            else
            {
                scale = new ScaleTransform(1, 1);
                translate = new TranslateTransform(0, 0);
                var newGroup = new TransformGroup();
                newGroup.Children.Add(scale);
                newGroup.Children.Add(translate);
                element.RenderTransform = newGroup;
                element.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            return (scale, translate);
        }

        private static void AddFadeSlideIn(Storyboard sb, FrameworkElement target, ScaleTransform scale, TranslateTransform translate, Duration duration)
        {
            target.Opacity = 0;
            translate.X = TransitionConfig.SlideDistance;

            var fade = new DoubleAnimation(0, 1, duration) { EasingFunction = TransitionConfig.DecelerateEase };
            Storyboard.SetTarget(fade, target);
            Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
            sb.Children.Add(fade);

            var slide = new DoubleAnimation(TransitionConfig.SlideDistance, 0, duration) { EasingFunction = TransitionConfig.SmoothEase };
            Storyboard.SetTarget(slide, translate);
            Storyboard.SetTargetProperty(slide, new PropertyPath("X"));
            sb.Children.Add(slide);
        }

        private static void AddFadeSlideOut(Storyboard sb, FrameworkElement target, ScaleTransform scale, TranslateTransform translate, Duration duration)
        {
            var fade = new DoubleAnimation(1, 0, duration) { EasingFunction = TransitionConfig.AccelerateEase };
            Storyboard.SetTarget(fade, target);
            Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
            sb.Children.Add(fade);

            var slide = new DoubleAnimation(0, -TransitionConfig.SlideDistanceSubtle, duration) { EasingFunction = TransitionConfig.AccelerateEase };
            Storyboard.SetTarget(slide, translate);
            Storyboard.SetTargetProperty(slide, new PropertyPath("X"));
            sb.Children.Add(slide);
        }

        private static void AddSlideIn(Storyboard sb, FrameworkElement target, TranslateTransform translate, Duration duration)
        {
            translate.X = TransitionConfig.SlideDistance;

            var slide = new DoubleAnimation(TransitionConfig.SlideDistance, 0, duration) { EasingFunction = TransitionConfig.SmoothEase };
            Storyboard.SetTarget(slide, translate);
            Storyboard.SetTargetProperty(slide, new PropertyPath("X"));
            sb.Children.Add(slide);
        }

        private static void AddSlideOut(Storyboard sb, FrameworkElement target, TranslateTransform translate, Duration duration)
        {
            var slide = new DoubleAnimation(0, -TransitionConfig.SlideDistance, duration) { EasingFunction = TransitionConfig.AccelerateEase };
            Storyboard.SetTarget(slide, translate);
            Storyboard.SetTargetProperty(slide, new PropertyPath("X"));
            sb.Children.Add(slide);
        }

        private static void AddScaleIn(Storyboard sb, FrameworkElement target, ScaleTransform scale, Duration duration)
        {
            target.Opacity = 0;

            var fade = new DoubleAnimation(0, 1, duration) { EasingFunction = TransitionConfig.DecelerateEase };
            Storyboard.SetTarget(fade, target);
            Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
            sb.Children.Add(fade);

            var scaleX = new DoubleAnimation(TransitionConfig.ScaleFrom, 1.0, duration) { EasingFunction = TransitionConfig.SpringEase };
            Storyboard.SetTarget(scaleX, scale);
            Storyboard.SetTargetProperty(scaleX, new PropertyPath("ScaleX"));
            sb.Children.Add(scaleX);

            var scaleY = new DoubleAnimation(TransitionConfig.ScaleFrom, 1.0, duration) { EasingFunction = TransitionConfig.SpringEase };
            Storyboard.SetTarget(scaleY, scale);
            Storyboard.SetTargetProperty(scaleY, new PropertyPath("ScaleY"));
            sb.Children.Add(scaleY);
        }

        private static void AddScaleOut(Storyboard sb, FrameworkElement target, ScaleTransform scale, Duration duration)
        {
            var fade = new DoubleAnimation(1, 0, duration) { EasingFunction = TransitionConfig.AccelerateEase };
            Storyboard.SetTarget(fade, target);
            Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
            sb.Children.Add(fade);

            var scaleX = new DoubleAnimation(1.0, TransitionConfig.ScaleFrom, duration) { EasingFunction = TransitionConfig.AccelerateEase };
            Storyboard.SetTarget(scaleX, scale);
            Storyboard.SetTargetProperty(scaleX, new PropertyPath("ScaleX"));
            sb.Children.Add(scaleX);

            var scaleY = new DoubleAnimation(1.0, TransitionConfig.ScaleFrom, duration) { EasingFunction = TransitionConfig.AccelerateEase };
            Storyboard.SetTarget(scaleY, scale);
            Storyboard.SetTargetProperty(scaleY, new PropertyPath("ScaleY"));
            sb.Children.Add(scaleY);
        }

        private static void AddFadeIn(Storyboard sb, FrameworkElement target, Duration duration)
        {
            target.Opacity = 0;
            var fade = new DoubleAnimation(0, 1, duration) { EasingFunction = TransitionConfig.DecelerateEase };
            Storyboard.SetTarget(fade, target);
            Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
            sb.Children.Add(fade);
        }

        private static void AddFadeOut(Storyboard sb, FrameworkElement target, Duration duration)
        {
            var fade = new DoubleAnimation(1, 0, duration) { EasingFunction = TransitionConfig.AccelerateEase };
            Storyboard.SetTarget(fade, target);
            Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
            sb.Children.Add(fade);
        }

        private static void AddSlideUp(Storyboard sb, FrameworkElement target, TranslateTransform translate, Duration duration)
        {
            target.Opacity = 0;
            translate.Y = TransitionConfig.SlideDistance;

            var fade = new DoubleAnimation(0, 1, duration) { EasingFunction = TransitionConfig.DecelerateEase };
            Storyboard.SetTarget(fade, target);
            Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
            sb.Children.Add(fade);

            var slide = new DoubleAnimation(TransitionConfig.SlideDistance, 0, duration) { EasingFunction = TransitionConfig.SmoothEase };
            Storyboard.SetTarget(slide, translate);
            Storyboard.SetTargetProperty(slide, new PropertyPath("Y"));
            sb.Children.Add(slide);
        }

        private static void AddSlideDown(Storyboard sb, FrameworkElement target, TranslateTransform translate, Duration duration)
        {
            var fade = new DoubleAnimation(1, 0, duration) { EasingFunction = TransitionConfig.AccelerateEase };
            Storyboard.SetTarget(fade, target);
            Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
            sb.Children.Add(fade);

            var slide = new DoubleAnimation(0, TransitionConfig.SlideDistance, duration) { EasingFunction = TransitionConfig.AccelerateEase };
            Storyboard.SetTarget(slide, translate);
            Storyboard.SetTargetProperty(slide, new PropertyPath("Y"));
            sb.Children.Add(slide);
        }

        #endregion
    }
}
