﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Utilizr.WPF.Converters
{
    public class StringEmptyOrNullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}