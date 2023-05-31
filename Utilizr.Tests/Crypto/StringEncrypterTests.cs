using NUnit.Framework;
using Utilizr.Crypto;

namespace Utilizr.Tests.Crypto
{
    [TestFixture]
    [Category("Crypto")]
    public class StringEncrypterTests
    {
        [Test]
        [TestCase("The cat sat in the hat then did a shat")]
        [TestCase("坐在猫的帽子，然后做了一个歇脚")]
        [TestCase("די קאַץ Sat אין די הוט דעמאָלט האט אַ שאַט")]
        [TestCase("猫はSHATをした後、帽子に座っ")]
        [TestCase("Mačka sjedila u šeširu zatim učinio Shat")]
        [TestCase("Кошка сидела в шляпе тогда сделали Шат")]
        [TestCase("القط جلس في قبعة ثم قام شط")]
        [TestCase("Con mèo ngồi trong chiếc mũ sau đó đã làm một shat")]
        [TestCase("बिल्ली एक शाट था तब टोपी में बैठे")]
        [TestCase("cat ໄດ້ນັ່ງໃນຫລີກໄດ້ຫຼັງຈາກນັ້ນໄດ້ sh+at ເປັນ")]
        public void StringEncyptionTest(string message)
        {
            string passphrase = "איר שמעקן ווי אַ מאַלפּע 'ס טאָכעס| 你闻起来像一只猴子的屁股";
            string encrypted = StringEncrypter.EncryptString(message, passphrase);
            string original = StringEncrypter.DecryptString(encrypted, passphrase);
            Assert.AreEqual(message, original);
        }
    }
}
