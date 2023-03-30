using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public enum InvertEmptyCollectionToVisibilityConverterType
    {
        CollapsedOnFalse,
        HiddenOnFalse,
    }

    public class InvertEmptyCollectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var colVal = value as ICollection;
            if (colVal == null)
                throw new ArgumentException($"Expected {nameof(value)} to derive from {nameof(ICollection)}");

            bool isEmpty = colVal.Count == 0;
            var converterType = parameter is InvertEmptyCollectionToVisibilityConverterType
                ? (InvertEmptyCollectionToVisibilityConverterType)parameter
                : InvertEmptyCollectionToVisibilityConverterType.CollapsedOnFalse;


            if (converterType == InvertEmptyCollectionToVisibilityConverterType.CollapsedOnFalse)
            {
                return isEmpty
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            return isEmpty
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
