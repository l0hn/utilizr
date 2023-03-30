using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public enum InvertNullReferenceVisibilityType
    {
        CollapsedOnInstance,
        HiddenOnInstance,
    }

    public class InvertNullReferenceToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            parameter ??= InvertNullReferenceVisibilityType.CollapsedOnInstance; // default

            if (value == null)
                return Visibility.Visible;

            return ((InvertNullReferenceVisibilityType)parameter) == InvertNullReferenceVisibilityType.HiddenOnInstance
                ? Visibility.Hidden
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
