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
        }
    }
}
