using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Utilizr.Util;

namespace Utilizr.Tests.Domain
{
    [TestFixture]
    public class DomainHelperTest
    {
        [Test]
        public void SubDomainTest()
        {
            string fullUrl = "aaa.bbb.ccc.com";
            var subdomain = DomainHelper.GetSubDomain(fullUrl);
            Assert.That(subdomain, Is.EqualTo("ccc.com"));

            fullUrl = "ccc.com";
            subdomain = DomainHelper.GetSubDomain(fullUrl);
            Assert.That(subdomain, Is.EqualTo(null));
        }

        [Test]
        public void DomainTest()
        {
            string fullUrl = "aaa.bbb.ccc.com";
            var domain = DomainHelper.GetDomain(fullUrl);
            Assert.That(domain, Is.EqualTo("aaa.bbb.ccc.com"));

            fullUrl = "www.bbb.ccc.com/xxx/www";
            domain = DomainHelper.GetDomain(fullUrl);
            Assert.That(domain, Is.EqualTo("www.bbb.ccc.com"));
        }
    }
}
