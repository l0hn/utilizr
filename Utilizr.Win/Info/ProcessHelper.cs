﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using Utilizr.Logging;
using Utilizr.Win32.Advapi32;
using Utilizr.Win32.Advapi32.Flags;
using Utilizr.Win32.Advapi32.Structs;
using Utilizr.Win32.Kernel32;
using Utilizr.Win32.Kernel32.Flags;
using Utilizr.Win32.Kernel32.Structs;
using Utilizr.Win32.Ntdll;
using Utilizr.Win32.Userenv;

namespace Utilizr.Win.Info
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public static class ProcessHelper
    {
        const string LOG_CAT = "process-helper";
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

        public static void StopAllProcessesByName(string logCat, string processName, int? timeoutMs = null)
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

                    if (timeoutMs.HasValue)
                        process.WaitForExit((int)timeoutMs);
                    else process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Log.Exception(logCat, ex);
                }
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

        public static nint GetEnvironmentBlock(nint token)
        {
            IntPtr envBlock = IntPtr.Zero;
            Environment.SetEnvironmentVariable("__compat_layer", "RunAsInvoker");
            bool retVal = Userenv.CreateEnvironmentBlock(ref envBlock, token, true);
            if (retVal == false)
            {
                //Environment Block, things like common paths to My Documents etc.
                //Will not be created if "false"
                //It should not adversely affect CreateProcessAsUser.
                Log.Exception(new Win32Exception(Marshal.GetLastWin32Error()), $"{nameof(Userenv.CreateEnvironmentBlock)} Error");
            }
            return envBlock;
        }

        public static bool LaunchProcessAsUser(string cmdLine, IntPtr token, IntPtr envBlock, bool userInteractive, bool waitForExit, Action<int>? pidSetCallback = null)
        {
            return LaunchProcessAsUser(cmdLine, token, envBlock, userInteractive, waitForExit, out _, pidSetCallback);
        }

        public static bool LaunchProcessAsUser(string cmdLine, IntPtr token, IntPtr envBlock, bool userInteractive, bool waitForExit, out uint? exitCode, Action<int>? pidSetCallback = null)
        {
            bool result = false;
            exitCode = null;

            var pi = new PROCESS_INFORMATION();
            var saProcess = new Win32.Kernel32.Structs.SECURITY_ATTRIBUTES();
            var saThread = new Win32.Kernel32.Structs.SECURITY_ATTRIBUTES();
            saProcess.nLength = (uint)Marshal.SizeOf(saProcess);
            saThread.nLength = (uint)Marshal.SizeOf(saThread);

            STARTUPINFO si = new STARTUPINFO();

            si.cb = (uint)Marshal.SizeOf(si);

            //if this member is NULL, the new process inherits the desktop
            //and window station of its parent process. If this member is
            //an empty string, the process does not inherit the desktop and
            //window station of its parent process; instead, the system
            //determines if a new desktop and window station need to be created.
            //If the impersonated user already has a desktop, the system uses the
            //existing desktop.

            //si.lpDesktop = @"winsta0\default"; //Modify as needed
            si.lpDesktop = null;
            si.dwFlags = (uint)(STARTUPINFO_FLAGS.STARTF_USESHOWWINDOW | STARTUPINFO_FLAGS.STARTF_FORCEONFEEDBACK);
            si.wShowWindow = ShowWindowFlags.SW_SHOW;
            si.hStdError = IntPtr.Zero;
            si.hStdInput = IntPtr.Zero;
            si.hStdOutput = IntPtr.Zero;
            si.lpReserved2 = IntPtr.Zero;
            si.cbReserved2 = 0;
            si.lpTitle = null;

            //When INTERACTIVE, service run locally, and needs to use CreateProcess since service 
            //running under logged in user's context, not local system. Account will not have
            //SE_INCREASE_QUOTA_NAME, and will fail with ERROR_PRIVILEGE_NOT_HELD (1314)

            // Environment.UserInteractive always return true for .net core, expose so callee
            // can set explicitly: https://github.com/dotnet/runtime/issues/770

            uint suspendedFlag = waitForExit ? ProcessCreationFlags.CREATE_SUSPENDED : 0;

            if (userInteractive)
            {
                result = Kernel32.CreateProcess(
                    null,
                    cmdLine,
                    ref saProcess,
                    ref saThread,
                    false,
                    ProcessCreationFlags.CREATE_UNICODE_ENVIRONMENT | ProcessCreationFlags.CREATE_NEW_CONSOLE | suspendedFlag,
                    envBlock,
                    null,
                    ref si,
                    out pi
                );
            }
            else
            {
                result = Advapi32.CreateProcessAsUser(
                    token,
                    null,
                    cmdLine,
                    ref saProcess,
                    ref saThread,
                    false,
                    ProcessCreationFlags.CREATE_UNICODE_ENVIRONMENT | suspendedFlag,
                    envBlock,
                    null,
                    ref si,
                    out pi
                );
            }

            if (result == false)
            {
                int error = Marshal.GetLastWin32Error();
                Log.Exception(new Win32Exception(error), $"{nameof(Advapi32.CreateProcessAsUser)}");

                return result;
            }

            if (!waitForExit)
            {
                pidSetCallback?.Invoke((int)pi.dwProcessId);
                Kernel32.CloseHandle(pi.hProcess);
                return true;
            }

            var job = new WindowsJobObject();
            job.StartProcessAndWait(pi);

            pidSetCallback?.Invoke((int)pi.dwProcessId);

            uint ec = 0;
            for (var trys = 10; trys > 0; trys--)
            {
                Kernel32.GetExitCodeProcess(pi.hProcess, out ec);
                if (ec != 259)
                {
                    result = true;
                    break;
                }
                Thread.Sleep(1000);
            }

            Kernel32.CloseHandle(pi.hProcess);

            return result;
        }

        static bool IsUnsafeWaitProcess(string processName)
        {
            // Don't wait on Windows Explorer.
            // Some uninstallers might fire feedback, etc, link the browser.
            // Don't wait on the browsers
            // Zoom also has an issue with ProcessTrace, causing it to fire until resources are consumed
            // One of Zooms components [cptinstall] looks to be the perp

            string lowerName = processName.ToLowerInvariant();
            bool isUnsafe = lowerName.Contains("explorer") ||
                lowerName.Contains("iexplore") ||
                lowerName.Contains("chrome") ||
                lowerName.Contains("firefox") ||
                lowerName.Contains("opera") ||
                lowerName.Contains("microsoftedge") ||
                lowerName.Contains("cptinstall") ||
                lowerName.Contains("zoom") ||
                lowerName.Contains("msedge"); // chromium based edge

            return isUnsafe;
        }

        public static int GetParentProcessId(int pid) {

            IntPtr pHandle = Kernel32.OpenProcess(ProcessAccessFlags.QueryInformation, false, (uint)pid);

            try
            {
                PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
                int returnLength = 0;

                int result = Ntdll.NtQueryInformationProcess(pHandle, 0, ref pbi, Marshal.SizeOf(pbi), ref returnLength);   
                
                if (result != 0)
                {
                    throw new InvalidOperationException($"NtQueryInformationProcess failed with status code: {result}");
                }

                return pbi.InheritedFromUniqueProcessId.ToInt32();
            }
            catch (System.Exception ex)
            {
                Log.Exception(ex);   
                throw;
            } finally {
                if (pHandle != IntPtr.Zero) {
                    try
                    {
                        Kernel32.CloseHandle(pHandle);
                    }
                    catch (System.Exception)
                    {
                    
                    }
                }
            }
        }

        public static bool TryGetParentProcessId(int pid, out int parentPid) {
            try
            {
                parentPid = GetParentProcessId(pid);
                return true;
            }
            catch (System.Exception)
            {
                
            }
            parentPid = 0;
            return false;
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
