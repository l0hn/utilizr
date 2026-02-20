using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using Utilizr.Logging;
using Utilizr.Win.Info;
using Utilizr.Win32.Kernel32;
using Utilizr.Win32.Kernel32.Flags;

namespace Utilizr.Win.Extensions
{
    public static class ProcessEx
    {
        ///// <summary>
        ///// Start the process as a user
        ///// </summary>
        ///// <param name="process">The process to start</param>
        ///// <param name="pid">The PID of a running process that is running under the user you wish to impersonate</param>
        ///// <returns>True if process is started, otherwise false</returns>
        //public static bool StartAsUser(this Process process, uint pid)
        //{
        //    IntPtr userToken = DuplicateProcessToken(pid);

        //    if (userToken == IntPtr.Zero)
        //        throw new InvalidOperationException($"Failed to get user's token from PID {pid}");

        //    return StartAsUser(process, userToken);
        //}

        //static IntPtr DuplicateProcessToken(uint pid)
        //{
        //    IntPtr hProcess = Win32.OpenProcess(Win32.ProcessAccessFlags.All, 0, pid);
        //    uint desiredAccess = (uint)(Win32.ETOKEN_PRIVILEGES.TOKEN_ALL_ACCESS);
        //    IntPtr token = IntPtr.Zero;
        //    int result = Win32.OpenProcessToken(hProcess, desiredAccess, ref token);
        //    return token;
        //}

        ///// <summary>
        ///// Start the process as a user
        ///// </summary>
        ///// <param name="process">The process to start</param>
        ///// <param name="pid">The token of a user you wish to impersonate</param>
        ///// <returns>True if process is started, otherwise false</returns>
        //public static bool StartAsUser(this Process process, IntPtr userToken)
        //{
        //    using (WindowsImpersonationContext impersonatedUser = WindowsIdentity.Impersonate(userToken))
        //    {
        //        return process.Start();
        //    }
        //}


        public static IEnumerable<Process> GetChildProcesses(this Process process)
        {
            return GetChildProcesses((uint)process.Id);
        }


        public static IEnumerable<Process> GetChildProcesses(uint pid)
        {
            var query = $"Select * From Win32_Process Where ParentProcessID={pid}";
            using var mos = new ManagementObjectSearcher(query);
            foreach (var mo in mos.Get())
            {
                Process? resultProcess = null;
                try
                {
                    resultProcess = Process.GetProcessById(Convert.ToInt32(mo["ProcessID"]));
                }
                catch (ArgumentException)
                {
                    // Process exited by now
                }

                if (resultProcess != null)
                    yield return resultProcess;
            }
        }


        public static void Kill(this Process process, bool killChildren)
        {
            if (killChildren)
            {
                foreach (var childProcess in process.GetChildProcesses())
                {
                    try
                    {
                        childProcess.Kill();
                    }
                    catch
                    {
                        // Ignore failures, the process probably ended
                    }
                }
            }
            process.Kill();
        }

        /// <summary>
        /// Wait for the process with the given PID to exit.
        /// </summary>
        /// <param name="pid"></param>
        /// <returns>The exit code of the terminated process.</returns>
        /// <exception cref="Win32Exception"></exception>
        public static int SafeWaitForExit(this Process p)
        {
            return SafeWaitForExit((uint)p.Id);
        }

        /// <summary>
        /// Wait for the process with the given PID to exit.
        /// </summary>
        /// <param name="pid"></param>
        /// <returns>The exit code of the terminated process.</returns>
        /// <exception cref="Win32Exception"></exception>
        public static int SafeWaitForExit(uint pid)
        {
            var flags = ProcessAccessFlags.QueryLimitedInformation | ProcessAccessFlags.Synchronize;

            var hProcess = Kernel32.OpenProcess(flags, false, pid);
            if (hProcess == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                if (Kernel32.WaitForSingleObject(hProcess, Kernel32.WAIT_FOR_OBJECT_INFINITE) != 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                if (!Kernel32.GetExitCodeProcess(hProcess, out uint exitCode))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                return unchecked((int)exitCode); // Win32 stores as unsigned DWORD, match .NET signed
            }
            finally
            {
                Kernel32.CloseHandle(hProcess);
            }
        }

        /// <summary>
        /// Get the filepath of the process, without an access denied error.
        /// </summary>
        public static string SafeGetProcessFilename(this Process p)
        {
            int capacity = 2000;
            var builder = new StringBuilder(capacity);
            var hProcess = Kernel32.OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, (uint)p.Id);

            if (hProcess == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            bool success = false;
            try
            {
                success = Kernel32.QueryFullProcessImageName(hProcess, 0, builder, ref capacity);
            }
            finally
            {
                Kernel32.CloseHandle(hProcess);
            }

            return success
                ? builder.ToString()
                : string.Empty;
        }

        public static void ResumeProcess(this Process p)
        {
            foreach (ProcessThread processThread in p.Threads)
            {
                var pThread = Kernel32.OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)processThread.Id);
                if (pThread == IntPtr.Zero)
                {
                    Log.Info(nameof(ProcessEx), "Resuming process thread");
                    continue;
                }

                Kernel32.ResumeThread(pThread);
                Kernel32.CloseHandle(pThread);
            }
        }

        public static Process GetParentProcess(this Process p) {
            var parentId = ProcessHelper.GetParentProcessId(p.Id);
            return Process.GetProcessById(parentId);
        }

        public static bool TryGetParentProcess(this Process p, out Process? parentProcess) {
            try
            {
                parentProcess = GetParentProcess(p);
                return true;
            }
            catch (System.Exception)
            {
            
            }
            parentProcess = null;
            return false;
        }
        
    }
}