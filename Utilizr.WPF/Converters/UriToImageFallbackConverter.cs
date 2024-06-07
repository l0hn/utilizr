using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Utilizr.WPF.Util;

namespace Utilizr.WPF.Converters
{
    public class UriToImageFallbackConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fullFilePath = (string)value;
            string fallbackIcon = (string)parameter;

            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() == true)
                return ResourceHelper.GetImageSource(fallbackIcon);
            
            BitmapImage bitmap = new BitmapImage();
            try
            {
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
                bitmap.EndInit();
            } 
            catch 
            {
                return ResourceHelper.GetImageSource(fallbackIcon);
            }

            return bitmap;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
