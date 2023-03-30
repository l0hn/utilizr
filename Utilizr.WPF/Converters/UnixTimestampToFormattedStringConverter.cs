using System;
using System.Globalization;
using System.Windows.Data;
using Utilizr.Extensions;

namespace Utilizr.WPF.Converters
{
    public class UnixTimestampToFormattedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime? dt = null;

            if (value is int intValue)
                dt = intValue.ToDateTime();
            else if (value is long longValue)
                dt = longValue.ToDateTime();

            if (parameter == null || (!dt.HasValue))
                throw new ArgumentException($"Expected {nameof(value)} to be either int or long.");

            return dt.Value.ToString((string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}