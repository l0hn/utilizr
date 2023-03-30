using System;
using System.Globalization;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public class InvertBooleanToVisibilityConverter : IValueConverter
    {
        private readonly BooleanToVisibilityConverter _converter;

        public InvertBooleanToVisibilityConverter()
        {
            _converter = new BooleanToVisibilityConverter();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _converter.Convert(!((bool)value), targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
           return _converter.ConvertBack(value, targetType, parameter, culture);
        }
    }
}
