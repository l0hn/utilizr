using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public class ObjectIsEqualToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                throw new ArgumentException($"You must provide a value for {nameof(parameter)} for {nameof(ObjectIsEqualToBooleanConverter)}.{nameof(Convert)}");

            return value == parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ObjectsAreEqualToBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var first = values.First();
            return values.All(first.Equals);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}