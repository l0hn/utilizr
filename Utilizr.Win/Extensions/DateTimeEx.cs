using System;
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
    }
}
