using NUnit.Framework;
using System;
using System.Threading;
using Utilizr.Extensions;

namespace Utilizr.Tests.Extension
{
    [TestFixture]
    public class SecureStringTests
    {
        [Test]
        [TestCase("secret")]
        [TestCase("multibyte: Hello 🔐 日本語 😀")]
        public void SecureString_GenerateEquivalentLength_CorrectSize(string secret)
        {
            var secureString = secret.ToSecureString();
            var test = secureString.GenerateEquivalentLength();
            Assert.AreEqual(secret.Length, test.Length);
        }
    }
}
