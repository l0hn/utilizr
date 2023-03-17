using System;

namespace Utilizr.Globalisation.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly DateTime ePoch = new(1970, 1, 1, 0, 0, 0);

        /// <summary>
        /// Convert a datetime object to a unix timestamp
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int ToUnixTimestamp(this DateTime dateTime, DateTimeKind dateTimeKind = DateTimeKind.Utc)
        {
            TimeSpan ts = new DateTime(dateTime.Ticks, dateTimeKind) - ePoch;
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

        /// <summary>
        /// Generates fuzzy "time ago" string, based on the current time
        /// </summary>
        /// <returns>Formatted string</returns>
        /// <param name="dateTime">Date time.</param>
        public static string TimeAgo(this DateTime dateTime)
        {
            var timeSpan = DateTime.Now.Subtract(dateTime);

            if (timeSpan <= TimeSpan.FromSeconds(60))
            {
                return L._P("{0} second ago", "{0} seconds ago", timeSpan.Seconds, timeSpan.Seconds);
            }
            else if (timeSpan <= TimeSpan.FromMinutes(60))
            {
                return L._P("{0} minute ago", "{0} minutes ago", timeSpan.Minutes, timeSpan.Minutes);
            }
            else if (timeSpan <= TimeSpan.FromHours(24))
            {
                return L._P("{0} hour ago", "{0} hours ago", timeSpan.Hours, timeSpan.Hours);
            }
            else if (timeSpan <= TimeSpan.FromDays(30))
            {
                return timeSpan.Days > 1
                    ? L._("{0} days ago", timeSpan.Days)
                    : L._("yesterday");
            }
            else if (timeSpan <= TimeSpan.FromDays(365))
            {
                int months = timeSpan.Days / 30;
                return L._P("{0} month ago", "{0} months ago", months, months);
            }
            else
            {
                int years = timeSpan.Days / 365;
                return L._P("{0} year ago", "{0} years ago", years, years);
            }
        }
    }
}
