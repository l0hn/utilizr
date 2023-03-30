using System;

namespace Utilizr.Extensions
{
    public static class DateTimeEx
    {
        private static readonly DateTime _epoch = new(1970, 1, 1, 0, 0, 0);

        /// <summary>
        /// Convert a datetime object to a unix timestamp
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int ToUnixTimestamp(this DateTime dateTime, DateTimeKind dateTimeKind = DateTimeKind.Utc)
        {
            TimeSpan ts = (new DateTime(dateTime.Ticks, dateTimeKind) - _epoch);
            int unixTime = (int)Math.Round(ts.TotalSeconds, 0);
            return unixTime;
        }

        /// <summary>
        /// Convert a datetime object to a unix timestamp
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToUnixTimestampString(this DateTime datetime, DateTimeKind dateTimeKind = DateTimeKind.Utc)
        {
            return datetime.ToUnixTimestamp(dateTimeKind).ToString();
        }

        /// <summary>
        /// Convert a unix timestamp to a datetime object
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns>DateTime object (UTC)</returns>
        public static DateTime ToDateTime(this int timestamp, DateTimeKind dateTimeKind = DateTimeKind.Utc)
        {
            DateTime t = new DateTime(1970, 1, 1, 0, 0, 0, dateTimeKind).AddSeconds(timestamp);
            return t;
        }

        /// <summary>
        /// Convert a unix timestamp to a datetime object
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns>DateTime object (UTC)</returns>
        public static DateTime ToDateTime(this long timestamp, DateTimeKind dateTimeKind = DateTimeKind.Utc)
        {
            DateTime t = new DateTime(1970, 1, 1, 0, 0, 0, dateTimeKind).AddSeconds(timestamp);
            return t;
        }
    }
}
