using Microsoft.Win32;
using System;
using System.Linq;
using System.Management;

namespace Utilizr.Win.Info
{
    /// <summary>
    /// Windows specific properties that cannot be in a target lacking the "-windows" target framework moniker.
    /// </summary>
    public static class WinPlatform
    {
        public static string[] DotNetFrameworksInstalled => DotNet.InstalledFrameworks().ToArray();

        /// <summary>
        /// Human readable version of OSVersion. E.g. Microsoft Windows 10 Pro
        /// </summary>
        public static string OSVersionHuman
        {
            get
            {
                string? windowsVer = null;
                try
                {
                    var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion");
                    windowsVer = key?.GetValue("ProductName", string.Empty) as string;
                }
                catch { /* swallow */ }

                if (!string.IsNullOrEmpty(windowsVer))
                    return windowsVer;

                return Environment.OSVersion.VersionString;
            }
        }


        private static ulong? _ram;
        /// <summary>
        /// Total amount of RAM installed on the computer, not how much is in use.
        /// </summary>
        public static ulong Ram
        {
            get
            {
                if (!_ram.HasValue)
                {
                    const string capcity = "Capacity";
                    var query = $"SELECT {capcity} FROM Win32_PhysicalMemory";
                    var searcher = new ManagementObjectSearcher(query);
                    foreach (var wmiPart in searcher.Get())
                    {
                        _ram = Convert.ToUInt64(wmiPart.Properties[capcity].Value);
                        break;
                    }
                }
                return _ram ?? 0UL;
            }
        }
    }
}
