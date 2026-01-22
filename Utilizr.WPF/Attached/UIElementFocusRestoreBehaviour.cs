using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Utilizr.Logging;

namespace Utilizr.WPF.Attached
{
    public static class InputTracker
    {
        public static bool LastInputWasKeyboard { get; private set; }

        static InputTracker()
        {
            EventManager.RegisterClassHandler(typeof(UIElement),
                UIElement.PreviewKeyDownEvent,
                new KeyEventHandler((s, e) => LastInputWasKeyboard = true));

            EventManager.RegisterClassHandler(typeof(UIElement),
                UIElement.PreviewMouseDownEvent,
                new MouseButtonEventHandler((s, e) => LastInputWasKeyboard = false));
        }
    }

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
            if (d is UIElement uiElement)
            {
                bool canFocus(UIElement uiElement) => uiElement.IsEnabled && uiElement.Focusable && InputTracker.LastInputWasKeyboard;
                if (canFocus(uiElement))
                {
                    uiElement.Focus();
                    Keyboard.Focus(uiElement);
                }
                else
                {
                    void resetFocusDispatcher()
                    {
                        try
                        {
                            uiElement.Dispatcher.BeginInvoke(
                                () =>
                                {
                                    uiElement.Focus();
                                    Keyboard.Focus(uiElement);
                                },
                                System.Windows.Threading.DispatcherPriority.Input
                            );
                        }
                        catch (Exception ex)
                        {
                            Log.Exception(nameof(UIElementFocusRestoreBehaviour), ex);
                        }
                    }

                    void uiElement_IsEnabledOrFocusChanged(object sender, DependencyPropertyChangedEventArgs e)
                    {
                        if (canFocus(uiElement))
                        {
                            uiElement.FocusableChanged -= uiElement_IsEnabledOrFocusChanged;
                            uiElement.IsEnabledChanged -= uiElement_IsEnabledOrFocusChanged;
                            resetFocusDispatcher();
                        }
                    }

                    uiElement.FocusableChanged -= uiElement_IsEnabledOrFocusChanged;
                    uiElement.IsEnabledChanged += uiElement_IsEnabledOrFocusChanged;

                    // double check for race condition, safe if we attempt to unregister twice
                    if (canFocus(uiElement))
                    {
                        uiElement.FocusableChanged -= uiElement_IsEnabledOrFocusChanged;
                        uiElement.IsEnabledChanged -= uiElement_IsEnabledOrFocusChanged;
                        resetFocusDispatcher();
                    }
                }
            }
        }
    }
}