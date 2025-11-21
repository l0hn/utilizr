using System.Windows;

namespace Utilizr.WPF.Attached
{
    /// <summary>
    /// The intention here is to restore focus to the UIElement when an action has completed.
    /// E.g. UIElement is disabled while waiting for an async action, restore the focus when it completes.
    /// </summary>
    public static class UIElementFocusRestoreBehaviour
    {
        public static readonly DependencyProperty TrackFocusForRestoreProperty =
            DependencyProperty.RegisterAttached(
                "TrackFocusForRestore",
                typeof(bool),
                typeof(UIElementFocusRestoreBehaviour),
                new PropertyMetadata(false, OnTrackFocusForRestoreChanged));

        public static void SetTrackFocusForRestore(DependencyObject element, bool value)
        {
            element.SetValue(TrackFocusForRestoreProperty, value);
        }

        public static bool GetTrackFocusForRestore(DependencyObject element)
        {
            return (bool)element.GetValue(TrackFocusForRestoreProperty);
        }

        private static void OnTrackFocusForRestoreChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement uiElement && !(bool)e.NewValue)
            {
                uiElement.Focus();
            }
        }
    }
}
