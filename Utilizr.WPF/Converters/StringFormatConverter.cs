using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public class StringFormatConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty FormatStringProperty =
            DependencyProperty.Register(
                nameof(FormatString),
                typeof(string),
                typeof(StringFormatConverter),
                new PropertyMetadata(default(string))
            );

        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(FormatString))
                return string.Format((string)parameter, value);

            return string.Format(FormatString, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
