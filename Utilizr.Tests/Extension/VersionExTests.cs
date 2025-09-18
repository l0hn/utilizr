using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilizr.Extensions;

namespace Utilizr.Tests.Extension
{
    [TestFixture]
    public class VersionExTests
    {
        [Test]
        public void VersionFieldCount4Test()
        {
            var version = new Version(1, 2, 3, 4);
            var vString = version.SafeToString(4);
            Assert.That(vString, Is.EqualTo("1.2.3.4"));
        }

        [Test]
        public void VersionFieldCount3Test()
        {
            var version = new Version(1, 2, 3);
            var vString = version.SafeToString(4);
            Assert.That(vString, Is.EqualTo("1.2.3"));
        }

        [Test]
        public void VersionFieldCount2Test()
        {
            var version = new Version(1, 2);
            var vString = version.SafeToString(4);
            Assert.That(vString, Is.EqualTo("1.2"));
        }

        [Test]
        public void VersionFieldCount0Test()
        {
            var version = new Version();
            var vString = version.SafeToString(4);
            Assert.That(vString, Is.EqualTo("0.0"));
        }
    }
}
