using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Utilizr.WPF.Attached
{
    public static class ListViewItemBehaviour
    {
        #region LeftArrowCommand Attached Property
        public static readonly DependencyProperty LeftArrowCommandProperty =
            DependencyProperty.RegisterAttached(
                "LeftArrowCommand",
                typeof(ICommand),
                typeof(ListViewItemBehaviour),
                new PropertyMetadata(default(ICommand), OnLeftArrowCommandChanged)
            );

        static void OnLeftArrowCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListViewItem lvi)
                return;

            if (e.NewValue is not ICommand ic)
                return;

            SetupHandler(lvi, ic, Key.Left);
        }

        public static ICommand GetLeftArrowCommand(ListViewItem obj)
        {
            return (ICommand)obj.GetValue(LeftArrowCommandProperty);
        }

        public static void SetLeftArrowCommand(ListViewItem obj, ICommand val)
        {
            obj.SetValue(LeftArrowCommandProperty, val);
        }
        #endregion


        #region RightArrowCommand Attached Property
        public static readonly DependencyProperty RightArrowCommandProperty =
            DependencyProperty.RegisterAttached(
                "RightArrowCommand",
                typeof(ICommand),
                typeof(ListViewItemBehaviour),
                new PropertyMetadata(default(ICommand), OnRightArrowCommandChanged)
            );

        static void OnRightArrowCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListViewItem lvi)
                return;

            if (e.NewValue is not ICommand ic)
                return;

            SetupHandler(lvi, ic, Key.Right);
        }

        public static ICommand GetRightArrowCommand(ListViewItem obj)
        {
            return (ICommand)obj.GetValue(RightArrowCommandProperty);
        }

        public static void SetRightArrowCommand(ListViewItem obj, ICommand val)
        {
            obj.SetValue(RightArrowCommandProperty, val);
        }
        #endregion


        #region UpArrowCommand Attached Property
        public static readonly DependencyProperty UpArrowCommandProperty =
            DependencyProperty.RegisterAttached(
                "UpArrowCommand",
                typeof(ICommand),
                typeof(ListViewItemBehaviour),
                new PropertyMetadata(default(ICommand), OnUpArrowCommandChanged)
            );

        static void OnUpArrowCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListViewItem lvi)
                return;

            if (e.NewValue is not ICommand ic)
                return;

            SetupHandler(lvi, ic, Key.Up);
        }

        public static ICommand GetUpArrowCommand(ListViewItem obj)
        {
            return (ICommand)obj.GetValue(UpArrowCommandProperty);
        }

        public static void SetUpArrowCommand(ListViewItem obj, ICommand val)
        {
            obj.SetValue(UpArrowCommandProperty, val);
        }
        #endregion


        #region DownArrowCommand Attached Property
        public static readonly DependencyProperty DownArrowCommandProperty =
            DependencyProperty.RegisterAttached(
                "DownArrowCommand",
                typeof(ICommand),
                typeof(ListViewItemBehaviour),
                new PropertyMetadata(default(ICommand), OnDownArrowCommandChanged)
            );

        static void OnDownArrowCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListViewItem lvi)
                return;

            if (e.NewValue is not ICommand ic)
                return;

            SetupHandler(lvi, ic, Key.Down);
        }

        public static ICommand GetDownArrowCommand(ListViewItem obj)
        {
            return (ICommand)obj.GetValue(DownArrowCommandProperty);
        }

        public static void SetDownArrowCommand(ListViewItem obj, ICommand val)
        {
            obj.SetValue(DownArrowCommandProperty, val);
        }
        #endregion


        #region CheckCommand Attached Property
        public static readonly DependencyProperty CheckCommandProperty =
            DependencyProperty.RegisterAttached(
                "CheckCommand",
                typeof(ICommand),
                typeof(ListViewItemBehaviour),
                new PropertyMetadata(default(ICommand), OnCheckCommandChanged)
            );

        static void OnCheckCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListViewItem lvi)
                return;

            if (e.NewValue is not ICommand ic)
                return;

            SetupHandler(lvi, ic, Key.Space);
        }

        public static ICommand GetCheckCommand(ListViewItem obj)
        {
            return (ICommand)obj.GetValue(CheckCommandProperty);
        }

        public static void SetCheckCommand(ListViewItem obj, ICommand val)
        {
            obj.SetValue(CheckCommandProperty, val);
        }
        #endregion


        static void SetupHandler(ListViewItem lvi, ICommand command, Key key)
        {
            lvi.KeyUp += (s, e) =>
            {
                if (e.Key != key)
                    return;

                if (command.CanExecute(lvi))
                {
                    command.Execute(lvi);
                }

                e.Handled = true;
            };
        }
    }
}