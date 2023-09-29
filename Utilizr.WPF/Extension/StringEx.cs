using System.Windows.Media;

namespace Utilizr.WPF.Extension
{
    public static class StringEx
    {
        /// <summary>
        /// E.g. #FF000000
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static Color ARGBToColor(this string hex)
        {
            return (Color)ColorConverter.ConvertFromString(hex);
        }
    }
}
