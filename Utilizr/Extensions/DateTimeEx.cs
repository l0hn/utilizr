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


        /// <summary>
        /// Get the whole number of months which has passed between 2 dates. Will not calculate fractions.
        /// Note: not great performance for large comparisons.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="toCompare"></param>
        /// <returns></returns>
        public static int TotalMonths(this DateTime dateTime, DateTime toCompare)
        {
            // todo: worth looking into more performant TotalMonths() for DateTimes that are far apart?
            var earlyDate = (dateTime > toCompare)
                ? toCompare.Date
                : dateTime.Date;
            var lateDate = (dateTime > toCompare)
                ? dateTime.Date
                : toCompare.Date;

            // Start with 1 month's difference and keep incrementing until we overshoot the late date
            int monthsDiff = 1;
            while (earlyDate.AddMonths(monthsDiff) <= lateDate)
            {
                monthsDiff++;
            }

            return monthsDiff - 1;
        }

        /// <summary>
        /// Get the whole number of years which has passed between 2 dates. Will not calculate fractions.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="toCompare"></param>
        /// <returns></returns>
        public static int TotalYears(this DateTime dateTime, DateTime toCompare)
        {
            var zeroTime = new DateTime(1, 1, 1);
            var timeSpan = dateTime > toCompare
                ? toCompare - dateTime
                : dateTime - toCompare;

            // Because we start at year 1 for the Gregorian calendar, we must subtract a year here.
            // todo: DateTimeEx.TotalYears() may be a bit off for cultures not using Gregorian calendar
            return (zeroTime + timeSpan).Year - 1;
        }
    }
}
