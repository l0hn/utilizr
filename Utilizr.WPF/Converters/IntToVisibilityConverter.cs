using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public enum IntToVisibilityConverterType
    {
        CollapsedOnZero,
        HiddenOnZero
    }

    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int intValue = (int)value;

            if (intValue != 0)
                return Visibility.Visible;

            parameter ??= IntToVisibilityConverterType.CollapsedOnZero; // default

            return (IntToVisibilityConverterType)parameter == IntToVisibilityConverterType.CollapsedOnZero
                ? Visibility.Collapsed
                : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
