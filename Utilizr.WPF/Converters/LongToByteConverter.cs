using System;
using System.Globalization;
using System.Windows.Data;
using Utilizr.Globalisation.Extensions;

namespace Utilizr.WPF.Converters
{
    public class LongToByteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return ((long)value).ToBytesString();

            return ((long)value).ToBytesString((int)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
