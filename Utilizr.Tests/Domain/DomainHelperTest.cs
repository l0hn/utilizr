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
        }
    }
}
