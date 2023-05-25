using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Utilizr.Async;
using Utilizr.Conversion;
using Utilizr.VPN;

namespace Tests.VPN
{
    [TestFixture(Category = "VPN")]
    public class MiscTests
    {

        [Test]
        public void BandwidthUsage()
        {
            var bu = new BandwidthUsage();
            var sw = new Stopwatch();//Stopwatch to take account of time we're spending doing test things;
            for (int i = 1; i <= 10; i++)
            {
                Thread.Sleep(100-(int)sw.Elapsed.TotalMilliseconds);
                sw.Restart();
                bu.Update(1000*i, 1000*i);
                var expected = 10000;
                var tolerance = 10000*.02;//tolerance of 2% difference as we can't guarantee perfection
                Assert.Greater(bu.TxBytesPerSecond, expected - tolerance);
                Assert.Greater(bu.RxBytesPerSecond, expected - tolerance);
                Assert.Less(bu.RxBytesPerSecond, expected + tolerance);
                Assert.Less(bu.RxBytesPerSecond, expected + tolerance);
            }
        }
    }
}
