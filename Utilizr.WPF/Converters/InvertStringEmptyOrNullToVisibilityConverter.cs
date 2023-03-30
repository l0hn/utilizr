using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public class InvertStringEmptyOrNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            parameter ??= InvertStringEmptyOrNullVisibilityType.CollapsedOnValue;

            var emptyOrNullString = string.IsNullOrEmpty(value as string);

            if (emptyOrNullString)
                return Visibility.Visible;

            return ((InvertStringEmptyOrNullVisibilityType)parameter) == InvertStringEmptyOrNullVisibilityType.HiddenOnValue
                ? Visibility.Hidden
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"Cannot convert {value} back to original object.");
        }
    }

    public enum InvertStringEmptyOrNullVisibilityType
    {
        CollapsedOnValue,
        HiddenOnValue,
    }
}
