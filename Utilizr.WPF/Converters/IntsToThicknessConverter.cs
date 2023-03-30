using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public class IntsToThicknessConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == DependencyProperty.UnsetValue)
                    values[i] = 0;
            }

            double left = System.Convert.ToDouble(values[0]);
            double top = System.Convert.ToDouble(values[1]);
            double right = System.Convert.ToDouble(values[2]);
            double bottom = System.Convert.ToDouble(values[3]);

            return new Thickness(left, top, right, bottom);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            var thickness = (Thickness)value;

            return new object[]
            {
                thickness.Left,
                thickness.Top,
                thickness.Right,
                thickness.Bottom,
            };
        }
    }
}
