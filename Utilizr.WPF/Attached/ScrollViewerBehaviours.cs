using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Utilizr.WPF.Attached
{
    public static class ScrollViewerBehaviours
    {
        public static readonly DependencyProperty ScrollHorizontalOffsetProperty =
           DependencyProperty.RegisterAttached(
               "ScrollHorizontalOffset",
               typeof(double),
               typeof(ScrollViewerBehaviours),
               new FrameworkPropertyMetadata(0.0, OnScrollHorizontalOffsetChanged)
            );

        public static void OnScrollHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScrollViewer scrollViewer)
                return;

            if (e.NewValue is not double offset)
                return;

            scrollViewer.ScrollToHorizontalOffset(offset);
        }

        public static double GetScrollHorizontalOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(ScrollHorizontalOffsetProperty);
        }

        public static void SetScrollHorizontalOffset(DependencyObject obj, double value)
        {
            obj.SetValue(ScrollHorizontalOffsetProperty, value);
        }



        public static readonly DependencyProperty AutoScrollToTopProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollToTop",
                typeof(bool),
                typeof(ScrollViewerBehaviours),
                new PropertyMetadata(false, OnAutoScrollTopChanged)
            );

        private static void OnAutoScrollTopChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScrollViewer scrollViewer)
                return;

            if (e.NewValue is not bool bNewValue)
                return; // binding not yet setup

            if (bNewValue)
            {
                scrollViewer.ScrollToTop();
                d.SetCurrentValue(AutoScrollToTopProperty, false);
            }
        }

        public static bool GetAutoSrollToTop(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollToTopProperty);
        }

        public static void SetAutoScrollToTop(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollToTopProperty, value);
        }




        #region ApplyCutOffOpacity Attached Property
        public static readonly DependencyProperty ApplyCutOffOpacityProperty =
            DependencyProperty.RegisterAttached(
                "ApplyCutOffOpacity",
                typeof(bool),
                typeof(ScrollViewerBehaviours),
                new PropertyMetadata(false, OnApplyCutOffOpacityChanged)
            );

        private static void OnApplyCutOffOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScrollViewer scrollViewer)
                return;

            bool newValue = (bool)e.NewValue;
            bool oldValue = (bool)e.OldValue;

            void loaded(object sender, RoutedEventArgs e) => UpdateOpacityMask(scrollViewer);
            void sizeChanged(object sender, SizeChangedEventArgs e) => UpdateOpacityMask(scrollViewer);

            if (oldValue && !newValue)
            {
                scrollViewer.SizeChanged -= sizeChanged;
                scrollViewer.Loaded -= loaded;
            }

            if (!oldValue && newValue)
            {
                scrollViewer.SizeChanged += sizeChanged;
                scrollViewer.Loaded += loaded;
                UpdateOpacityMask(scrollViewer);
            }
        }

        public static bool GetApplyCutOffOpacity(ScrollViewer obj)
        {
            return (bool)obj.GetValue(ApplyCutOffOpacityProperty);
        }

        public static void SetApplyCutOffOpacity(ScrollViewer obj, bool val)
        {
            obj.SetValue(ApplyCutOffOpacityProperty, val);
        }
        #endregion


        static Brush? _solidBrush;
        static Pen? _pen;
        static Brush? _fadeBrush;

        static void UpdateOpacityMask(ScrollViewer scrollViewer)
        {
            if (!scrollViewer.IsLoaded)
                return;

            // todo: Expose the height of the fade, and the width of the scroll viewer
            const double fadeHeight = 36; // 3
            const int scrollBarWidth = 10; // 2

            var width = scrollViewer.ActualWidth;
            var height = scrollViewer.ActualHeight;

            if (double.IsNaN(scrollViewer.ActualHeight) ||
                double.IsNaN(scrollViewer.ActualWidth) ||
                scrollViewer.ActualHeight - fadeHeight < 1 ||
                scrollViewer.ActualHeight - scrollBarWidth < 1)
            {
                scrollViewer.OpacityMask = null;
                return;
            }

            //  ______________
            // |            | |
            // |            | |
            // |      1     |2|
            // |            | |
            // |____________|_|
            // |      3     |2|
            //  --------------

            // 1 main area, 100% opacity
            // 2 scroll bar area, 100% opacity
            // 3 faded opacity, linear with 0% and 100% gradient stops

            var drawingBrush = new DrawingBrush()
            {
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Center,
                AlignmentY = AlignmentY.Top,
                ViewboxUnits = BrushMappingMode.RelativeToBoundingBox,
            };

            _solidBrush ??= new SolidColorBrush(Colors.White);
            _pen ??= new Pen(_solidBrush, 0);
            _fadeBrush ??= new LinearGradientBrush(
                new GradientStopCollection(
                    new List<GradientStop>()
                    {
                        new GradientStop(Colors.White, 0),
                        new GradientStop(Colors.Transparent, 1)
                    }
                ),
                startPoint: new Point(0.5, 0),
                endPoint: new Point(0.5, 1)
            );

            var drawingGroup = new DrawingGroup();
            drawingGroup.Children.Add(new GeometryDrawing(_solidBrush, _pen, new RectangleGeometry(new Rect(0, 0, width, height - fadeHeight)))); //1
            drawingGroup.Children.Add(new GeometryDrawing(_solidBrush, _pen, new RectangleGeometry(new Rect(width - scrollBarWidth, 0, scrollBarWidth, height)))); //2
            drawingGroup.Children.Add(new GeometryDrawing(_fadeBrush, _pen, new RectangleGeometry(new Rect(0, height - fadeHeight, width, fadeHeight)))); //3

            drawingBrush.Drawing = drawingGroup;
            scrollViewer.OpacityMask = drawingBrush;
        }
    }
}
