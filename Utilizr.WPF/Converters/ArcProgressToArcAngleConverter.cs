using System;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public class ArcProgressToArcAngleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double progress = (double)values[0];
            double minimum = (double)values[1];
            double maximum = (double)values[2];
            double angleToExclude = values[3] as double? ?? 0.0; //Handle DependecyProperty.Unset

            // Hacky: 359.999 since path of 360 will be at the same position, and never draw arc
            return (359.999 - angleToExclude) * (progress / (maximum - minimum));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
