using System;
using System.Globalization;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public class InvertBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bValue)
                return !bValue;

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bValue)
                return !bValue;

            return false;
        }
    }
}
