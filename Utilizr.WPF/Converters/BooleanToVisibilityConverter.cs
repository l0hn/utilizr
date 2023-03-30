using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public enum BooleanToVisibilityConverterType
    {
        CollapsedOnFalse,
        HiddenOnFalse,
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visibile = (bool)value;

            if (visibile)
                return Visibility.Visible;

            return parameter is BooleanToVisibilityConverterType && ((BooleanToVisibilityConverterType)parameter) == BooleanToVisibilityConverterType.HiddenOnFalse
                ? Visibility.Hidden
                : (object)Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;

            if (visibility == Visibility.Visible)
                return true;

            return false;
        }
    }
}