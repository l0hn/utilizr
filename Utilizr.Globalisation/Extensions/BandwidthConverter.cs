using System;
using Utilizr.Globalisation;

namespace Utilizr.Globalisation.Extensions
{
    public static class BandwidthConverter
    {
        private static readonly ITranslatable[] _suffix = new ITranslatable[]
        {
            L._I("bps"),
            L._I("Kbps"),
            L._I("Mbps"),
            L._I("Gbps"),
            L._I("Tbps"),
            L._I("Pbps"),
            L._I("Ebps"),
        };

        private static readonly double[] _thresholds = new double[]
        {
            // Fine to be 1, rather than 0 on first entry. Avoids division by 0, and checks 
            // if less than 1000 to handle decimal points since cannot have a bit of a bit

            Math.Pow(1000, 0), // b
            Math.Pow(1000, 1), // kb
            Math.Pow(1000, 2), // mb
            Math.Pow(1000, 3), // gb
            Math.Pow(1000, 4), // tb
            Math.Pow(1000, 5), // pb
            Math.Pow(1000, 6), // eb
        };

        public static string ToBandwidthString(this int bitsPerSecond, int? decimalPlaces = 2)
        {
            return ToBandwidthString((long)bitsPerSecond, decimalPlaces);
        }

        public static string ToBandwidthString(this long bitsPerSecond, int? decimalPlaces = 2)
        {
            return ToBandwidthString(bitsPerSecond, out string _, out string __, decimalPlaces);
        }

        public static string ToBandwidthString(this int bitsPerSecond, out string formattedBits, out string suffix, int? decimalPlaces = 2)
        {
            return ToBandwidthString((long)bitsPerSecond, out formattedBits, out suffix, decimalPlaces);
        }

        public static string ToBandwidthString(this long bitsPerSecond, out string formattedBits, out string unit, int? decimalPlaces = 2)
        {
            if (bitsPerSecond < 1000)
            {
                unit = _suffix[0].Translation;
                formattedBits = string.Format("{0:N0}", bitsPerSecond);
                return string.Format("{0} {1}", formattedBits, unit);
            }

            int i = -1;
            while (_thresholds[i + 1] <= bitsPerSecond)
            {
                i++;

                if (i >= _thresholds.Length - 1)
                    break;
            }

            unit = _suffix[i].Translation;
            formattedBits = string.Format(
                decimalPlaces.HasValue
                    ? $"{{0:N{decimalPlaces.Value}}}"
                    : "{0}",
                bitsPerSecond / _thresholds[i]);

            return string.Format("{0} {1}", formattedBits, unit);
        }
    }
}