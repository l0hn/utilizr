using System;
using System.Diagnostics;
using System.Threading;
using Utilizr.Logging;
//using Utilizr.Windows;
using Utilizr.Win.Info;

namespace Utilizr.OpenVPN
{
    public static class ProcessManager
    {
        private static EventWaitHandle _handle;
        public static OVPNProcess CurrentProcess { get; private set; }
        
        /// <summary>
        /// Launches an open vpn client process and returns the management interface port number
        /// </summary>
        /// <param name="remote"></param>
        /// <param name="caFile"></param>
        /// <param name="proto"></param>
        /// <param name="pingTimeout"></param>
        /// <returns></returns>
        public static int LaunchOpenVPN(string remote, string caFile, string proto = "udp", int port = 1194, int pingTimeout = 30, string upScript = null, string downScript = null)
        {
            StopAllOpenVPNProcesses();
            CurrentProcess = OVPNProcess.WithCAFile(remote, caFile, proto, pingTimeout, upScript, downScript);
            return CurrentProcess.ManagementPort;
        }

        public static int LaunchOpenVPNWithConfig(string remote, string configFile, string upScript = null, string downScript = null, int pingTimeout = 30, string managementPwd = null, int port = 1194, string protocol = "udp")
        {
            StopAllOpenVPNProcesses();
            CurrentProcess = OVPNProcess.FromConfig(remote, configFile, upScript, downScript, pingTimeout, managementPwd, port, protocol);
            return CurrentProcess.ManagementPort;
        }

        public static void StopAllProcessByName(string logCat, string processName, int? timeout = null)
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                try
                {
                    var wmiInfo = ProcessHelper.GetRunningProcess(process.Id);

                    if (wmiInfo == null)
                    {
                        Log.Info(logCat, $"Killing process {process.Id}");
                    }
                    else
                    {
                        Log.Info(logCat, $"Killing process {process.Id}, \"{wmiInfo.ExecutablePath}\"{Environment.NewLine}Command Line: \"{wmiInfo.CommandLine}\"");
                    }
                    process.Kill();

                    if (timeout.HasValue)
                        process.WaitForExit((int)timeout);
                    else process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Log.Exception(logCat, ex);
                }
            }
        }

        public static void StopAllOpenVPNProcesses()
        {
            try
            {
                if (CurrentProcess != null)
                {
                    Log.Info("ovpn_procman", "Closing existing ovpn process");
                    CurrentProcess.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Exception("ovpn_procman", ex);
            }
            foreach (var process in Process.GetProcessesByName("openvpn"))
            {
                try
                {
                    var wmiInfo = ProcessHelper.GetRunningProcess(process.Id);

                    if (wmiInfo == null)
                    {
                        Log.Info("OPEN_VPN", $"Killing process {process.Id}");
                    }
                    else
                    {
                        Log.Info("OPEN_VPN", $"Killing process {process.Id}, \"{wmiInfo.ExecutablePath}\"{Environment.NewLine}Command Line: \"{wmiInfo.CommandLine}\"");
                    }
                    process.Kill();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Log.Exception("OPEN_VPN", ex);
                }
            }
        }
    }
}