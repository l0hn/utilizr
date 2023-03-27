using System;
using System.Management;
using Microsoft.Win32;
using Utilizr.Logging;

namespace Utilizr.Info
{
    public class HardwareInfo
    {
        private string _motherboardSerial = string.Empty;

        public string GetMotherboardSerialNumber()
        {
            var query = "Select * From Win32_BaseBoard";
            if (string.IsNullOrEmpty(_motherboardSerial))
            {
                string id = "";
                using var searcher = new ManagementObjectSearcher(query);
                foreach (ManagementObject item in searcher.Get())
                {
                    id += item["SerialNumber"].ToString();
                }
                _motherboardSerial = id;
            }
            return _motherboardSerial;
        }


        private string _processorId = string.Empty;
        public string GetProcessorID()
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
                        id = item["processorID"].ToString();
                        if (!string.IsNullOrEmpty(id))
                            break;
                    }
                    catch (Exception)
                    {
                    }
                }
                _processorId = id;
            }
            return _processorId;
        }

        public string GetVolumeSerial()
        {
            string defaultVolume = Environment.ExpandEnvironmentVariables("%homedrive%");
            return GetVolumeSerial(defaultVolume);
        }


        public string? GetVolumeSerial(string volume)
        {
            try
            {
                volume = volume.TrimEnd('\\', ':');
                using var disk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + volume + @":""");
                disk.Get();
                string volumeSerial = disk["VolumeSerialNumber"].ToString();
                return volumeSerial;
            }
            catch (Exception err)
            {
                throw new HardwareInfoException("Could not retrieve volume serial number", err);
            }

            throw new HardwareInfoException("Could not retrieve volume serial. MONO directive set but using WindowsHardwareInfo object.");
        }

        public string GetComputerName()
        {
            return Environment.MachineName;
        }

        public string GetRootHDDPhysicalSerial()
        {
            try
            {
                var query = "SELECT Tag, SerialNumber FROM win32_physicalmedia";
                var phsyicalZero = "\\\\.\\PHYSICALDRIVE0";

                var searcher = new ManagementObjectSearcher(query);
                foreach (ManagementObject managementObject in searcher.Get())
                {
                    if (managementObject.Properties["Tag"].Value.ToString() == phsyicalZero)
                    {
                        return managementObject.Properties["SerialNumber"].Value.ToString();
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
                machineGuid = regKey.GetValue("MachineGuid").ToString();
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