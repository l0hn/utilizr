using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public class IntsToRectOffset
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }

    public class IntsToRectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var offset = (IntsToRectOffset)parameter ?? new IntsToRectOffset();

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == DependencyProperty.UnsetValue)
                    values[i] = 0;
            }

            double x = System.Convert.ToDouble(values[0]);
            double y = System.Convert.ToDouble(values[1]);
            double width = System.Convert.ToDouble(values[2]);
            double height = System.Convert.ToDouble(values[3]);

            //Don't add offset for zero
            if (width < 1)
            {
                offset.X = 0;
                offset.Width = 0;
            }

            if (height < 1)
            {
                offset.Y = 0;
                offset.Height = 0;
            }

            return new Rect(x + offset.X, y + offset.Y, width + offset.Width, height + offset.Height);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            var rect = (Rect)value;

            return new object[]
            {
                rect.X,
                rect.Y,
                rect.Width,
                rect.Height,
            };
        }
    }
}
