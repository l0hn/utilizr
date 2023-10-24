using NUnit.Framework;
using Utilizr.Globalisation.Extensions;

namespace Utilizr.Globalisation.Tests.Extensions
{
    [TestFixture]
    class ByteConverterTests
    {
        [Test]
        [TestCase(0, ExpectedResult="0 B")]
        [TestCase(1, ExpectedResult = "1 B")]
        [TestCase(-1, ExpectedResult = "-1 B")]
        [TestCase(10, ExpectedResult = "10 B")]
        [TestCase(-10, ExpectedResult = "-10 B")]
        [TestCase(1 << 10, ExpectedResult = "1 KB")]
        [TestCase(1 << 20, ExpectedResult = "1 MB")]
        [TestCase(-(1 << 20), ExpectedResult = "-1 MB")]
        [TestCase((int)((1 << 20) * 1.5), ExpectedResult = "1.5 MB")]
        [TestCase(1 << 30, ExpectedResult = "1 GB")]
        [TestCase(1L << 40, ExpectedResult = "1 TB")]
        [TestCase(1L << 50, ExpectedResult = "1 PB")]
        [TestCase((long)((1L << 50) * 1.18), ExpectedResult = "1.18 PB")]
        [TestCase(1L << 60, ExpectedResult = "1 EB")]
        [TestCase(int.MaxValue, ExpectedResult = "2 GB")]
        [TestCase(int.MinValue, ExpectedResult = "-2 GB")]
        [TestCase(long.MaxValue, ExpectedResult = "8 EB")]
        [TestCase(long.MinValue, ExpectedResult = "-8 EB")] 
        public string TestOutput(long size)
        {
            return ByteConverter.ToBytesString(size, false, out _, out _, 0, false);
        }
    
        [Test]
        [TestCase(10, ExpectedResult = "10 B")]
        [TestCase(1 << 30, ExpectedResult = "1 GB")]
        public string TestExtensions(long size)
        {
            return size.ToBytesString(false, out _, out _, 0, true);
        }
    }
}
