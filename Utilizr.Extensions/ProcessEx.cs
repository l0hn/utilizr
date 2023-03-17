using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;

namespace Utilizr.Extensions
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
    }
}