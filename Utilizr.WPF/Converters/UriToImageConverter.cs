using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Utilizr.WPF.Util;

namespace Utilizr.WPF.Converters
{
    public class UriToImageConverter: MarkupExtension, IValueConverter
    {
        private static UriToImageConverter? _converter;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            _converter ??= new UriToImageConverter();
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ResourceHelper.GetImageSource((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
