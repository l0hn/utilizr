using NUnit.Framework;
using System;
using System.Globalization;
using System.Threading;
using Utilizr.Extensions;

namespace Utilizr.Tests.Extension
{
    [TestFixture]
    public class DateTimeExtensionTest
    {
        [Test]
        public void DateTimeExtensionsTest()
        {
            var ts = 1373994325;
            var dt = ts.ToDateTime();
            var checkTs = dt.ToUnixTimestamp();
            Assert.That(ts, Is.EqualTo(checkTs));

            var newDate = new DateTime(2013, 7, 16, 17, 5, 25, DateTimeKind.Utc);
            var timestamp = newDate.ToUnixTimestamp();
            Assert.That(ts, Is.EqualTo(timestamp));
        }

        [Test]
        [SetCulture("en-NZ")]
        [SetUICulture("en-NZ")]
        public void DateTimeLocalTest()
        {
            // todo: ensure on a culture where local != utc, this doesn't seem to be working. But BST for now so okay...
            var culture = CultureInfo.GetCultureInfo("en-NZ");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            var someDate = new DateTime(2013, 8, 12, 17, 45, 20, DateTimeKind.Local);
            Assert.That(someDate, Is.Not.EqualTo(someDate.ToUniversalTime()));
            var timestamp = someDate.ToUnixTimestamp();
            var backAgain = timestamp.ToDateTime(DateTimeKind.Local);
            Assert.That(backAgain, Is.EqualTo(someDate));
        }

        [Test]
        public void TotalMonthsTest()
        {
            // Simple comparison
            Assert.That(new DateTime(2014, 1, 1).TotalMonths(new DateTime(2014, 2, 1)), Is.EqualTo(1));
            // Just under 1 month's diff
            Assert.That(new DateTime(2014, 1, 1).TotalMonths(new DateTime(2014, 1, 31)), Is.EqualTo(0));
            // Just over 1 month's diff
            Assert.That(new DateTime(2014, 1, 1).TotalMonths(new DateTime(2014, 2, 2)), Is.EqualTo(1));
            // 31 Jan to 28 Feb
            Assert.That(new DateTime(2014, 1, 31).TotalMonths(new DateTime(2014, 2, 28)), Is.EqualTo(1));
            // Leap year 29 Feb to 29 Mar
            Assert.That(new DateTime(2012, 2, 29).TotalMonths(new DateTime(2012, 3, 29)), Is.EqualTo(1));
            // Whole year minus a day
            Assert.That(new DateTime(2012, 1, 1).TotalMonths(new DateTime(2012, 12, 31)), Is.EqualTo(11));
            // Whole year
            Assert.That(new DateTime(2012, 1, 1).TotalMonths(new DateTime(2013, 1, 1)), Is.EqualTo(12));
            // 29 Feb (leap) to 28 Feb (non-leap)
            Assert.That(new DateTime(2012, 2, 29).TotalMonths(new DateTime(2013, 2, 28)), Is.EqualTo(12));
            // 100 years
            Assert.That(new DateTime(2000, 1, 1).TotalMonths(new DateTime(2100, 1, 1)), Is.EqualTo(1200));
            // Same date
            Assert.That(new DateTime(2014, 8, 5).TotalMonths(new DateTime(2014, 8, 5)), Is.EqualTo(0));
            // Past date
            Assert.That(new DateTime(2012, 1, 1).TotalMonths(new DateTime(2011, 6, 10)), Is.EqualTo(6));
        }

        [Test]
        public void TotalYearsTest()
        {
            // Same day
            Assert.That(new DateTime(2023, 5, 31).TotalYears(new DateTime(2023, 5, 31)), Is.EqualTo(0));
            // Exactly 1 year
            Assert.That(new DateTime(2022, 5, 31).TotalYears(new DateTime(2023, 5, 31)), Is.EqualTo(1));
            // Just under 1 years's diff by time
            Assert.That(new DateTime(2022, 5, 31, 10, 0, 0).TotalYears(new DateTime(2023, 5, 30, 9, 59, 59)), Is.EqualTo(0));
            // Just under 1 years's diff by day
            Assert.That(new DateTime(2022, 5, 31).TotalYears(new DateTime(2023, 5, 30)), Is.EqualTo(0));
            // Just over 1 year's diff by day
            Assert.That(new DateTime(2022, 5, 30).TotalYears(new DateTime(2023, 5, 31)), Is.EqualTo(1));
            // Just over 1 year's diff by time
            Assert.That(new DateTime(2022, 5, 31, 10, 0, 0).TotalYears(new DateTime(2023, 5, 31, 10, 0, 1)), Is.EqualTo(1));
            // 100 years
            Assert.That(new DateTime(2000, 1, 1).TotalYears(new DateTime(2100, 1, 1)), Is.EqualTo(100));
        }
    }
}
