using System.Windows;
using System.Windows.Controls;

namespace Utilizr.WPF.Extension
{
    public static class ScrollViewerEx
    {
        /// <summary>
        /// Scroll the ScrollViewer so that the decendant will be placed at the top of the view.
        /// Will throw InvalidOperationException if the VisualTree has not yet been loaded.
        /// </summary>
        public static void ScrollItemToTop(this ScrollViewer scrollViewer, UIElement item)
        {
            var transform = item.TransformToAncestor(scrollViewer);
            var offsetPoint = transform.Transform(new Point(0,0));
            scrollViewer.ScrollToVerticalOffset(offsetPoint.Y);
        }
    }
}