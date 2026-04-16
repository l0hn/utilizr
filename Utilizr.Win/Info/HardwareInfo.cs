using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using Utilizr.Extensions;
using Utilizr.Logging;
using Utilizr.Win32.Kernel32;
using Utilizr.Win32.Kernel32.Flags;
using Utilizr.Win32.Kernel32.Structs;

namespace Utilizr.Win.Info
{
    public static class HardwareInfo
    {
        /// <summary>
        /// Generates a unique hardware id based on various hardware serial numbers for the current system.
        /// Uses a combination of motherboard | hdd_physical_serial | processor_id
        /// </summary>
        /// <returns></returns>
        public static string GenerateUniqueHardwareID()
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
            motherboardSerial = motherboardSerial.ToLower().HashMD5();
            processorSerial = processorSerial.ToLower().HashMD5();
            hddSerial = hddSerial.ToLower().HashMD5();
            return $"{motherboardSerial}|{hddSerial}|{processorSerial}".ToLower();
        }

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
                    /*
                    * NOTE: there's dragons here, wmi (system.management) gives null (if oem doesn't fill in or for some
                    * VMs etc) despite the "SerialNumber" property existing and the do not attempt to fix this or it will 
                    * cause backwards compatibility issues for applications relying on this class as a way to uniquely
                    * auth/identify devices.
                    */

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

        /// <summary>
        /// Returns RAM size in bytes, null if an error occurred.
        /// </summary>
        /// <returns></returns>
        public static MEMORYSTATUSEX? GetSystemRamData()
        {
            try
            {
                var mem = new MEMORYSTATUSEX();
                mem.dwLength = (uint)Marshal.SizeOf(mem);

                if (!Kernel32.GlobalMemoryStatusEx(mem))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                return mem;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }

            return null;
        }

        /// <summary>
        /// The real installed RAM, which won't subtract any hardware reserved memory.
        /// Returns 0 if an error occurs.
        /// </summary>
        /// <returns></returns>
        public static long GetInstalledRamBytes()
        {
            try
            {
                if (!Kernel32.GetPhysicallyInstalledSystemMemory(out long kb))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                return kb * 1024;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }

            return 0;
        }

        /// <summary>
        /// Get the physical CPU core count, null if an error occurred. Physical
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Win32Exception"></exception>
        public static int? GetPhysicalProcessorCoreCount()
        {
            uint len = 0;
            Kernel32.GetLogicalProcessorInformation(IntPtr.Zero, ref len);

            IntPtr ptr = Marshal.AllocHGlobal((int)len);
            try
            {
                if (!Kernel32.GetLogicalProcessorInformation(ptr, ref len))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                int size = Marshal.SizeOf(typeof(SYSTEM_LOGICAL_PROCESSOR_INFORMATION));
                int count = (int)len / size;

                int physicalCores = 0;

                for (int i = 0; i < count; i++)
                {
                    var info = Marshal.PtrToStructure<SYSTEM_LOGICAL_PROCESSOR_INFORMATION>(ptr + i * size);

                    if (info.Relationship == LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorCore)
                        physicalCores++;
                }

                return physicalCores;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return null;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
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
