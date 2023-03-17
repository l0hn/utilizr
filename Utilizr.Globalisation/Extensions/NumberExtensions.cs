using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Utilizr.Logging;

namespace Utilizr.Globalisation.Extensions
{
    public static class NumberExtensions
    {
        private static readonly string[] _suffixes = new string[] { "k", "m", "b", "t", "q" };
        private static readonly double[] _pows = new double[]
        {
            Math.Pow(1000, 1),
            Math.Pow(1000, 2),
            Math.Pow(1000, 3),
            Math.Pow(1000, 4),
            Math.Pow(1000, 5),
        };

        public static string ToFancyString(this long num)
        {
            return FormatNumber(num);
        }

        public static string ToFancyString(this int num)
        {
            return FormatNumber(num);
        }

        public static string ToFancyString(this double num)
        {
            return FormatNumber(num);
        }

        private static string FormatNumber(double value)
        {
            for (int j = _suffixes.Length; j > 0; j--)
            {
                if (value >= _pows[--j])
                    return string.Format("{0:#,##0}{1}+", (int)(value / _pows[j]), _suffixes[j]);
            }
            return value.ToString("#,##0");
        }

        /// <summary>
        /// Gets the currency string.
        /// </summary>
        /// <param name="value">The amount</param>
        /// <param name="iso4217currencyCode">Currency code.</param>
        /// <param name="decimalPlaces">Explicitly specify decimal places. Default to 2, if necessary, otherwise 0. E.g. 10 returns £10, but 10.1 would return £10.10 for GBP</param>
        /// <returns></returns>
        public static string ToCurrencyString(this int value, string iso4217currencyCode, int? decimalPlaces = null)
        {
            return FormatCurrency(value, iso4217currencyCode, decimalPlaces);
        }

        /// <summary>
        /// Gets the currency string.
        /// </summary>
        /// <param name="value">The amount</param>
        /// <param name="iso4217currencyCode">Currency code.</param>
        /// <param name="decimalPlaces">Explicitly specify decimal places. Default to 2, if necessary, otherwise 0. E.g. 10 returns £10, but 10.1 would return £10.10 for GBP</param>
        /// <returns></returns>
        public static string ToCurrencyString(this long value, string iso4217currencyCode, int? decimalPlaces = null)
        {
            return FormatCurrency(value, iso4217currencyCode, decimalPlaces);
        }

        /// <summary>
        /// Gets the currency string.
        /// </summary>
        /// <param name="value">The amount</param>
        /// <param name="iso4217currencyCode">Currency code.</param>
        /// <param name="decimalPlaces">Explicitly specify decimal places. Default to 2, if necessary, otherwise 0. E.g. 10 returns £10, but 10.1 would return £10.10 for GBP</param>
        /// <returns></returns>
        public static string ToCurrencyString(this double value, string iso4217currencyCode, int? decimalPlaces = null)
        {
            return FormatCurrency(value, iso4217currencyCode, decimalPlaces);
        }

        private static Dictionary<string, CultureInfo>? _isoCurrenciesToACultureMap;
        /// <summary>
        /// Gets the currency string.
        /// </summary>
        /// <param name="value">The amount</param>
        /// <param name="iso4217currencyCode">Currency code.</param>
        /// <param name="decimalPlaces">Explicitly specify decimal places. Default to 2, if necessary, otherwise 0. E.g. 10 returns £10, but 10.1 would return £10.10 for GBP</param>
        /// <returns></returns>
        static string FormatCurrency(double value, string iso4217currencyCode, int? decimalPlaces)
        {
            // If null, calculate ourselves. Defaulting to no decimals. But only when decimals not explicitly specified.
            // E.g. Show £10, not £10.00, if possible
            decimalPlaces ??= value % 1 == 0 ? 0 : 2;

            try
            {
                _isoCurrenciesToACultureMap ??= CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                        .Select(c => new { c, new RegionInfo(c.LCID).ISOCurrencySymbol })
                        .GroupBy(x => x.ISOCurrencySymbol)
                        .ToDictionary(g => g.Key, g => g.First().c, StringComparer.OrdinalIgnoreCase);

                string formatStrDecimal = $"{{0:C{decimalPlaces.Value}}}";
                if (_isoCurrenciesToACultureMap.TryGetValue(iso4217currencyCode, out CultureInfo? culture))
                    return string.Format(culture, formatStrDecimal, value);
            }
            catch (Exception ex)
            {
                Log.Exception(nameof(ToCurrencyString), ex);
            }

            string lastResortFomatStr = $"0.{new string('0', decimalPlaces.Value)}";
            return value.ToString(lastResortFomatStr);
        }


        /// <summary>
        /// Builds up a string to '_ Days _ Hours _ Minutes _ Seconds', omitting any unnecessary parts.
        /// </summary>
        public static string ToDurationFromMilliseconds(this long milliSeconds, MaxDurationUnit largestUnit = MaxDurationUnit.Day)
        {
            if (milliSeconds < 1)
                return L._("Never");

            var ts = TimeSpan.FromMilliseconds(milliSeconds);
            var sb = new StringBuilder();

            if (ts.TotalDays >= 1 && largestUnit >= MaxDurationUnit.Day)
            {
                var days = (long)ts.TotalDays;
                // ## Building sentence which can look something like '_ Days _ Hours _ Minutes _ Seconds' from milliseconds. Some parts may be omitted if not needed.
                sb.Append(L._P("{0:N0} Day", "{0:N0} Days", days, days));
            }

            if (ts.TotalHours >= 1 && largestUnit >= MaxDurationUnit.Hour)
            {
                var hours = largestUnit == MaxDurationUnit.Hour
                    ? (long)ts.TotalHours
                    : ts.Hours;

                if (hours > 0)
                {
                    // ## Building sentence which can look something like '_ Days _ Hours _ Minutes _ Seconds' from milliseconds. Some parts may be omitted if not needed.
                    sb.Append(L._P(" {0:N0} Hour", " {0:N0} Hours", hours, hours));
                }
            }

            if (ts.TotalMinutes >= 1 && largestUnit >= MaxDurationUnit.Minute)
            {
                var mins = largestUnit == MaxDurationUnit.Minute
                    ? (long)ts.TotalMinutes
                    : ts.Minutes;

                if (mins > 0)
                {
                    // ## Building sentence which can look something like '_ Days _ Hours _ Minutes _ Seconds' from milliseconds. Some parts may be omitted if not needed.
                    sb.Append(L._P(" {0:N0} Minute", " {0:N0} Minutes", mins, mins));
                }
            }

            if (ts.TotalSeconds >= 1)
            {
                var seconds = largestUnit == MaxDurationUnit.Second
                    ? (long)ts.TotalSeconds
                    : ts.Seconds;

                if (seconds > 0)
                {
                    // ## Building sentence which can look something like '_ Days _ Hours _ Minutes _ Seconds' from milliseconds. Some parts may be omitted if not needed.
                    sb.Append(L._P(" {0:N0} Second", " {0:N0} Seconds", seconds, seconds));
                }
            }

            return sb.ToString().Trim();
        }
    }

    public enum MaxDurationUnit
    {
        // Note: order important for comparisons, smallest first

        /// <summary>
        /// Seconds is the largest unit: e.g. 90 seconds
        /// </summary>
        Second,

        /// <summary>
        /// Minute is the largest unit: e.g. 120 Minutes
        /// </summary>
        Minute,

        /// <summary>
        /// Hour is the largest unit: e.g. 48 Hours
        /// </summary>
        Hour,

        /// <summary>
        /// Day is the largest unit: e.g. 2 Days
        /// </summary>
        Day,
    }
}