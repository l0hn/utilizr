using System;

namespace Utilizr.Extensions
{
    public static class DateTimeEx
    {
        /// <summary>
        /// Convert a datetime object to a unix timestamp
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToUnixTimestamp(this DateTime dateTime, DateTimeKind dateTimeKind = DateTimeKind.Utc)
            => new DateTimeOffset(
                new DateTime(dateTime.Ticks, dateTimeKind), TimeSpan.Zero).ToUnixTimeSeconds();

        /// <summary>
        /// Convert a datetime object to a unix timestamp
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToUnixTimestampString(this DateTime dateTime, DateTimeKind dateTimeKind = DateTimeKind.Utc)
        {
            return dateTime.ToUnixTimestamp(dateTimeKind).ToString();
        }

        /// <summary>
        /// Convert a unix timestamp to a datetime object
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns>DateTime object (UTC)</returns>
        public static DateTime ToDateTime(this int timestamp, DateTimeKind dateTimeKind = DateTimeKind.Utc)
            => ((long)timestamp).ToDateTime(dateTimeKind);

        /// <summary>
        /// Convert a unix timestamp to a datetime object
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns>DateTime object (UTC)</returns>
        public static DateTime ToDateTime(this long timestamp, DateTimeKind dateTimeKind = DateTimeKind.Utc)
            => new DateTime(DateTimeOffset.FromUnixTimeSeconds(timestamp).Ticks, dateTimeKind);
    }
}
