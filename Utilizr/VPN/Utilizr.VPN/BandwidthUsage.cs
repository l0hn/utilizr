using System;
using System.Diagnostics;
//using GetText;
//using Utilizr.Conversion;
using Utilizr.Globalisation;

namespace Utilizr.VPN
{
    public class BandwidthUsage: EventArgs
    {
        public const long KB_THRESHOLD = 0x100000;
        public const long MB_THRESHOLD = 0x40000000;
        public const long GB_THRESHOLD = 0x10000000000;
        public const long TB_THRESHOLD = 0x4000000000000;
        public const long PB_THRESHOLD = 0x1000000000000000;

        private DateTime _lastUpdate = DateTime.UtcNow;
        public long BytesSent { get; private set; }
        public long BytesRecieved { get; private set; }

        public long TxBytesPerSecond { get; set; }
        public long RxBytesPerSecond { get; set; }

        private static string[] _suffix = new string[]
        {
            L._M("B"),
            L._M("KB"),
            L._M("MB"),
            L._M("GB"),
            L._M("TB"),
            L._M("PB"),
            L._M("EB"),
        };

        private static long[] _base10Thresholds = new long[]
        {
            0,
            1000,
            1000000,
            1000000000,
            1000000000000,
            1000000000000000,
            (long)Math.Pow(10, 18)
        };

        public void Update(long totalBytesSend, long totalBytesRecieved)
        {
            var newTime = DateTime.UtcNow;
            var secsPassed = (newTime - _lastUpdate).TotalSeconds;
            var outDiff = totalBytesSend - BytesSent;
            var inDiff = totalBytesRecieved - BytesRecieved;
            if (secsPassed > 0)
            {
                TxBytesPerSecond = (long)(outDiff / secsPassed);
                RxBytesPerSecond = (long)(inDiff / secsPassed);
            }
            _lastUpdate = newTime;
            BytesRecieved = totalBytesRecieved;
            BytesSent = totalBytesSend;
        }

        public void Reset()
        {
            _lastUpdate = DateTime.UtcNow;
            BytesSent = 0;
            BytesRecieved = 0;
            TxBytesPerSecond = 0;
            RxBytesPerSecond = 0;
        }

        public BandwidthUsage()
        {

        }

        public override string ToString()
        {
            return L._("In: {0}, Out: {1}",
                  ToBytesString(BytesRecieved, 2),
                ToBytesString(BytesSent, 2));
        }


        public static string ToBytesString(long bytes, int decimalPlaces = 0, bool returnEnglish = false)
        {
            return ToBytesString(bytes, false, decimalPlaces, returnEnglish);
        }

        public static string ToBytesString(long bytes, bool useBase10, int decimalPlaces = 0, bool returnEnglish = false)
        {
            return useBase10
                ? Base10Impl(bytes, decimalPlaces, returnEnglish)
                : Base2Impl(bytes, decimalPlaces, returnEnglish);
        }

        private static string Base2Impl(long bytes, int decimalPlaces, bool returnEnglish)
        {
            string sign = (bytes < 0 ? "-" : "");
            if (bytes == long.MinValue)
                bytes++;
            bytes = bytes < 0 ? -bytes : bytes;
            double readable;
            string suffix;

            if (bytes < 0x400) // Byte
            {
                return string.Format("{0}{1} B", sign, bytes);
            }
            else if (bytes < KB_THRESHOLD) // Kilobyte
            {
                suffix = returnEnglish ? L._M("KB") : L._("KB");
                readable = (double)bytes;
            }
            else if (bytes < MB_THRESHOLD) // Megabyte
            {
                suffix = returnEnglish ? L._M("MB") : L._("MB");
                readable = (double)(bytes >> 10);
            }
            else if (bytes < GB_THRESHOLD) // Gigabyte
            {
                suffix = returnEnglish ? L._M("GB") : L._("GB");
                readable = (double)(bytes >> 20);
            }
            else if (bytes < TB_THRESHOLD) // Terabye
            {
                suffix = returnEnglish ? L._M("TB") : L._("TB");
                readable = (double)(bytes >> 30);
            }
            else if (bytes < PB_THRESHOLD) // Petabyte
            {
                suffix = returnEnglish ? L._M("PB") : L._("PB");
                readable = (double)(bytes >> 40);
            }
            else // Exabyte
            {
                suffix = returnEnglish ? L._M("EB") : L._("EB");
                readable = (double)(bytes >> 50);
            }

            readable = readable / 1024;

            string formatStr = decimalPlaces < 1 ? "0.##" : string.Format("n{0}", decimalPlaces);
            return string.Format("{0}{1} {2}", sign, readable.ToString(formatStr), suffix);
        }

        private static string Base10Impl(long bytes, int decimalPlaces, bool returnEnglish)
        {
            if (bytes < 1000)
                return string.Format("{0:N0} {1}", bytes, L._("B"));

            int i = -1;
            while (_base10Thresholds[i + 1] < bytes)
            {
                if (i > _base10Thresholds.Length - 2)
                    break;
                i++;
            }

            return string.Format(
                "{0:N" + decimalPlaces + "} {1}",
                (double)bytes / (double)_base10Thresholds[i],
                returnEnglish ? _suffix[i] : L._(_suffix[i])
            );
        }
    }
}