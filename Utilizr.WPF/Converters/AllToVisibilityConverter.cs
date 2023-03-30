using System;
using System.Linq;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Utilizr.WPF.Converters
{
    public class AllToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool visibile = values.OfType<bool>().All(value => value);

            if (visibile)
                return Visibility.Visible;

            parameter ??= AllToVisibilityConverterType.CollapsedOnFalse; // default

            return (AllToVisibilityConverterType)parameter == AllToVisibilityConverterType.CollapsedOnFalse
                ? Visibility.Collapsed
                : Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public enum AllToVisibilityConverterType
    {
        CollapsedOnFalse,
        HiddenOnFalse
    }
}
