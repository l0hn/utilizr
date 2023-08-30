using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Utilizr.WPF.Util;

namespace Utilizr.WPF.Converters
{
    public class UriToImageFallbackConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fullFilePath = (string)value;

            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() == true)
                return ResourceHelper.GetImageSource("logo-grey-error@2x.png");
            
            BitmapImage bitmap = new BitmapImage();
            try
            {
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
                bitmap.EndInit();
            } 
            catch 
            {
                return ResourceHelper.GetImageSource("logo-grey-error@2x.png");
            }

            return bitmap;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
