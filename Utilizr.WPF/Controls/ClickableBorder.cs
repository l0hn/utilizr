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




        public ClickableBorder()
        {
            MouseLeftButtonUp += (s, e) =>
            {
                if (e.Handled)
                    return;

                if( Command?.CanExecute(CommandParameter) == true)
                    Command?.Execute(CommandParameter);
            };

            MouseLeftButtonDown += (s, e) =>
            {
                if (e.Handled)
                    return;

                if (MouseDownCommand?.CanExecute(MouseDownCommandParameter) == true)
                    MouseDownCommand?.Execute(MouseDownCommandParameter);
            };
        }
    }
}