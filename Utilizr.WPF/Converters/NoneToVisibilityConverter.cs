using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public enum NoneToVisibilityConverterType
    {
        CollapsedOnNotNone,
        HiddenOnNotNone
    }

    public class NoneToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible = !values.OfType<bool>().Any(value => value);

            if (visible)
                return Visibility.Visible;

            parameter ??= NoneToVisibilityConverterType.CollapsedOnNotNone; // default

            return (NoneToVisibilityConverterType)parameter == NoneToVisibilityConverterType.CollapsedOnNotNone
                ? Visibility.Collapsed
                : Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
