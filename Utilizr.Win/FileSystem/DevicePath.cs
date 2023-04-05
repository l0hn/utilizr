using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Kernel32 = Utilizr.Win32.Kernel32.Kernel32;

namespace Utilizr.Win.FileSystem
{
    public static class DevicePath
    {
        private static Dictionary<string, string>? _deviceMap;
        private const string networkDevicePrefix = "\\Device\\LanmanRedirector\\";
        private const int MAX_PATH = 260;

        public static bool ConvertDevicePathToDosPath(string devicePath, out string dosPath)
        {
            EnsureDeviceMap();
            int i = devicePath.Length;
            while (i > 0 && (i = devicePath.LastIndexOf('\\', i - 1)) != -1)
            {
                if (_deviceMap?.TryGetValue(devicePath[..i], out string? drive) == true)
                {
                    dosPath = string.Concat(drive, devicePath[i..]);
                    return dosPath.Length != 0;
                }
            }
            dosPath = string.Empty;
            return false;
        }

        private static void EnsureDeviceMap()
        {
            if (_deviceMap == null)
            {
                Dictionary<string, string> localDeviceMap = BuildDeviceMap();
                Interlocked.CompareExchange<Dictionary<string, string?>>(ref _deviceMap!, localDeviceMap!, null!);
            }
        }

        private static Dictionary<string, string> BuildDeviceMap()
        {
            string[] logicalDrives = Environment.GetLogicalDrives();
            var localDeviceMap = new Dictionary<string, string>(logicalDrives.Length);
            var lpTargetPath = new StringBuilder(MAX_PATH);
            foreach (string drive in logicalDrives)
            {
                string lpDeviceName = drive[..2];
                Kernel32.QueryDosDevice(lpDeviceName, lpTargetPath, MAX_PATH);
                localDeviceMap.Add(NormalizeDeviceName(lpTargetPath.ToString()), lpDeviceName);
            }
            localDeviceMap.Add(networkDevicePrefix[..^1], "\\");
            return localDeviceMap;
        }

        private static string NormalizeDeviceName(string deviceName)
        {
            if (string.Compare(deviceName, 0, networkDevicePrefix, 0, networkDevicePrefix.Length, StringComparison.InvariantCulture) == 0)
            {
                string shareName = deviceName[(deviceName.IndexOf('\\', networkDevicePrefix.Length) + 1)..];
                return string.Concat(networkDevicePrefix, shareName);
            }
            return deviceName;
        }
    }
}
