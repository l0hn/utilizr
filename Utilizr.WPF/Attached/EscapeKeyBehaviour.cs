using System;
using System.Windows;
using System.Windows.Input;

namespace Utilizr.WPF.Attached
{
    public static class EscapeKeyBehaviour
    {
        public static readonly DependencyProperty EnableEscapeKeyProperty =
            DependencyProperty.RegisterAttached(
                "EnableEscapeKey",
                typeof(bool),
                typeof(EscapeKeyBehaviour),
                new PropertyMetadata(false, OnEnableEscapeKeyChanged));

        public static void SetEnableEscapeKey(UIElement element, bool value)
        {
            element.SetValue(EnableEscapeKeyProperty, value);
        }

        public static bool GetEnableEscapeKey(UIElement element)
        {
            return (bool)element.GetValue(EnableEscapeKeyProperty);
        }

        public static readonly DependencyProperty EscapeKeyActionProperty =
            DependencyProperty.RegisterAttached(
                "EscapeKeyAction",
                typeof(Action<UIElement>),
                typeof(EscapeKeyBehaviour),
                new PropertyMetadata(null));

        public static void SetEscapeKeyAction(UIElement element, Action<UIElement> value)
        {
            element.SetValue(EscapeKeyActionProperty, value);
        }

        public static Action<UIElement> GetEscapeKeyAction(UIElement element)
        {
            return (Action<UIElement>)element.GetValue(EscapeKeyActionProperty);
        }

        private static void OnEnableEscapeKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement uiElement)
            {
                if ((bool)e.NewValue)
                {
                    uiElement.PreviewKeyDown += OnPreviewKeyDown;
                }
                else
                {
                    uiElement.PreviewKeyDown -= OnPreviewKeyDown;
                }
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && sender is UIElement element)
            {
                var action = GetEscapeKeyAction(element);
                action?.Invoke(element);
            }
        }
    }
}
