using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public class InvertEmptyICollectionToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ICollection colVal)
                throw new ArgumentException($"Expected {nameof(value)} to derive from {nameof(ICollection)}");

            return colVal.Count != 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
