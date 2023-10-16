using System.Windows;
using System.Windows.Controls;

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
    }
}
