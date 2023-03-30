using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public class DoubleToThicknessConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var doubleValue = (double)value;
            return new Thickness(doubleValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var thickness = (Thickness)value;
            return thickness.Bottom; //uniform, so any property
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values.Length != 4)
                throw new ArgumentException($"{nameof(values)} array does not have 4 items. It has {values.Length} items.");

            return new Thickness(
                (double)values[0],
                (double)values[1],
                (double)values[2],
                (double)values[3]
            );
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            var thickness = (Thickness)value;
            return new object[] { thickness.Left, thickness.Top, thickness.Right, thickness.Bottom };
        }
    }
}
