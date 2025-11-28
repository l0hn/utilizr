using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Utilizr.WPF.Attached
{
    public static class ListBoxItemBehaviour
    {
        #region LeftArrowCommand Attached Property
        public static readonly DependencyProperty LeftArrowCommandProperty =
            DependencyProperty.RegisterAttached(
                "LeftArrowCommand",
                typeof(ICommand),
                typeof(ListBoxItemBehaviour),
                new PropertyMetadata(default(ICommand), OnLeftArrowCommandChanged)
            );

        static void OnLeftArrowCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBoxItem lbi)
                return;

            if (e.NewValue is not ICommand ic)
                return;

            SetupHandler(lbi, ic, GetLeftArrowCommandParameter(lbi), Key.Left);
        }

        public static ICommand GetLeftArrowCommand(ListBoxItem obj)
        {
            return (ICommand)obj.GetValue(LeftArrowCommandProperty);
        }

        public static void SetLeftArrowCommand(ListBoxItem obj, ICommand val)
        {
            obj.SetValue(LeftArrowCommandProperty, val);
        }
        #endregion

        #region LeftArrowCommandParameter Attached Property
        public static readonly DependencyProperty LeftArrowCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "LeftArrowCommandParameter",
                typeof(ICommand),
                typeof(ListBoxItemBehaviour),
                new PropertyMetadata(default(object))
            );

        public static object GetLeftArrowCommandParameter(ListBoxItem obj)
        {
            return (object)obj.GetValue(LeftArrowCommandParameterProperty);
        }

        public static void SetLeftArrowCommandParameter(ListBoxItem obj, object val)
        {
            obj.SetValue(LeftArrowCommandParameterProperty, val);
        }
        #endregion


        #region RightArrowCommand Attached Property
        public static readonly DependencyProperty RightArrowCommandProperty =
            DependencyProperty.RegisterAttached(
                "RightArrowCommand",
                typeof(ICommand),
                typeof(ListBoxItemBehaviour),
                new PropertyMetadata(default(ICommand), OnRightArrowCommandChanged)
            );

        static void OnRightArrowCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBoxItem lbi)
                return;

            if (e.NewValue is not ICommand ic)
                return;

            SetupHandler(lbi, ic, GetRightArrowCommandParameter(lbi), Key.Right);
        }

        public static ICommand GetRightArrowCommand(ListBoxItem obj)
        {
            return (ICommand)obj.GetValue(RightArrowCommandProperty);
        }

        public static void SetRightArrowCommand(ListBoxItem obj, ICommand val)
        {
            obj.SetValue(RightArrowCommandProperty, val);
        }
        #endregion

        #region RightArrowCommandParameter Attached Property
        public static readonly DependencyProperty RightArrowCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "RightArrowCommandParameter",
                typeof(object),
                typeof(ListBoxItemBehaviour),
                new PropertyMetadata(default(object))
            );

        public static object GetRightArrowCommandParameter(ListBoxItem obj)
        {
            return (object)obj.GetValue(RightArrowCommandParameterProperty);
        }

        public static void SetRightArrowCommandParameter(ListBoxItem obj, object val)
        {
            obj.SetValue(RightArrowCommandParameterProperty, val);
        }
        #endregion


        #region UpArrowCommand Attached Property
        public static readonly DependencyProperty UpArrowCommandProperty =
            DependencyProperty.RegisterAttached(
                "UpArrowCommand",
                typeof(ICommand),
                typeof(ListBoxItemBehaviour),
                new PropertyMetadata(default(ICommand), OnUpArrowCommandChanged)
            );

        static void OnUpArrowCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBoxItem lbi)
                return;

            if (e.NewValue is not ICommand ic)
                return;

            SetupHandler(lbi, ic, GetUpArrowCommandParameter(lbi), Key.Up);
        }

        public static ICommand GetUpArrowCommand(ListBoxItem obj)
        {
            return (ICommand)obj.GetValue(UpArrowCommandProperty);
        }

        public static void SetUpArrowCommand(ListBoxItem obj, ICommand val)
        {
            obj.SetValue(UpArrowCommandProperty, val);
        }
        #endregion

        #region UpArrowCommandParameter Attached Property
        public static readonly DependencyProperty UpArrowCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "UpArrowCommandParameter",
                typeof(object),
                typeof(ListBoxItemBehaviour),
                new PropertyMetadata(default(object))
            );

        public static object GetUpArrowCommandParameter(ListBoxItem obj)
        {
            return (object)obj.GetValue(UpArrowCommandParameterProperty);
        }

        public static void SetUpArrowCommandParameter(ListBoxItem obj, object val)
        {
            obj.SetValue(UpArrowCommandParameterProperty, val);
        }
        #endregion


        #region DownArrowCommand Attached Property
        public static readonly DependencyProperty DownArrowCommandProperty =
            DependencyProperty.RegisterAttached(
                "DownArrowCommand",
                typeof(ICommand),
                typeof(ListBoxItemBehaviour),
                new PropertyMetadata(default(ICommand), OnDownArrowCommandChanged)
            );

        static void OnDownArrowCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBoxItem lbi)
                return;

            if (e.NewValue is not ICommand ic)
                return;

            SetupHandler(lbi, ic, GetDownArrowCommandParameter(lbi), Key.Down);
        }

        public static ICommand GetDownArrowCommand(ListBoxItem obj)
        {
            return (ICommand)obj.GetValue(DownArrowCommandProperty);
        }

        public static void SetDownArrowCommand(ListBoxItem obj, ICommand val)
        {
            obj.SetValue(DownArrowCommandProperty, val);
        }
        #endregion

        #region DownArrowCommandParameter Attached Property
        public static readonly DependencyProperty DownArrowCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "DownArrowCommandParameter",
                typeof(object),
                typeof(ListBoxItemBehaviour),
                new PropertyMetadata(default(object))
            );

        public static object GetDownArrowCommandParameter(ListBoxItem obj)
        {
            return (object)obj.GetValue(DownArrowCommandParameterProperty);
        }

        public static void SetDownArrowCommandParameter(ListBoxItem obj, object val)
        {
            obj.SetValue(DownArrowCommandParameterProperty, val);
        }
        #endregion


        #region CheckCommand Attached Property
        public static readonly DependencyProperty CheckCommandProperty =
            DependencyProperty.RegisterAttached(
                "CheckCommand",
                typeof(ICommand),
                typeof(ListBoxItemBehaviour),
                new PropertyMetadata(default(ICommand), OnCheckCommandChanged)
            );

        static void OnCheckCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBoxItem lbi)
                return;

            if (e.NewValue is not ICommand ic)
                return;

            SetupHandler(lbi, ic, GetCheckCommandParameter(lbi), Key.Space);
        }

        public static ICommand GetCheckCommand(ListBoxItem obj)
        {
            return (ICommand)obj.GetValue(CheckCommandProperty);
        }

        public static void SetCheckCommand(ListBoxItem obj, ICommand val)
        {
            obj.SetValue(CheckCommandProperty, val);
        }
        #endregion

        #region CheckCommandParameter Attached Property
        public static readonly DependencyProperty CheckCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "CheckCommandParameter",
                typeof(object),
                typeof(ListBoxItemBehaviour),
                new PropertyMetadata(default(object))
            );

        public static object GetCheckCommandParameter(ListBoxItem obj)
        {
            return (object)obj.GetValue(CheckCommandParameterProperty);
        }

        public static void SetCheckCommandParameter(ListBoxItem obj, object val)
        {
            obj.SetValue(CheckCommandParameterProperty, val);
        }
        #endregion


        #region EnterCommand Attached Property
        public static readonly DependencyProperty EnterCommandProperty =
            DependencyProperty.RegisterAttached(
                "EnterCommand",
                typeof(ICommand),
                typeof(ListBoxItemBehaviour),
                new PropertyMetadata(default(ICommand), OnEnterCommandChanged)
            );

        static void OnEnterCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBoxItem lbi)
                return;

            if (e.NewValue is not ICommand ic)
                return;

            SetupHandler(lbi, ic, GetCheckCommandParameter(lbi), Key.Enter);
        }

        public static ICommand GetEnterCommand(ListBoxItem obj)
        {
            return (ICommand)obj.GetValue(EnterCommandProperty);
        }

        public static void SetEnterCommand(ListBoxItem obj, ICommand val)
        {
            obj.SetValue(EnterCommandProperty, val);
        }
        #endregion

        #region EnterCommandParameter Attached Property
        public static readonly DependencyProperty EnterCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "EnterCommandParameter",
                typeof(object),
                typeof(ListBoxItemBehaviour),
                new PropertyMetadata(default(object))
            );

        public static object GetEnterCommandParameter(ListBoxItem obj)
        {
            return (object)obj.GetValue(EnterCommandParameterProperty);
        }

        public static void SetEnterCommandParameter(ListBoxItem obj, object val)
        {
            obj.SetValue(EnterCommandParameterProperty, val);
        }
        #endregion


        static void SetupHandler(ListBoxItem lbi, ICommand command, object commandParameter, Key key)
        {
            lbi.KeyUp += (s, e) =>
            {
                if (e.Key != key)
                    return;

                if (command.CanExecute(commandParameter))
                {
                    command.Execute(commandParameter);
                }

                e.Handled = true;
            };
        }
    }
}