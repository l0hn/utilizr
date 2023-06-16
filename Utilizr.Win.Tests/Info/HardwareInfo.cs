using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Utilizr.Crypto;
using Utilizr.Win.Info;

namespace Utilizr.Win.Tests.Info
{
    [TestFixture]
    public class HardwareInfoTests
    {
        [Test]
        public void TestCpuID()
        {
            string id = HardwareInfo.GetProcessorID();
            Assert.That(id, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void TestMotherboardID()
        {
            string id = HardwareInfo.GetMotherboardSerialNumber();
            Assert.That(id, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void TestVolumeSerial()
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed))
            {
                try
                {
                    string id = HardwareInfo.GetVolumeSerial(drive.RootDirectory.FullName);
                    Assert.That(id, Is.Not.Null.And.Not.Empty);
                }
                catch (HardwareInfoException)
                {
                }
            }
        }

        [Test]
        public void TestUniqueID()
        {
            var expected = string.Format(
                "{0}|{1}|{2}",
                Hash.MD5(HardwareInfo.GetMotherboardSerialNumber().ToLower()),
                Hash.MD5(HardwareInfo.GetRootHDDPhysicalSerial().ToLower()),
                Hash.MD5(HardwareInfo.GetProcessorID().ToLower())
            ).ToLower();

            Assert.That(expected, Is.EqualTo(HardwareInfo.GenerateUniqueHardwareID()));
        }
    }
}
