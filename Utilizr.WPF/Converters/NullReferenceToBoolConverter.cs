using System;
using System.Globalization;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public enum NullReferenceToBoolConverterType
    {
        TrueWhenNull,
        FalseWhenNull,
    }

    public class NullReferenceToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            parameter ??= NullReferenceToBoolConverterType.TrueWhenNull; // default

            if ((NullReferenceToBoolConverterType)parameter == NullReferenceToBoolConverterType.TrueWhenNull)
                return value == null;

            // FalseWhenNull
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
