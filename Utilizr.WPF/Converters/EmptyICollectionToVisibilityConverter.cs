using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public enum EmptyICollectionToVisibilityConverterType
    {
        CollapsedOnTrue,
        HiddenOnTrue,
    }

    public class EmptyICollectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ICollection colVal)
                throw new ArgumentException($"Expected {nameof(value)} to derive from {nameof(ICollection)}");

            bool isEmpty = colVal.Count == 0;
            var converterType = parameter is EmptyICollectionToVisibilityConverterType
                ? (EmptyICollectionToVisibilityConverterType)parameter
                : EmptyICollectionToVisibilityConverterType.CollapsedOnTrue;


            if (converterType == EmptyICollectionToVisibilityConverterType.CollapsedOnTrue)
            {
                return isEmpty
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }

            return isEmpty
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
