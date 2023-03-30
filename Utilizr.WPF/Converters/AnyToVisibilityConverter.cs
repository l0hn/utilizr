using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public enum AnyToVisibilityConverterType
    {
        CollapsedOnFalse,
        HiddenOnFalse
    }

    public class AnyToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool visibile = values.OfType<bool>().Any(value => value);

            if (visibile)
                return Visibility.Visible;

            parameter ??= AnyToVisibilityConverterType.CollapsedOnFalse; // default

            return (AnyToVisibilityConverterType)parameter == AnyToVisibilityConverterType.CollapsedOnFalse
                ? Visibility.Collapsed
                : Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
