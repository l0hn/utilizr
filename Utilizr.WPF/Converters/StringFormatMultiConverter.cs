using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public class StringFormatMultiConverter : DependencyObject, IMultiValueConverter
    {
        public static readonly DependencyProperty FormatStringProperty =
            DependencyProperty.Register(
                nameof(MultiFormatString),
                typeof(string),
                typeof(StringFormatConverter),
                new PropertyMetadata(default(string))
            );

        public string MultiFormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(MultiFormatString))
                return string.Format((string)parameter, values);

            return string.Format(MultiFormatString, values);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
