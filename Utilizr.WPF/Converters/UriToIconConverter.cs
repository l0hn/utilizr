using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Utilizr.WPF.Util;

namespace Utilizr.WPF.Converters
{
    public class UriToIconConverter : MarkupExtension, IValueConverter
    {
        private static UriToIconConverter? _instance;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            _instance ??= new UriToIconConverter();
            return _instance;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ResourceHelper.GetIconSource((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
