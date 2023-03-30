using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public enum NullReferenceVisibilityType
    {
        CollapsedOnNull,
        HiddenOnNull,
    }

    public class NullReferenceToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            parameter ??= NullReferenceVisibilityType.CollapsedOnNull;

            if (value != null)
                return Visibility.Visible;

            return ((NullReferenceVisibilityType)parameter) == NullReferenceVisibilityType.HiddenOnNull
                ? Visibility.Hidden
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"Cannot convert {value} back to original object.");
        }
    }
}