using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilizr.Network;

namespace Utilizr.Tests.Network
{
    [TestFixture]
    public class SubDomainStripperTests
    {
        [TestCase("amazon.co.uk", "amazon.co.uk")]
        [TestCase("login.ebay.com", "ebay.com")]
        [TestCase("secure.scan.co.uk", "scan.co.uk")]
        [TestCase("api.dev.blog.example.com", "example.com")]
        [TestCase("api.dev.blog.example.gov.uk", "example.gov.uk")]
        public void EnsureHostHasExpectedValue(string urlWithSubdomain, string expectedUrl)
        {
            var uri = new Uri($"https://{urlWithSubdomain}");
            var stripped = new SubDomainStripper(uri);
            Assert.That(stripped.HostWithoutSubdomain, Is.EqualTo(expectedUrl));
        }
    }
}
