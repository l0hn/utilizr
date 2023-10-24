using System;

namespace Utilizr.Globalisation.Extensions
{
    public static class ByteConverter
    {
        public const long KB_THRESHOLD = 0x100000;
        public const long MB_THRESHOLD = 0x40000000;
        public const long GB_THRESHOLD = 0x10000000000;
        public const long TB_THRESHOLD = 0x4000000000000;
        public const long PB_THRESHOLD = 0x1000000000000000;

        private static readonly string[] _suffix = new string[]
        {
            L._M("B"),
            L._M("KB"),
            L._M("MB"),
            L._M("GB"),
            L._M("TB"),
            L._M("PB"),
            L._M("EB"),
        };

        private static readonly long[] _base10Thresholds = new long[]
        {
            0,
            1000,
            1000000,
            1000000000,
            1000000000000,
            1000000000000000,
            (long)Math.Pow(10, 18)
        };

        public static string ToBytesString(this int bytes, int decimalPlaces = 0, bool returnEnglish = false)
        {
            return ToBytesString((long) bytes, out _, out _, decimalPlaces, returnEnglish);
        }

        public static string ToBytesString(this int bytes, out double byteCount, out string byteUnit, int decimalPlaces = 0, bool returnEnglish = false)
        {
            return ToBytesString((long)bytes, out byteCount, out byteUnit, decimalPlaces, returnEnglish);
        }

        public static string ToBytesString(this long bytes, int decimalPlaces = 0, bool returnEnglish = false)
        {
            return ToBytesString(bytes, false, out _, out _, decimalPlaces, returnEnglish);
        }

        public static string ToBytesString(this long bytes, out double byteCount, out string byteUnit, int decimalPlaces = 0, bool returnEnglish = false)
        {
            if (Environment.OSVersion.Platform == PlatformID.MacOSX)
                return ToBytesString(bytes, true, out byteCount, out byteUnit, decimalPlaces, returnEnglish);

            // unix / windows
            return ToBytesString(bytes, false, out byteCount, out byteUnit, decimalPlaces, returnEnglish);
        }

        public static string ToBytesString(this int bytes, bool useBase10, out double byteCount, out string byteUnit, int decimalPlaces = 0, bool returnEnglish = false)
        {
            return ToBytesString((long) bytes, useBase10, out byteCount, out byteUnit, decimalPlaces, returnEnglish);
        }

        public static string ToBytesString(this long bytes, bool useBase10, out double byteCount, out string byteUnit, int decimalPlaces = 0, bool returnEnglish = false)
        {
            return useBase10
                ? Base10Impl(bytes, decimalPlaces, returnEnglish, out byteCount, out byteUnit)
                : Base2Impl(bytes, decimalPlaces, returnEnglish, out byteCount, out byteUnit);
        }

        private static string Base2Impl(long bytes, int decimalPlaces, bool returnEnglish, out double byteCount, out string byteUnit)
        {
            string sign = (bytes < 0 ? "-" : "");
            if (bytes == long.MinValue)
                bytes++;
            bytes = bytes < 0 ? -bytes : bytes;

            if (bytes < 0x400) // Byte
            {
                byteCount = bytes;
                byteUnit = returnEnglish ? L._M("B") : L._("B");
                return string.Format("{0}{1:N0} {2}", sign, bytes, byteUnit);
            }
            else if (bytes < KB_THRESHOLD) // Kilobyte
            {
                byteUnit = returnEnglish ? L._M("KB") : L._("KB");
                byteCount = (double)bytes;
            }
            else if (bytes < MB_THRESHOLD) // Megabyte
            {
                byteUnit = returnEnglish ? L._M("MB") : L._("MB");
                byteCount = (double)(bytes >> 10);
            }
            else if (bytes < GB_THRESHOLD) // Gigabyte
            {
                byteUnit = returnEnglish ? L._M("GB") : L._("GB");
                byteCount = (double)(bytes >> 20);
            }
            else if (bytes < TB_THRESHOLD) // Terabye
            {
                byteUnit = returnEnglish ? L._M("TB") : L._("TB");
                byteCount = (double)(bytes >> 30);
            }
            else if (bytes < PB_THRESHOLD) // Petabyte
            {
                byteUnit = returnEnglish ? L._M("PB") : L._("PB");
                byteCount = (double)(bytes >> 40);
            }
            else // Exabyte
            {
                byteUnit = returnEnglish ? L._M("EB") : L._("EB");
                byteCount = (double)(bytes >> 50);
            }

            byteCount /= 1024;

            string formatStr = decimalPlaces < 1 ? "0.##" : string.Format("n{0}", decimalPlaces);
            return string.Format("{0}{1} {2}", sign, byteCount.ToString(formatStr), byteUnit);
        }

        private static string Base10Impl(long bytes, int decimalPlaces, bool returnEnglish, out double byteCount, out string byteUnit)
        {
            if (bytes < 1000)
            {
                byteCount = bytes;
                byteUnit = returnEnglish ? L._M("B") : L._("B");
                return string.Format("{0:N0} {1}", byteCount, byteUnit);
            }

            int i = -1;
            while (_base10Thresholds[i+1] < bytes)
            {
                if (i > _base10Thresholds.Length - 2)
                    break;
                i++;
            }

            byteCount = (double)bytes / (double)_base10Thresholds[i];
            byteUnit = returnEnglish ? _suffix[i] : L._(_suffix[i]);

            return string.Format("{0:N" + decimalPlaces + "} {1}", byteCount, byteUnit);
        }
    }
}
