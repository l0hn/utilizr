using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Utilizr.WPF.Attached
{
    public static class UIElementKeyDownBehavior
    {
        public static readonly DependencyProperty KeysProperty =
            DependencyProperty.RegisterAttached(
                "Keys",
                typeof(IEnumerable<Key>),
                typeof(UIElementKeyDownBehavior),
                new PropertyMetadata(new List<Key> { Key.Enter })
            );

        public static void SetKeys(DependencyObject element, IEnumerable<Key> value)
        {
            element.SetValue(KeysProperty, value);
        }

        public static IEnumerable<Key> GetKeys(DependencyObject element)
        {
            return (IEnumerable<Key>)element.GetValue(KeysProperty);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(UIElementKeyDownBehavior),
                new PropertyMetadata(null, OnCommandChanged)
            );

        public static void SetCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(CommandProperty, value);
        }

        public static ICommand GetCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(CommandProperty);
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "CommandParameter",
                typeof(object),
                typeof(UIElementKeyDownBehavior),
                new PropertyMetadata(null));

        public static void SetCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(CommandParameterProperty, value);
        }

        public static object GetCommandParameter(DependencyObject element)
        {
            return element.GetValue(CommandParameterProperty);
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement uiElement)
            {
                if (e.NewValue != null)
                {
                    uiElement.PreviewKeyDown += UiElement_PreviewKeyDown;
                }
                else
                {
                    uiElement.PreviewKeyDown -= UiElement_PreviewKeyDown;
                }
            }
        }

        private static void UiElement_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is UIElement uiElement))
                return;

            var keys = GetKeys(uiElement);
            var command = GetCommand(uiElement);
            var parameter = GetCommandParameter(uiElement);

            if (keys != null && command != null)
            {
                foreach (var key in keys)
                {
                    if (e.Key == key)
                    {
                        if (command.CanExecute(parameter))
                        {
                            command.Execute(parameter);
                            e.Handled = true;
                        }
                        break;
                    }
                }
            }
        }
    }
}