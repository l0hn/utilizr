using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Utilizr.Win32.Advapi32;
using Utilizr.Win32.Advapi32.Flags;
using Utilizr.Win32.Advapi32.Structs;

namespace Utilizr.Win.Info
{
    public static class ProcessHelper
    {
        public static bool ProcessOwnedByUser(int pid, string userSID)
        {
            IntPtr pToken = IntPtr.Zero;
            var process = Process.GetProcessById(pid);

            if (Advapi32.OpenProcessToken(process.Handle, (uint)TOKEN_PRIVILEGE_FLAGS.TOKEN_QUERY, ref pToken) != 0)
            {
                IntPtr pSidPtr = IntPtr.Zero;
                if (ProcessTokenToSID(pToken, out pSidPtr))
                {
                    string pSidStr = string.Empty;
                    Advapi32.ConvertSidToStringSid(pSidPtr, ref pSidStr);
                    return userSID.Equals(pSidStr, StringComparison.OrdinalIgnoreCase);
                }
            }
            return false;
        }

        private static bool ProcessTokenToSID(IntPtr token, out IntPtr pSID)
        {
            pSID = IntPtr.Zero;
            TOKEN_USER tokUser;
            const int bufLength = 256;
            IntPtr tu = Marshal.AllocHGlobal(bufLength);
            bool result = false;
            try
            {
                int cb = bufLength;
                result = Advapi32.GetTokenInformation(token, TOKEN_INFORMATION_CLASS.TokenUser, tu, cb, ref cb);
                if (result)
                {
                    tokUser = (TOKEN_USER)Marshal.PtrToStructure(tu, typeof(TOKEN_USER));
                    pSID = tokUser.User.Sid;
                }
                return result;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                Marshal.FreeHGlobal(tu);
            }
        }

        public static WMIProcessInfo? GetRunningProcess(string processName)
        {
            return GetRunningProcesses(processName).FirstOrDefault();
        }

        public static WMIProcessInfo? GetRunningProcess(int pid)
        {
            return InternalGetRunningProcesses($"WHERE ProcessId={pid}").FirstOrDefault();
        }

        public static IEnumerable<WMIProcessInfo> GetRunningProcesses()
        {
            return InternalGetRunningProcesses(null);
        }

        public static IEnumerable<WMIProcessInfo> GetRunningProcesses(string processName)
        {
            return InternalGetRunningProcesses($"WHERE Name=\"{processName}\"");
        }

        public static WMIProcessInfo? GetRunningParentProcess()
        {
            return GetRunningParentProcess(Process.GetCurrentProcess().Id);
        }

        public static WMIProcessInfo? GetRunningParentProcess(int processID)
        {
            var running = GetRunningProcesses();
            var pInfo = running.FirstOrDefault(p => p.Process.Id == processID);

            if (pInfo == null)
                return null;

            return running.FirstOrDefault(p => p.Process.Id == pInfo.ParentProcessID);
        }

        public static IEnumerable<WMIProcessInfo> GetRunningProcessesForFile(string filePath)
        {
            return GetRunningProcesses()
                   .Where(i => i.ExecutablePath != null && 
                               i.ExecutablePath.Equals(filePath, StringComparison.InvariantCultureIgnoreCase));
        }

        static IEnumerable<WMIProcessInfo> InternalGetRunningProcesses(string? whereClause = null)
        {
            var wmiQueryString = string.IsNullOrEmpty(whereClause)
                ? $"SELECT ProcessId, ExecutablePath, CommandLine, ParentProcessId FROM Win32_Process"
                : $"SELECT ProcessId, ExecutablePath, CommandLine, ParentProcessId FROM Win32_Process {whereClause}";

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
                            Cmd = wrapper.CommandLine as string ?? string.Empty, // may be DBNull
                            Path = wrapper.ExecutablePath as string ?? string.Empty, // as above
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
                    (p, wmiObj) => new WMIProcessInfo(p, wmiObj.Path, wmiObj.Cmd, wmiObj.ParentPid)
                )
                // appears to be null if lacking permissions, filter out any
                .Where(p => !string.IsNullOrEmpty(p.ExecutablePath))
                .ToList();

            return results;
        }


        [DebuggerDisplay("ExecutablePath={ExecutablePath}")]
        public class WMIProcessInfo
        {
            public Process Process { get; set; }
            public string ExecutablePath { get; set; }
            public string CommandLine { get; set; }
            public uint ParentProcessID { get; set; }

            public WMIProcessInfo(Process process, string executablePath, string commandLine, uint parentPid)
            {
                Process = process;
                ExecutablePath = executablePath;
                CommandLine = commandLine;
                ParentProcessID = parentPid;
            }
        }
    }
}
