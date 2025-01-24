using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Utilizr.WPF.Controls
{
    public class ClickableBorder : Border
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(ClickableBorder),
                new PropertyMetadata(default(ICommand))
            );

        /// <summary>
        /// MouseLeftButtonUp Command
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }


        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                nameof(CommandParameter),
                typeof(object),
                typeof(ClickableBorder),
                new PropertyMetadata(default(object))
            );

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }



        public static readonly DependencyProperty MouseDownCommandProperty =
            DependencyProperty.Register(
                nameof(MouseDownCommand),
                typeof(ICommand),
                typeof(ClickableBorder),
                new PropertyMetadata(default(ICommand))
            );

        /// <summary>
        /// MouseLeftButtonDown command
        /// </summary>
        public ICommand MouseDownCommand
        {
            get { return (ICommand)GetValue(MouseDownCommandProperty); }
            set { SetValue(MouseDownCommandProperty, value); }
        }


        public static readonly DependencyProperty MouseDownCommandParameterProperty =
            DependencyProperty.Register(
                nameof(MouseDownCommandParameter),
                typeof(object),
                typeof(ClickableBorder),
                new PropertyMetadata(null)
            );

        public object MouseDownCommandParameter
        {
            get { return GetValue(MouseDownCommandParameterProperty); }
            set { SetValue(MouseDownCommandParameterProperty, value); }
        }


        public static readonly DependencyProperty EnterKeyPressedCommandProperty =
            DependencyProperty.Register(
                nameof(EnterKeyPressedCommand),
                typeof(ICommand),
                typeof(ClickableBorder),
                new PropertyMetadata(default)
            );

        public ICommand EnterKeyPressedCommand
        {
            get { return (ICommand)GetValue(EnterKeyPressedCommandProperty); }
            set { SetValue(EnterKeyPressedCommandProperty, value); }
        }



        public static readonly DependencyProperty EnterKeyPressedCommandParameterProperty =
            DependencyProperty.Register(
                nameof(EnterKeyPressedCommandParameter),
                typeof(object),
                typeof(ClickableBorder),
                new PropertyMetadata(default)
            );

        public object EnterKeyPressedCommandParameter
        {
            get { return (object)GetValue(EnterKeyPressedCommandParameterProperty); }
            set { SetValue(EnterKeyPressedCommandParameterProperty, value); }
        }



        public ClickableBorder()
        {
            MouseLeftButtonUp += (s, e) =>
            {
                if (e.Handled)
                    return;

                if (Command?.CanExecute(CommandParameter) == true)
                {
                    Command?.Execute(CommandParameter);
                    e.Handled = true;
                }
            };

            MouseLeftButtonDown += (s, e) =>
            {
                if (e.Handled)
                    return;

                if (MouseDownCommand?.CanExecute(MouseDownCommandParameter) == true)
                {
                    MouseDownCommand?.Execute(MouseDownCommandParameter);
                    e.Handled = true;
                }
            };

            KeyUp += (s, e) =>
            {
                if (e.Key != Key.Enter)
                    return;

                if (e.Handled)
                    return;

                if (EnterKeyPressedCommand?.CanExecute(EnterKeyPressedCommandParameter) == true)
                {
                    EnterKeyPressedCommand?.Execute(EnterKeyPressedCommandParameter);
                    e.Handled = true;
                }
            };
        }
    }
}