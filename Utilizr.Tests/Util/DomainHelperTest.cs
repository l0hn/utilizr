using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilizr.Util;

namespace Utilizr.Tests.Util
{
    [TestFixture]
    public class DomainHelperTest
    {
        [Test]
        [TestCase("http://test.com", "test.com", "/", false)]
        [TestCase("http://test.test1.com", "test.test1.com", "/", false)]
        [TestCase("http://test.com?param=something", "test.com", "/", false)]
        [TestCase("http://test.test1.test2.com", "test.test1.test2.com", "/", false)]
        [TestCase("http://test.test1.test2.com/path", "test.test1.test2.com", "/path", true)]
        public void DomainTest(string inUrl, string? outUrl, string? absolutPath, bool isPAth)
        {
            string fullUrl = inUrl;
            string host = "";
            string absolutpath = "";
            var ret = DomainHelper.GetDomain(fullUrl, out host, out absolutpath);
            Assert.That(host, Is.EqualTo(outUrl));
            Assert.That(absolutpath, Is.EqualTo(absolutPath));
            Assert.That(ret, Is.EqualTo(isPAth));
        }

        [Test]
        [TestCase("http://test.com", "", "")]
        [TestCase("http://test.com?param=something", "", "")]
        [TestCase("http://subdomain.test.com?param=something", "test.com", "*.test.com")]
        [TestCase("subdomain1.subdomain2.test.com?param=something", "test.com", "*.test.com")]
        [TestCase("http://subdomain.test.com", "test.com", "*.test.com")]
        public void SubDomainTest(string inUrl, string subDomain1, string subDomain2)
        {
            string fullUrl = inUrl;
            var subdomain = DomainHelper.GetSubDomain(fullUrl);
            
            if (subDomain1 == "" && subDomain2 == "")
            {
                var list = new List<string>();
                Assert.That(list, Is.EqualTo(subdomain).AsCollection);
            }
            else
            {
                var list = new List<string>();
                list.Add(subDomain1);
                list.Add(subDomain2);
                Assert.That(list, Is.EqualTo(subdomain).AsCollection);
            }


        }
    }
}
