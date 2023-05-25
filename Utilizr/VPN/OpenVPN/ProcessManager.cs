using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Utilizr.Extensions;
using Utilizr.Logging;
//using Utilizr.Windows;

namespace Utilizr.OpenVPN
{
    [DebuggerDisplay("ExecutablePath={ExecutablePath}")]
    public class WMIProcessInfo
    {
        public Process Process { get; set; }
        public string ExecutablePath { get; set; }
        public string CommandLine { get; set; }
        public uint ParentProcessID { get; set; }
    }

    // Workaround until .NET 5.0 is out (should be fixed in that version)
    // A small wrapper around COM interop to make it more easy to use.
    // See https://github.com/dotnet/runtime/issues/12587#issuecomment-534611966
    public class COMObject : DynamicObject, IDisposable
    {
        public static COMObject CreateObject(string progID)
        {
            return new COMObject(Activator.CreateInstance(Type.GetTypeFromProgID(progID, true)));
        }

        public dynamic Instance { get; private set; }

        public COMObject(object instance)
        {
            Instance = instance;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder.Name == nameof(Instance))
            {
                result = Instance;
                return true;
            }

            result = Instance.GetType().InvokeMember(
                binder.Name,
                BindingFlags.GetProperty,
                Type.DefaultBinder,
                Instance,
                new object[] { }
            );
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Instance.GetType().InvokeMember(
                binder.Name,
                BindingFlags.SetProperty,
                Type.DefaultBinder,
                Instance,
                new object[] { WrapIfRequired(value) }
            );
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is COMObject co)
                    args[i] = co.Instance;
            }
            result = Instance.GetType().InvokeMember(
                binder.Name,
                BindingFlags.InvokeMethod,
                Type.DefaultBinder,
                Instance,
                args
            );
            result = WrapIfRequired(result);
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = WrapIfRequired(
                Instance.GetType()
                    .InvokeMember(
                        "Item",
                        BindingFlags.GetProperty,
                        Type.DefaultBinder,
                        Instance,
                        indexes
                    ));
            return true;
        }

        private static object WrapIfRequired(object obj)
            => obj != null &&
            obj.GetType().IsCOMObject
                ? new COMObject(obj)
                : obj;

        public void Dispose()
        {
            // The RCW is a .NET object and cannot be released from the finalizer,
            // because it might not exist anymore.
            if (Instance != null)
            {
                Marshal.ReleaseComObject(Instance);
                Instance = null;
            }

            GC.SuppressFinalize(this);
        }
    }

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
                    var wmiInfo = GetRunningProcess(process.Id);

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
                    var wmiInfo = GetRunningProcess(process.Id);

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


        public static WMIProcessInfo GetRunningProcess(int pid)
        {
            return InternalGetRunningProcesses($"WHERE ProcessId={pid}").FirstOrDefault();
        }

        static IEnumerable<WMIProcessInfo> InternalGetRunningProcesses(string whereClause = null)
        {
            var wmiQueryString = string.IsNullOrEmpty(whereClause)
                ? $"SELECT ProcessId, ExecutablePath, CommandLine, ParentProcessId FROM Win32_Process"
                : $"SELECT ProcessId, ExecutablePath, CommandLine, ParentProcessId FROM Win32_Process {whereClause}";

#if NETCOREAPP

            // WbemScripting is in the COM library "Microsoft WMI Scripting v1.2 Library"
            // WbemScripting.SWbemLocatorClass locator = new WbemScripting.SWbemLocatorClass();            
            var wmiResults = new[] { new { Pid = 0U, Cmd = "", Path = "", ParentPid = 0U } }.ToList();
            using (dynamic locator = COMObject.CreateObject("WbemScripting.SWbemLocator"))
            using (dynamic service = locator.ConnectServer(".", @"Root\Cimv2"))
            using (var resultsSet = service.ExecQuery(wmiQueryString))
            {
                foreach (var obj in resultsSet.Instance)
                {
                    using (dynamic wrapper = new COMObject(obj))
                    {
                        wmiResults.Add(new
                        {
                            Pid = (uint)wrapper.ProcessId,
                            Cmd = wrapper.CommandLine as string, // may be DBNull
                            Path = wrapper.ExecutablePath as string, // as above
                            ParentPid = (uint)wrapper.ParentProcessId,
                        });
                    }
                }
            }

            var results = Process.GetProcesses()
                .Join(
                    wmiResults,
                    p => (uint)p.Id,
                    wmiObj => wmiObj.Pid,
                    (p, wmiObj) => new WMIProcessInfo()
                    {
                        Process = p,
                        ExecutablePath = wmiObj.Path,
                        CommandLine = wmiObj.Cmd,
                        ParentProcessID = wmiObj.ParentPid,
                    }
                )
                // appears to be null if lacking permissions, filter out any
                .Where(p => p.ExecutablePath.IsNotNullOrEmpty())
                .ToList();

            return results;

#else
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            using (var results = searcher.Get())
            {
                var query = from p in Process.GetProcesses()
                            join mo in results.Cast<ManagementObject>()
                            on p.Id equals (int)(uint)mo["ProcessId"]
                            select new WMIProcessInfo()
                            {
                                Process = p,
                                ExecutablePath = (string)mo["ExecutablePath"],
                                CommandLine = (string)mo["CommandLine"],
                                ParentProcessID = (uint)mo["ParentProcessId"],
                            };
                return query.ToList();
            }
#endif
        }

    }
}