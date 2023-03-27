using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;

namespace Utilizr.Info
{
    public static class Platform
    {
        static readonly Dictionary<string, OperatingSystem> _winOSLookup = new Dictionary<string, OperatingSystem>()
        {
            {"5.1", OperatingSystem.XP},
            {"5.2", OperatingSystem.XP},
            {"6.0", OperatingSystem.Vista},
            {"6.1", OperatingSystem.Win7},
            {"6.2", OperatingSystem.Win8},
            {"6.3", OperatingSystem.Win8_1},
            {"10.0", OperatingSystem.Win10},
            {"10.0.22000", OperatingSystem.Win11},
        };

        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsXPOrLower => IsWindows && OSVersion < OperatingSystem.Vista;

        public static bool IsVistaAndLower => IsWindows && OSVersion < OperatingSystem.Win7;

        public static bool IsVistaAndHigher => IsWindows && OSVersion > OperatingSystem.XP;

        public static bool IsWin7OrHigher => IsWindows && OSVersion >= OperatingSystem.Win7;

        public static bool IsWin8AndHigher => IsWindows && OSVersion >= OperatingSystem.Win8;

        /// <summary>
        /// Is windows 8 or windows 8.1
        /// </summary>
        public static bool IsWin8Variant => IsWindows && (OSVersion == OperatingSystem.Win8 || OSVersion == OperatingSystem.Win8_1);

        public static bool IsWin10AndHigher => IsWindows && OSVersion >= OperatingSystem.Win10;

        private static Version RS5Version = new Version(10, 0, 17763);
        public static bool IsRS5OrHigher => Environment.OSVersion.Version >= RS5Version;

        private static Version Win10_20H1 = new Version(10, 0, 19000);
        public static bool IsWin10_20H1Plus => Environment.OSVersion.Version >= Win10_20H1;

        /// <summary>
        /// Lowest windows 11 version number since it still starts with 10.0
        /// </summary>
        public static Version Win11_Min => new Version(10, 0, 22000);


        /// <summary>
        /// Identifies if the OS is 64bit
        /// </summary>
        /// <remarks>
        /// This method replaces Is64Bit method, it now returns the OS Bitness instead of the running process Bitness
        /// If you require the old functionality please use Is64BitProcess instead
        /// </remarks>
        public static bool Is64BitOS => RuntimeInformation.OSArchitecture == Architecture.X64 ||
                                        RuntimeInformation.OSArchitecture == Architecture.Arm64;

        /// <summary>
        /// Indicates if the application process is a 32Bit
        /// </summary>
        public static bool Is32BitProcess => RuntimeInformation.ProcessArchitecture == Architecture.X86 ||
                                             RuntimeInformation.ProcessArchitecture == Architecture.Arm;


        /// <summary>
        /// Indicates if the application process is a 64Bit
        /// </summary>
        public static bool Is64BitProcess => RuntimeInformation.ProcessArchitecture == Architecture.X64 ||
                                             RuntimeInformation.ProcessArchitecture == Architecture.Arm64;

        public static OperatingSystem OSVersion
        {
            get
            {
                int major = Environment.OSVersion.Version.Major;
                int minor = Environment.OSVersion.Version.Minor;
                string vLookup = $"{major}.{minor}";
                _winOSLookup.TryGetValue(vLookup, out OperatingSystem osVersion);

                // A little hacky since all other OS just need major.minor, but win 11
                // needs major.minor.build
                if (osVersion == OperatingSystem.Win10)
                {
                    osVersion = Environment.OSVersion.Version >= Win11_Min
                        ? OperatingSystem.Win11
                        : OperatingSystem.Win10;
                }

                return osVersion;
            }
        }

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


    public enum OperatingSystem
    {
        XP,
        Vista,
        Win7,
        Win8,
        Win8_1,
        Win10,
        Win11,
    }
}