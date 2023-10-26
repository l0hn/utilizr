using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Utilizr.Logging;
using Utilizr.Win.Extensions;
using Utilizr.Win32.Advapi32;
using Utilizr.Win32.Advapi32.Flags;
using Utilizr.Win32.Advapi32.Structs;
using Utilizr.Win32.Kernel32;
using Utilizr.Win32.Kernel32.Flags;
using Utilizr.Win32.Kernel32.Structs;
using Utilizr.Win32.Userenv;

namespace Utilizr.Win.Info
{
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

        public static bool LaunchProcessAsUser(string cmdLine, IntPtr token, IntPtr envBlock, bool userInteractive, bool waitForExit)
        {
            return LaunchProcessAsUser(cmdLine, token, envBlock, userInteractive, waitForExit, out _);
        }

        public static bool LaunchProcessAsUser(string cmdLine, IntPtr token, IntPtr envBlock, bool userInteractive, bool waitForExit, out uint? exitCode)
        {
            bool result = false;
            exitCode = null;

            var pi = new PROCESS_INFORMATION();
            var saProcess = new SECURITY_ATTRIBUTES();
            var saThread = new SECURITY_ATTRIBUTES();
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

            if (userInteractive)
            {
                result = Kernel32.CreateProcess(
                    null,
                    cmdLine,
                    ref saProcess,
                    ref saThread,
                    false,
                    ProcessCreationFlags.CREATE_UNICODE_ENVIRONMENT | ProcessCreationFlags.CREATE_NEW_CONSOLE,
                    envBlock,
                    null,
                    ref si,
                    out pi
                );
            }
            else
            {
                Log.Info(LOG_CAT, "START");
                result = Advapi32.CreateProcessAsUser(
                    token,
                    null,
                    cmdLine,
                    ref saProcess,
                    ref saThread,
                    false,
                    ProcessCreationFlags.CREATE_UNICODE_ENVIRONMENT,
                    envBlock,
                    null,
                    ref si,
                    out pi
                );
                Log.Info(LOG_CAT, "END");
            }

            if (result == false)
            {
                int error = Marshal.GetLastWin32Error();
                Log.Exception(new Win32Exception(error), $"{nameof(Advapi32.CreateProcessAsUser)}");


                //Log.Info(LOG_CAT, "FALSE");
                //Log.Info(LOG_CAT, "BBB1: {0}", token.ToString());
                //Log.Info(LOG_CAT, "BBB2: {0}", cmdLine);

                //Log.Info(LOG_CAT, "saProcess: {0}", saProcess.nLength.ToString());
                //Log.Info(LOG_CAT, "saProcess: {0}", saProcess.lpSecurityDescriptor.ToString());
                //Log.Info(LOG_CAT, "saProcess: {0}", saProcess.bInheritHandle.ToString());

                //Log.Info(LOG_CAT, "saThread: {0}", saThread.nLength.ToString());
                //Log.Info(LOG_CAT, "saThread: {0}", saThread.lpSecurityDescriptor.ToString());
                //Log.Info(LOG_CAT, "saThread: {0}", saThread.bInheritHandle.ToString());

                //Log.Info(LOG_CAT, "BBB3: {0}", envBlock.ToString());

                //Log.Info(LOG_CAT, "PI: {0}", pi.hProcess.ToString());
                //Log.Info(LOG_CAT, "PI: {0}", pi.hThread.ToString());
                //Log.Info(LOG_CAT, "PI: {0}", pi.dwProcessId.ToString());
                //Log.Info(LOG_CAT, "PI: {0}", pi.dwThreadId.ToString());
                return result;
            }

            Log.Info(LOG_CAT, "TRUE");
            Log.Info(LOG_CAT, "BBB1: {0}", token.ToString());
            Log.Info(LOG_CAT, "BBB2: {0}", cmdLine);

            Log.Info(LOG_CAT, "saProcess: {0}", saProcess.nLength.ToString());
            Log.Info(LOG_CAT, "saProcess: {0}", saProcess.lpSecurityDescriptor.ToString());
            Log.Info(LOG_CAT, "saProcess: {0}", saProcess.bInheritHandle.ToString());

            Log.Info(LOG_CAT, "saThread: {0}", saThread.nLength.ToString());
            Log.Info(LOG_CAT, "saThread: {0}", saThread.lpSecurityDescriptor.ToString());
            Log.Info(LOG_CAT, "saThread: {0}", saThread.bInheritHandle.ToString());

            Log.Info(LOG_CAT, "BBB3: {0}", envBlock.ToString());

            Log.Info(LOG_CAT, "PI: {0}", pi.hProcess.ToString());
            Log.Info(LOG_CAT, "PI: {0}", pi.hThread.ToString());
            Log.Info(LOG_CAT, "PI: {0}", pi.dwProcessId.ToString());
            Log.Info(LOG_CAT, "PI: {0}", pi.dwThreadId.ToString());

            if (!waitForExit)
            {
                return true;
            }

            Kernel32.WaitForSingleObject(pi.hProcess, Kernel32.WAIT_FOR_OBJECT_INFINITE);

            result = Kernel32.GetExitCodeProcess(pi.hProcess, out uint ec);
            Kernel32.CloseHandle(pi.hProcess);
            exitCode = ec;

            if (exitCode != 0)
            {
                Log.Exception(new Exception($"Started {cmdLine} but exited with {exitCode}"));
                return false;
            }

            //----
            var children = ProcessEx.GetChildProcesses(pi.dwProcessId).ToList();
            Log.Info(LOG_CAT, "Children: {0}", children.Count.ToString());
            result = WaitOnChildren(children, cmdLine, recursiveWait: true) && result;
            //----

            return result;
        }

        public static bool WaitOnChildren(List<Process> children, string parentExe, bool recursiveWait = false)
        {
            bool success = true;
            //string logExeArgsInfo = string.IsNullOrEmpty(parentArgs)
            //    ? parentExe
            //    : $"{parentExe} {parentArgs}";

            string logExeArgsInfo = parentExe;

            //Log.Info(LogCat, "'{0}' started {1:N0} child process(es)", logExeArgsInfo, children.Count);

            var idLookup = new Dictionary<int, string>();
            void exitedHandler(object s, EventArgs e)
            {
                // Cannot just get the ExitCode from the process, since the childProcess
                // object didn't start it. This is a hacky work around...

                if (!(s is Process process))
                    return;

                idLookup.TryGetValue(process.Id, out string executablePath);

                success = success && process.ExitCode == 0;
                //Log.Info(
                //    LogCat,
                //    "{0} process '{1}' returned {2} from parent '{3}'",
                //    nameof(WaitOnChildren),
                //    executablePath,
                //    process.ExitCode,
                //    logExeArgsInfo
                //);
            }

            foreach (var childProcess in children)
            {
                var loopLocal = childProcess;
                if (loopLocal.HasExited)
                    continue;

                if (IsUnsafeWaitProcess(loopLocal.ProcessName))
                    continue;

                try
                {
                    var wmiInfo = ProcessHelper.GetRunningProcess(loopLocal.Id);
                    idLookup[loopLocal.Id] = wmiInfo.ExecutablePath;

                    //Log.Info(
                    //    LogCat,
                    //    "Waiting on child process '{0}' from '{1}'",
                    //    wmiInfo.ExecutablePath,
                    //    logExeArgsInfo
                    //);

                    loopLocal.EnableRaisingEvents = true;
                    loopLocal.Exited += exitedHandler;
                    var grandChildren = loopLocal.GetChildProcesses().ToList();
                    success = WaitOnChildren(grandChildren, wmiInfo.ExecutablePath, true) && success;
                    loopLocal.WaitForExit();
                }
                catch (Exception ex)
                {
                    //Log.Exception(LogCat, ex);
                }
                finally
                {
                    loopLocal.Exited -= exitedHandler;
                }
            }

            return success;
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
