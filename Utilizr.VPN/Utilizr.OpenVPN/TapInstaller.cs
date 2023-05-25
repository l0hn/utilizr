using System;
using System.IO;
using System.Threading;
using Utilizr.Info;
using Utilizr.Logging;
#if !MONO && !NETCOREAPP
using System.Management;
#endif

namespace Utilizr.OpenVPN
{


    public static class TapInstaller
    {
#if !MONO
        public static bool InstallTapDriver(bool requestAdmin = false)
        {
            var driverDir = GetDriverDir();
            var devconPath = Path.Combine(driverDir, "devcon.exe");
            UninstallTapDriver();
            var instResult = Shell.Exec(devconPath, driverDir, requestAdmin, "install", "OemWin2k.inf", "tap0901");
            if (instResult.Output.IsNotNullOrEmpty())
            {
                Log.Info("TAP_DRIVER", instResult.Output);
            }
            if (instResult.ErrorOutput.IsNotNullOrEmpty())
            {
                Log.Info("TAP_DRIVER", instResult.ErrorOutput);
            }

            Log.Info("TAP_DRIVER", $"Tap driver install exit code: {instResult.ExitCode}");

            //sometimes there's a delay for the adapter to be available.. :/
            if (instResult.ExitCode == 0)
            {
                var ellapsed = 0;
                while (!IsInstalled())
                {
                    if (ellapsed > 10000)
                        break;

                    Thread.Sleep(500);
                    ellapsed += 500;
                }
            }

            return instResult.ExitCode == 0;
        }

        public static void UninstallTapDriver()
        {
            var driverDir = GetDriverDir();
            var devconPath = Path.Combine(driverDir, "devcon.exe");
            var uninstResult = Shell.Exec(devconPath, driverDir, false, "remove", "tap0901");
            Log.Info("TAP_DRIVER", uninstResult.Output);
            if (uninstResult.ErrorOutput.IsNotNullOrEmpty())
            {
                Log.Info("TAP_DRIVER", uninstResult.ErrorOutput);
            }
        }

        static string GetDriverDir()
        {
            var driverDir = Path.Combine(AppInfo.AppDirectory, "driver");
            driverDir = Path.Combine(driverDir, Platform.Is64BitOS ? "amd64" : "i386");
            return driverDir;
        }


        /// <summary>
        /// Whether tap0901 is installed. Null if unable to check.
        /// NOTE: Unreliable. Seems to return true if it has been installed at some point, and tap0901.sys is
        /// still within C:\Windows\System32\Drivers, despite running 'devcon.exe remove tap0901' and it not
        /// being listed within device manager.
        /// </summary>
        /// <returns></returns>
        public static bool? HasDriverInstalledVistaPlus()
        {
            if (Platform.IsXPOrLower)
                throw new InvalidOperationException("Only supported on Windows Vista+");

            var wql = "SELECT * FROM Win32_SystemDriver WHERE Name='tap0901'";
            bool? installed = null;
            try
            {
#if NETCOREAPP
                using (dynamic locator = COMObject.CreateObject("WbemScripting.SWbemLocator"))
                using (dynamic service = locator.ConnectServer(".", @"Root\Cimv2"))
                using (var drivers = service.ExecQuery(wql))
                {
                    installed = drivers.Count > 0;
                }
#else
                var query = new ManagementObjectSearcher(wql);
                var drivers = query.Get();
                installed = drivers.Count > 0;
#endif

            }
            catch(Exception ex)
            {
                Log.Exception("TAP_DRIVER", ex);
            }
            
            return installed;
        }

        public static UInt32 GetTapAdapterIndex()
        {
            return GetTapAdapterInfo().Index;
        }

        public static AdapterInfo GetTapAdapterInfo()
        {
            var query = "SELECT * FROM Win32_NetworkAdapter WHERE ServiceName='tap0901' AND Name='TAP-Windows Adapter V9'";

#if NETCOREAPP
            using(dynamic locator = COMObject.CreateObject("WbemScripting.SWbemLocator"))
            using (dynamic service = locator.ConnectServer(".", @"Root\Cimv2"))
            using (var resultsSet = service.ExecQuery(query))
            {
                foreach (var obj in resultsSet.Instance)
                {
                    dynamic wrapper = new COMObject(obj);
                    try
                    {
                        var description = (string)wrapper.Description;
                        Console.WriteLine(description);
                        if (description.ToLowerInvariant().Contains("windscribe"))
                        {
                            continue;
                        }
                    }
                    catch (Exception e)
                    {

                    }

                    string guid = null;

                    if (!Platform.IsXPOrLower)
                    {
                        guid = (string)wrapper.GUID;
                    }

                    var index = Platform.IsXPOrLower
                        ? (UInt32)wrapper.Index
                        : (UInt32)wrapper.InterfaceIndex;

                    return new AdapterInfo()
                    {
                        GUID = guid,
                        Index = index,
                    };
                }
            }
#else
            using (var wmi = new ManagementObjectSearcher(query))
            using (var objCol = wmi.Get())
            {
                var propName = Platform.IsXPOrLower ? "Index" : "InterfaceIndex";

                foreach (var obj in objCol)
                {
                    try
                    {
                        var description = (string)obj.GetPropertyValue("Description");
                        Console.WriteLine(description);
                        if (description.ToLowerInvariant().Contains("windscribe"))
                        {
                            continue;
                        }
                    }
                    catch (Exception e)
                    {

                    }

                    string guid = null;

                    if (!Platform.IsXPOrLower)
                    {
                        guid = (string)obj.GetPropertyValue("GUID");
                    }

                    var index = (UInt32)obj[propName];

                    return new AdapterInfo()
                    {
                        GUID = guid,
                        Index = index,
                    };
                }
            }
#endif

            throw new Exception("Failed to locate vpn adapter index");
        }

        public static bool IsInstalled()
        {
            try
            {
                GetTapAdapterIndex();
                return true;
            }
            catch (Exception e)
            {
                Log.Exception("tap_adapter", e);
                return false;
            }
        }
#endif

            }

    public class AdapterInfo
    {
        public string GUID { get; set; }
        public UInt32 Index { get; set; }
    }
}
