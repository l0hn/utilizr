using Microsoft.Win32;
using System;
using System.Linq;
using System.Management;
using Utilizr.Logging;

namespace Utilizr.Info
{
    public static class HardwareInfo
    {
        private static string _motherboardSerial = string.Empty;

        public static string GetMotherboardSerialNumber()
        {
            var query = "Select * From Win32_BaseBoard";
            if (string.IsNullOrEmpty(_motherboardSerial))
            {
                string id = "";
                using var searcher = new ManagementObjectSearcher(query);
                foreach (var item in searcher.Get().Cast<ManagementObject>())
                {
                    id += item["SerialNumber"].ToString();
                }
                _motherboardSerial = id;
            }
            return _motherboardSerial;
        }


        private static string _processorId = string.Empty;
        public static string GetProcessorID()
        {
            if (string.IsNullOrEmpty(_processorId))
            {
                string id = "";
                var query = "Select * From Win32_Processor";
                using var searcher = new ManagementObjectSearcher(query);
                foreach (var item in searcher.Get())
                {
                    try
                    {
                        if (item != null)
                        {
                            id = item["processorID"].ToString() ?? string.Empty;
                            if (!string.IsNullOrEmpty(id))
                                break;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                _processorId = id;
            }
            return _processorId;
        }

        public static string? GetVolumeSerial()
        {
            string defaultVolume = Environment.ExpandEnvironmentVariables("%homedrive%");
            return GetVolumeSerial(defaultVolume);
        }


        public static string GetVolumeSerial(string volume)
        {
            try
            {
                volume = volume.TrimEnd('\\', ':');
                using var disk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + volume + @":""");
                disk.Get();
                var volumeSerial = disk["VolumeSerialNumber"].ToString();

                if (string.IsNullOrEmpty(volumeSerial))
                    throw new HardwareInfoException("Retrieved VolumeSerialNumber null or empty");

                return volumeSerial;
            }
            catch (Exception err)
            {
                throw new HardwareInfoException("Could not retrieve volume serial number", err);
            }

            throw new HardwareInfoException("Could not retrieve volume serial. MONO directive set but using WindowsHardwareInfo object.");
        }

        public static string GetComputerName()
        {
            return Environment.MachineName;
        }

        public static string GetRootHDDPhysicalSerial()
        {
            try
            {
                var query = "SELECT Tag, SerialNumber FROM win32_physicalmedia";
                var phsyicalZero = "\\\\.\\PHYSICALDRIVE0";

                var searcher = new ManagementObjectSearcher(query);
                foreach (var managementObject in searcher.Get().Cast<ManagementObject>())
                {
                    if (managementObject.Properties["Tag"].Value.ToString() == phsyicalZero)
                    {
                        var diskSerial = managementObject.Properties["SerialNumber"].Value.ToString();
                        if (string.IsNullOrEmpty(diskSerial))
                            throw new HardwareInfoException("Retrieved physical serial for root hdd null or empty.");

                        return diskSerial;
                    }
                }
            }
            catch (Exception err)
            {
                throw new HardwareInfoException("Could not retrieve physical serial for root hdd", err);
            }

            throw new HardwareInfoException("Could not retrieve physical serial for root hdd");
        }

        public static string GetWindowsMachineGuid()
        {
            string machineGuid = "";
            try
            {
                using var regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
                machineGuid = regKey?.GetValue("MachineGuid")?.ToString() ?? "";
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return machineGuid;
        }
    }

    public class HardwareInfoException : Exception
    {
        public HardwareInfoException(string message) : base(message)
        {

        }

        public HardwareInfoException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}