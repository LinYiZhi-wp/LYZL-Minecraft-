using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace GeminiLauncher.Controls
{
    public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
    {
        private double _extentWidth;
        private double _extentHeight;
        private double _viewportWidth;
        private double _viewportHeight;
        private double _offsetX;
        private double _offsetY;
        private Size _lastAvailableSize;

        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(VirtualizingWrapPanel),
                new FrameworkPropertyMetadata(220.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(VirtualizingWrapPanel),
                new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        private int Columns => _viewportWidth > 0 ? Math.Max(1, (int)Math.Floor(_viewportWidth / ItemWidth)) : 1;

        protected override Size MeasureOverride(Size availableSize)
        {
            _lastAvailableSize = availableSize;
            _viewportWidth = availableSize.Width;
            _viewportHeight = availableSize.Height;

            var itemsControl = ItemsControl.GetItemsOwner(this);
            var items = itemsControl?.Items;
            int count = items?.Count ?? 0;
            int cols = Columns;

            double maxItemHeight = 0;
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];
                child.Measure(new Size(ItemWidth, double.PositiveInfinity));
                maxItemHeight = Math.Max(maxItemHeight, child.DesiredSize.Height);
            }

            double rh = !double.IsNaN(ItemHeight) ? ItemHeight : (maxItemHeight > 0 ? maxItemHeight : 250);
            int rows = count > 0 ? (count + cols - 1) / cols : 0;

            _extentWidth = cols * ItemWidth;
            _extentHeight = rows * rh;

            if (_offsetY + _viewportHeight > _extentHeight)
                _offsetY = Math.Max(0, _extentHeight - _viewportHeight);

            return new Size(Math.Min(_extentWidth, availableSize.Width), Math.Min(_extentHeight, availableSize.Height));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var itemsControl = ItemsControl.GetItemsOwner(this);
            var items = itemsControl?.Items;
            int count = items?.Count ?? 0;
            int cols = Columns;

            double maxItemHeight = 0;
            for (int i = 0; i < InternalChildren.Count; i++)
                maxItemHeight = Math.Max(maxItemHeight, InternalChildren[i].DesiredSize.Height);

            double rh = !double.IsNaN(ItemHeight) ? ItemHeight : (maxItemHeight > 0 ? maxItemHeight : 250);

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];
                int row = i / cols;
                int col = i % cols;
                double x = col * ItemWidth;
                double y = row * rh - _offsetY;
                child.Arrange(new Rect(x, y, ItemWidth, child.DesiredSize.Height));
            }

            return finalSize;
        }

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            base.OnItemsChanged(sender, args);
            InvalidateMeasure();
        }

        #region IScrollInfo

        public ScrollViewer? ScrollOwner { get; set; }
        public bool CanHorizontallyScroll { get; set; } = false;
        public bool CanVerticallyScroll { get; set; } = true;
        public double ExtentWidth => _extentWidth;
        public double ExtentHeight => _extentHeight;
        public double ViewportWidth => _viewportWidth;
        public double ViewportHeight => _viewportHeight;
        public double HorizontalOffset => _offsetX;
        public double VerticalOffset => _offsetY;

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return rectangle;
        }

        public void LineUp() { SetVerticalOffset(_offsetY - 30); }
        public void LineDown() { SetVerticalOffset(_offsetY + 30); }
        public void LineLeft() { }
        public void LineRight() { }
        public void PageUp() { SetVerticalOffset(_offsetY - _viewportHeight); }
        public void PageDown() { SetVerticalOffset(_offsetY + _viewportHeight); }
        public void PageLeft() { }
        public void PageRight() { }
        public void MouseWheelUp() { SetVerticalOffset(_offsetY - 60); }
        public void MouseWheelDown() { SetVerticalOffset(_offsetY + 60); }
        public void MouseWheelLeft() { }
        public void MouseWheelRight() { }

        public void SetHorizontalOffset(double offset)
        {
            _offsetX = Math.Max(0, Math.Min(offset, _extentWidth - _viewportWidth));
            InvalidateMeasure();
            ScrollOwner?.InvalidateScrollInfo();
        }

        public void SetVerticalOffset(double offset)
        {
            _offsetY = Math.Max(0, Math.Min(offset, _extentHeight - _viewportHeight));
            InvalidateMeasure();
            ScrollOwner?.InvalidateScrollInfo();
        }

        #endregion
    }
}
