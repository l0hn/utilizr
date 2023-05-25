using System;
using Microsoft.Win32;
//using Utilizr.Crypto;
//using Utilizr.Logging;
using Utilizr.Win;
using Utilizr.Win.Info;
//using Utilizr.Windows;
#if !MONO && !NETCOREAPP
using System.Management;
#endif

namespace Utilizr.Hardware
{
    public class WindowsHardwareInfo
    {
        /// <summary>
        /// Generates a unique hardware id based on various hardware serial numbers for the current system.
        /// Uses a combination of motherboard | hdd_physical_serial | processor_id
        /// </summary>
        /// <returns></returns>
        public string GenerateUniqueHardwareID()
        {
            var motherboardSerial = "none";
            var processorSerial = "none";
            var hddSerial = "none";
            try
            {
                motherboardSerial = GetMotherboardSerialNumber();
            }
            catch (Exception)
            {
            }
            try
            {
                processorSerial = GetProcessorID();
            }
            catch (Exception)
            {
            }
            try
            {
                hddSerial = GetRootHDDPhysicalSerial();
            }
            catch (Exception)
            {
            }

            motherboardSerial = Crypto.Hash.MD5(motherboardSerial.ToLower());
            processorSerial = Crypto.Hash.MD5(processorSerial.ToLower());
            hddSerial = Crypto.Hash.MD5(hddSerial.ToLower());
            return string.Format("{0}|{1}|{2}", motherboardSerial, hddSerial, processorSerial).ToLower();
        }


        private string _motherboardSerial = string.Empty;
        public string GetMotherboardSerialNumber()
        {
#if !MONO
            var query = "Select * From Win32_BaseBoard";
            if (string.IsNullOrEmpty(_motherboardSerial))
            {
                string id = "";
#if NETCOREAPP
                using (dynamic locator = COMObject.CreateObject("WbemScripting.SWbemLocator"))
                {
                    dynamic service = locator.ConnectServer(".", @"Root\Cimv2");
                    var results = service.ExecQuery(query);

                    foreach (var p in results.Instance)
                    {
                        dynamic wrapper = new COMObject(p);
                        id += wrapper.SerialNumber;
                    }
                }
#else
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                foreach (ManagementObject item in searcher.Get())
                {
                    id += item["SerialNumber"].ToString();
                }                
#endif
                _motherboardSerial = id;
            }
#endif
            return _motherboardSerial;
        }


        private string _processorId = string.Empty;
        public string GetProcessorID()
        {
#if !MONO
            if (string.IsNullOrEmpty(_processorId))
            {
                string id = "";
                var query = "Select * From Win32_Processor";
#if NETCOREAPP
                using (dynamic locator = COMObject.CreateObject("WbemScripting.SWbemLocator"))
                {
                    dynamic service = locator.ConnectServer(".", @"Root\Cimv2");
                    var results = service.ExecQuery(query);

                    foreach (var p in results.Instance)
                    {
                        try
                        {
                            using (dynamic wrapper = new COMObject(p))
                            {
                                id = wrapper.ProcessorID;

                                if (!string.IsNullOrEmpty(id))
                                    break;
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
#else
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
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
#endif
                _processorId = id;
            }
#endif
            return _processorId;
        }

        public string GetVolumeSerial()
        {            
            string defaultVolume = Environment.ExpandEnvironmentVariables("%homedrive%");
            return GetVolumeSerial(defaultVolume);
        }


        public string GetVolumeSerial(string volume)
        {
#if !MONO
            try
            {
                volume = volume.TrimEnd('\\', ':');
#if NETCOREAPP
                var query = "Select * From Win32_LogicalDisk";
                using (dynamic locator = COMObject.CreateObject("WbemScripting.SWbemLocator"))
                {
                    dynamic service = locator.ConnectServer(".", @"Root\Cimv2");
                    var disks = service.ExecQuery(query);

                    foreach (var disk in disks.Instance)
                    {
                        using (dynamic wrapper = new COMObject(disk))
                        {
                            if (wrapper.DeviceID.StartsWith(volume))
                                return wrapper.VolumeSerialNumber;
                        }
                    }
                }
#else
                using (ManagementObject disk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + volume + @":"""))
                {
                    disk.Get();
                    string volumeSerial = disk["VolumeSerialNumber"].ToString();
                    return volumeSerial;                
                }            
#endif
            }
            catch (Exception err)
            {
                throw new HardwareInfoException("Could not retrieve volume serial number", err);
            }
#endif

            throw new HardwareInfoException("Could not retrieve volume serial. MONO directive set but using WindowsHardwareInfo object.");
        }

        public string GetComputerName()
        {
            return Environment.MachineName;
        }

        public string GetRootHDDPhysicalSerial()
        {
#if !MONO
            try
            {
                var query = "SELECT Tag, SerialNumber FROM win32_physicalmedia";
                var phsyicalZero = "\\\\.\\PHYSICALDRIVE0";
#if NETCOREAPP
                using (dynamic locator = COMObject.CreateObject("WbemScripting.SWbemLocator"))
                {
                    dynamic service = locator.ConnectServer(".", @"Root\Cimv2");
                    var results = service.ExecQuery(query);

                    foreach (var p in results.Instance)
                    {
                        using (dynamic wrapper = new COMObject(p))
                        {
                            if (wrapper.Tag == phsyicalZero)
                                return wrapper.SerialNumber;
                        }
                    }
                }
#else
                var searcher = new ManagementObjectSearcher(query);
                foreach (ManagementObject managementObject in searcher.Get())
                {
                    if (managementObject.Properties["Tag"].Value.ToString() == phsyicalZero)
                    {
                        return managementObject.Properties["SerialNumber"].Value.ToString();
                    }
                }
#endif
            }
            catch (Exception err)
            {
                
            }
#endif
            throw new HardwareInfoException("Could not retrieve physical serial for root hdd");
        }

        public static string GetWindowsMachineGuid()
        {
            string machineGuid = null;
            #if !MONO
            try
            {
                //using (var regKey = Registry.LocalMachine.OpenSubKeyWow6432(@"SOFTWARE\Microsoft\Cryptography", false, false))
                //{
                //    machineGuid = regKey.GetValue("MachineGuid").ToString();
                //}
            }
            catch (Exception e)
            {
                //Log.Exception(e);
            }
            #endif
            return machineGuid;
        }
    }
}