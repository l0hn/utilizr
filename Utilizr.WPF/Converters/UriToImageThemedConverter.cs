using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Utilizr.Logging;
using Utilizr.WPF.Util;

namespace Utilizr.WPF.Converters
{
    public enum ImageThemedConverterMode
    {
        /// <summary>
        /// Only check the themed folder.
        /// </summary>
        ThemedResourceOnly,

        /// <summary>
        /// The main use case here is to make it easier to implement UserControls without the need to
        /// swap out to different IValueConverter implementations for themed and unthemed icons.
        /// </summary>
        FallbackToUnthemedResource,
    }

    public class UriToImageThemedConverter: MarkupExtension, IValueConverter
    {
        private static UriToImageThemedConverter? _instance;

        /// <summary>
        /// The subfolder which will be checked when trying to load a theme specific image.
        /// </summary>
        public static string? CurrentTheme { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            _instance ??= new UriToImageThemedConverter();
            return _instance;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mode = ImageThemedConverterMode.FallbackToUnthemedResource;
            if (parameter is ImageThemedConverterMode explicitMode)
                mode = explicitMode;

            var result = ResourceHelper.GetImageSource(
                (string)value!,
                CurrentTheme,
                mode == ImageThemedConverterMode.FallbackToUnthemedResource
            );

            Log.Debug(nameof(UriToImageThemedConverter), $"CurrentTheme={CurrentTheme}, value={value}, mode={mode}, result={result}");
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
