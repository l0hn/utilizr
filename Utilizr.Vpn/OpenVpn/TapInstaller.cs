using Utilizr.FileSystem;
using Utilizr.Info;
using Utilizr.Logging;
using Utilizr.Win;

namespace Utilizr.Vpn.OpenVpn
{
    public static class TapInstaller
    {
        const string LOG_CAT = "tap-driver";

        public static bool InstallTapDriver(bool requestAdmin = false)
        {
            var driverDir = GetDriverDir();
            var devconPath = Path.Combine(driverDir, "devcon.exe");
            UninstallTapDriver();
            var instResult = Shell.Exec(devconPath, driverDir, requestAdmin, "install", "OemWin2k.inf", "tap0901");
            if (!string.IsNullOrEmpty(instResult.Output))
            {
                Log.Info(LOG_CAT, instResult.Output);
            }
            if (!string.IsNullOrEmpty(instResult.ErrorOutput))
            {
                Log.Info(LOG_CAT, instResult.ErrorOutput);
            }

            Log.Info(LOG_CAT, $"Tap driver install exit code: {instResult.ExitCode}");

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
            Log.Info(LOG_CAT, uninstResult.Output);
            if (!string.IsNullOrEmpty(uninstResult.ErrorOutput))
            {
                Log.Info(LOG_CAT, uninstResult.ErrorOutput);
            }
        }

        static string GetDriverDir()
        {
            var driverDir = Path.Combine(AppInfo.AppDirectory, "OpenVpn", "driver");
            if (OVPNProcess.OverridePath != null)
                driverDir = Path.Combine(OVPNProcess.OverridePath, "OpenVpn", "driver");

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
                using (dynamic locator = COMObject.CreateObject("WbemScripting.SWbemLocator"))
                using (dynamic service = locator.ConnectServer(".", @"Root\Cimv2"))
                using (var drivers = service.ExecQuery(wql))
                {
                    installed = drivers.Count > 0;
                }
            }
            catch(Exception ex)
            {
                Log.Exception(LOG_CAT, ex);
            }

            return installed;
        }

        public static uint GetTapAdapterIndex()
        {
            return GetTapAdapterInfo().Index;
        }

        public static AdapterInfo GetTapAdapterInfo()
        {
            var query = "SELECT * FROM Win32_NetworkAdapter WHERE ServiceName='tap0901' AND Name='TAP-Windows Adapter V9'";

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
                    catch (Exception)
                    {

                    }

                    string? guid = null;

                    if (!Platform.IsXPOrLower)
                    {
                        guid = (string)wrapper.GUID;
                    }

                    var index = Platform.IsXPOrLower
                        ? (uint)wrapper.Index
                        : (uint)wrapper.InterfaceIndex;

                    return new AdapterInfo()
                    {
                        GUID = guid,
                        Index = index,
                    };
                }
            }

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
                Log.Exception(LOG_CAT, e);
                return false;
            }
        }

    }

    public class AdapterInfo
    {
        public string GUID { get; set; }
        public uint Index { get; set; }
    }
}
