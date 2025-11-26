using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Utilizr.WPF.Attached
{
    public static class DataGridBehaviours
    {
        public interface IDataGridCheckBoxToggleModel
        {
            void ToggleCheckState();
        }

        public static readonly DependencyProperty EnableCheckBoxToggleProperty =
            DependencyProperty.RegisterAttached(
                "EnableCheckBoxToggle",
                typeof(bool),
                typeof(DataGridBehaviours),
                new PropertyMetadata(false, OnEnableCheckBoxToggleChanged)
            );

        public static bool GetEnableCheckBoxToggle(DependencyObject element)
        {
            return (bool)element.GetValue(EnableCheckBoxToggleProperty);
        }

        public static void SetEnableCheckBoxToggle(DependencyObject element, bool value)
        {
            element.SetValue(EnableCheckBoxToggleProperty, value);
        }

        private static void OnEnableCheckBoxToggleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if ((bool)e.NewValue)
                    dataGrid.PreviewKeyDown += DataGrid_PreviewKeyDown;
                else
                    dataGrid.PreviewKeyDown -= DataGrid_PreviewKeyDown;
            }
        }

        private static void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is DataGrid dataGrid))
                return;

            if (!(e.Key == Key.Space))
                return;

            if (!(dataGrid.SelectedItem is IDataGridCheckBoxToggleModel checkBoxToggleModel))
                return;

            // SelectedItem can still implement IDataGridCheckBoxToggleModel as it will be index 0 when
            // the keyboard focus moves back up to the header. We have to be sure it's not a DataGrid header.
            // Doesn't appear to be a nice way to do this other than the VisualParent, hmm....

            if (IsInHeader(Keyboard.FocusedElement))
                return;

            if (checkBoxToggleModel != null)
            {
                checkBoxToggleModel.ToggleCheckState();
                e.Handled = true;
            }
        }

        private static bool IsInHeader(IInputElement focusedElement)
        {
            var dep = focusedElement as DependencyObject;
            while (dep != null)
            {
                if (dep is DataGridColumnHeader || dep is DataGridColumnHeadersPresenter)
                    return true;

                dep = VisualTreeHelper.GetParent(dep);
            }
            return false;
        }
    }
}
