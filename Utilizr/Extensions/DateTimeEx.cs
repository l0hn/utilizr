using System;
using System.Runtime.InteropServices.Marshalling;

namespace Utilizr.Extensions
{
    public static class DateTimeEx
    {
        /// <summary>
        /// Convert a datetime object to a unix timestamp
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
        }

        /// <summary>
        /// Convert a datetime object to a unix timestamp
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToUnixTimestampString(this DateTime dateTime)
        {
            return dateTime.ToUnixTimestamp().ToString();
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
        {
            var dtOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);

            return dateTimeKind == DateTimeKind.Utc
                ? dtOffset.UtcDateTime
                : dtOffset.LocalDateTime;
        }

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
            var timeSpan = dateTime < toCompare
                ? toCompare - dateTime
                : dateTime - toCompare;

            // Because we start at year 1 for the Gregorian calendar, we must subtract a year here.
            // todo: DateTimeEx.TotalYears() may be a bit off for cultures not using Gregorian calendar
            return (zeroTime + timeSpan).Year - 1;
        }

        /// <summary>
        /// Return a DateTime instance matching the next (or previous) specified day.
        /// </summary>
        /// <param name="dateTime">Current date and time.</param>
        /// <param name="dayToFind">The desired day to find based based on the <see cref="dateTime"/></param> value.
        /// <param name="futureDate">Whether we're finding the day in the future, or in the past.</param>
        /// <returns></returns>
        public static DateTime GetNextDay(this DateTime dateTime, DayOfWeek dayToFind, bool futureDate)
        {
            return GetNextDay(dateTime, dayToFind, futureDate, out _);
        }

        /// <summary>
        /// Return a DateTime instance matching the next (or previous) specified day.
        /// </summary>
        /// <param name="dateTime">Current date and time.</param>
        /// <param name="dayToFind">The desired day to find based based on the <see cref="dateTime"/></param> value.
        /// <param name="futureDate">Whether we're finding the day in the future, or in the past.</param>
        /// <param name="alreadyMatched">Whether any offset was applied due to the day already matching.</param>
        /// <returns></returns>
        public static DateTime GetNextDay(this DateTime dateTime, DayOfWeek dayToFind, bool futureDate, out bool alreadyMatched)
        {
            var dayDelta = CalculateDayOffset(dateTime.DayOfWeek, dayToFind);
            alreadyMatched = dayDelta == 0;
            return dateTime.AddDays(futureDate ? dayDelta : (0 - dayDelta));
        }

        /// <summary>
        /// Calculate the number of days between the given two values.
        /// </summary>
        /// <param name="currentDay"></param>
        /// <param name="dayOfDesiredWeek"></param>
        /// <returns></returns>
        public static int CalculateDayOffset(DayOfWeek currentDay, DayOfWeek dayOfDesiredWeek)
        {
            int c = (int)currentDay;
            int d = (int)dayOfDesiredWeek;
            return (7 - c + d) % 7;
        }

        /// <summary>
        /// Create a DateTime with the same Date, but set the time with the given values.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="hours"></param>
        /// <param name="minutes">Optional minutes, defaults to 0.</param>
        /// <param name="seconds">Optional seconds, defaults to 0.</param>
        /// <returns></returns>
        public static DateTime SetTime(this DateTime dt, int hours, int? minutes = null, int? seconds = null)
        {
            return new DateTime(
                dt.Year,
                dt.Month,
                dt.Day,
                hours,
                minutes ?? 0,
                seconds ?? 0
            );
        }
    }
}
