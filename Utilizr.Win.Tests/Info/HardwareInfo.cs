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
            //there's some nuance here:
            //GetMotherBoardSerialNumber can throw if the serialnumber doesn't exist in wmi OR it's value is null (because ToString is called on it).
            //in which case the mobo serial would become "none".
            //For maintaining backward compatiblity for any apps using this as devide identification we need to replicate the try/catch logic in GenerateUniqueHardwareID().
            //NOTE: This could also vary in future if something other than the System.Management WMI query methods are used to retrieve either the mobo serial or volume serial
            //e.g. WMI gives a null value in the existing GetMotherboardSerialNumber() method on item["SerialNumber"]
            var tryGet = (Func<string> a) =>
            {
                try
                {
                    var result = a();
                    // Console.WriteLine(result);
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                return "none";
            };

            var expected = string.Format(
                "{0}|{1}|{2}",
                Hash.MD5(tryGet(() => HardwareInfo.GetMotherboardSerialNumber().ToLower())),
                Hash.MD5(tryGet(() => HardwareInfo.GetRootHDDPhysicalSerial().ToLower())),
                Hash.MD5(tryGet(() => HardwareInfo.GetProcessorID().ToLower()))
            ).ToLower();

            Console.WriteLine($"expected={expected}");

            Assert.That(expected, Is.EqualTo(HardwareInfo.GenerateUniqueHardwareID()));
        }
    }
}
