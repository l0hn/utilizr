using System;
using Utilizr.Extensions;
using Utilizr.Win32.Kernel32;
using Utilizr.Win32.Kernel32.Structs;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace Utilizr.Win.Extensions
{
    public static class DateTimeEx
    {
        /// <summary>
        /// Converts a file time to DateTime format.
        /// </summary>
        /// <param name="filetime">FILETIME structure</param>
        /// <returns>DateTime structure</returns>
        public static DateTime ToDateTime(this FILETIME filetime)
        {
            SYSTEMTIME st = new SYSTEMTIME();
            Kernel32.FileTimeToSystemTime(ref filetime, ref st);
            return new DateTime(st.Year, st.Month, st.Day, st.Hour, st.Minute, st.Second, st.Milliseconds);

        }

        /// <summary>
        /// Convert FILETIME to DateTime
        /// </summary>
        /// <param name="time"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this FILETIME time, DateTimeKind kind = DateTimeKind.Utc)
        {
            long highBits = time.dwHighDateTime;
            highBits <<= 32;

            if (kind == DateTimeKind.Local)
                return DateTime.FromFileTime(highBits + (uint)time.dwLowDateTime);

            return DateTime.FromFileTimeUtc(highBits + (uint)time.dwLowDateTime);
        }

        /// <summary>
        /// Converts a file time to unix time.
        /// </summary>
        /// <param name="time">FILETIME</param>
        /// <returns>unix timestamp</returns>
        public static long ToUnixTimestamp(this FILETIME time)
        {
            var dt = ToDateTime(time);
            return dt.ToUnixTimestamp();
        }
    }
}
