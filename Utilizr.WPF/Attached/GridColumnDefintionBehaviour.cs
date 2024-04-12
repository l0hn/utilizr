using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Utilizr.WPF.Attached
{
    public class GridColumnDefintionBehaviour
    {

        #region BindableWidth Attached Property
        public static readonly DependencyProperty BindableWidthProperty =
            DependencyProperty.RegisterAttached(
                "BindableWidth",
                typeof(double),
                typeof(GridColumnDefintionBehaviour),
                new PropertyMetadata(double.NaN, OnBindableWidthChanged)
            );

        public static double GetBindableWidth(ColumnDefinition obj)
        {
            return (double)obj.GetValue(BindableWidthProperty);
        }

        public static void SetBindableWidth(ColumnDefinition obj, double val)
        {
            obj.SetValue(BindableWidthProperty, val);
        }

        static void OnBindableWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ColumnDefinition columnDefinition)
                return;

            if (e.NewValue is not double newWidth)
                return;

            if (double.IsNaN(newWidth) || newWidth < 0)
            {
                columnDefinition.Width = new GridLength(1, GridUnitType.Star);
                return;
            }

            columnDefinition.Width = new GridLength(newWidth, GridUnitType.Pixel);
        }
        #endregion

    }
}
