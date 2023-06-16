using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Utilizr.WPF.Attached
{
    public static class ScrollViewerBehaviours
    {
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
