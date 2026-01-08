using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace Utilizr.WPF.Controls
{
    public class ClickableRun : Run
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(ClickableRun),
                new PropertyMetadata(default(ICommand))
            );

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty MouseDownCommandProperty =
            DependencyProperty.Register(
                nameof(MouseDownCommand),
                typeof(ICommand),
                typeof(ClickableRun),
                new PropertyMetadata(default(ICommand))
            );

        public ICommand MouseDownCommand
        {
            get { return (ICommand)GetValue(MouseDownCommandProperty); }
            set { SetValue(MouseDownCommandProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                nameof(CommandParameter),
                typeof(object),
                typeof(ClickableRun),
                new PropertyMetadata(default(object))
            );

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly DependencyProperty MouseDownCommandParameterProperty =
            DependencyProperty.Register(
                nameof(MouseDownCommandParameter ),
                typeof(object),
                typeof(ClickableRun),
                new PropertyMetadata(default(object))
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
                typeof(ClickableRun),
                new PropertyMetadata(default(ICommand), OnEnterKeyPressedCommandPropertyChanged)
            );

        static void OnEnterKeyPressedCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ClickableRun cRun)
            {
                cRun.RegisterEnterKeyChange(e.NewValue != null);
            }
        }

        public ICommand EnterKeyPressedCommand
        {
            get { return (ICommand)GetValue(EnterKeyPressedCommandProperty); }
            set { SetValue(EnterKeyPressedCommandProperty, value); }
        }

        public static readonly DependencyProperty EnterKeyPressedCommandParameterProperty =
            DependencyProperty.Register(
                nameof(EnterKeyPressedCommandParameter),
                typeof(object),
                typeof(ClickableRun),
                new PropertyMetadata(default(object))
            );

        public object EnterKeyPressedCommandParameter
        {
            get { return GetValue(EnterKeyPressedCommandParameterProperty); }
            set { SetValue(EnterKeyPressedCommandParameterProperty, value); }
        }

        public ClickableRun()
        {
            MouseLeftButtonUp += (s, e) =>
            {
                if (Command?.CanExecute(CommandParameter) == true)
                {
                    // Null check, as unlikely, but possibly changed can execute check
                    Command?.Execute(CommandParameter);
                }
            };

            MouseLeftButtonDown += (s, e) =>
            {
                if (MouseDownCommand?.CanExecute(MouseDownCommandParameter) == true)
                {
                    // Null check, as unlikely, but possibly changed can execute check
                    MouseDownCommand?.Execute(MouseDownCommandParameter);
                }
            };
        }

        void RegisterEnterKeyChange(bool subscribe)
        {
            if (subscribe)
            {
                KeyUp += ClickableRun_KeyUp;
            }
            else
            {
                KeyUp -= ClickableRun_KeyUp;
            }
        }

        void ClickableRun_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (EnterKeyPressedCommand?.CanExecute(EnterKeyPressedCommandParameter) == true)
            {
                EnterKeyPressedCommand?.Execute(EnterKeyPressedCommandParameter);
            }
        }
    }
}
