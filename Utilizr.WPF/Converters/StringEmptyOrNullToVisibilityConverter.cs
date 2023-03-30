using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public enum StringEmptyOrNullVisibilityType
    {
        CollapsedOnNull,
        HiddenOnNull,
    }

    public class StringEmptyOrNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            parameter ??= StringEmptyOrNullVisibilityType.CollapsedOnNull;

            bool validString = !string.IsNullOrEmpty(value as string);

            if (validString)
                return Visibility.Visible;

            return ((StringEmptyOrNullVisibilityType)parameter) == StringEmptyOrNullVisibilityType.HiddenOnNull
                ? Visibility.Hidden
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"Cannot convert {value} back to original object.");
        }
    }
}
