using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Utilizr.Logging;
using Utilizr.Win32.Advapi32;
using Utilizr.Win32.Advapi32.Flags;
using Utilizr.Win32.Advapi32.Structs;
using Utilizr.Win32.Kernel32;
using Utilizr.Win32.Kernel32.Flags;
using Utilizr.Win32.Kernel32.Structs;

namespace Utilizr.Win.Security
{
    public static class Security
    {
        public static bool AdjustToken(bool Enable, string[] rights)
        {
            IntPtr hToken = IntPtr.Zero;
            IntPtr hProcess = IntPtr.Zero;
            var tLuid = new LUID();
            var newState = new TOKEN_PRIVILEGES();
            var uPriv = (uint)(TOKEN_PRIVILEGE_FLAGS.TOKEN_ADJUST_PRIVILEGES | TOKEN_PRIVILEGE_FLAGS.TOKEN_QUERY | TOKEN_PRIVILEGE_FLAGS.TOKEN_QUERY_SOURCE);

            try
            {
                hProcess = Kernel32.OpenProcess(ProcessAccessFlags.All, false, Kernel32.GetCurrentProcessId());
                if (hProcess == IntPtr.Zero)
                    return false;

                if (Advapi32.OpenProcessToken(hProcess, uPriv, ref hToken) == 0)
                    return false;

                for (int i = 0; i < rights.Length; i++)
                {
                    // Get the local unique id for the privilege.
                    if (!Advapi32.LookupPrivilegeValue(null, rights[i], ref tLuid))
                        return false;
                }

                // Assign values to the TOKEN_PRIVILEGE structure.
                newState.PrivilegeCount = 1;
                newState.Privileges.pLuid = tLuid;
                newState.Privileges.Attributes = (Enable ? SePrivileges.SE_PRIVILEGE_ENABLED : 0);
                // Adjust the token privilege
                //IntPtr pState = IntPtr.Zero;
                //Marshal.StructureToPtr(NewState, pState, true);
                return (Advapi32.AdjustTokenPrivileges(hToken, false, ref newState, (uint)Marshal.SizeOf(newState), IntPtr.Zero, IntPtr.Zero));
            }
            finally
            {
                if (hToken != IntPtr.Zero)
                    Kernel32.CloseHandle(hToken);

                if (hProcess != IntPtr.Zero)
                    Kernel32.CloseHandle(hProcess);
            }
        }

        public static bool IsAdmin()
        {
            IntPtr hToken = IntPtr.Zero;
            IntPtr hProcess = IntPtr.Zero;
            var tLuid = new LUID();
            var uPriv = (uint)(TOKEN_PRIVILEGE_FLAGS.TOKEN_ADJUST_PRIVILEGES | TOKEN_PRIVILEGE_FLAGS.TOKEN_QUERY | TOKEN_PRIVILEGE_FLAGS.TOKEN_QUERY_SOURCE);

            try
            {
                hProcess = Kernel32.OpenProcess(ProcessAccessFlags.All, false, Kernel32.GetCurrentProcessId());
                if (hProcess == IntPtr.Zero)
                    return false;

                if (Advapi32.OpenProcessToken(hProcess, uPriv, ref hToken) == 0)
                    return false;

                return (Advapi32.LookupPrivilegeValue(null, SePrivilegeNames.SE_TCB_NAME, ref tLuid));
            }
            finally
            {
                if (hToken != IntPtr.Zero)
                    Kernel32.CloseHandle(hToken);

                if (hProcess != IntPtr.Zero)
                    Kernel32.CloseHandle(hProcess);
            }
        }

        public static IntPtr DuplicateProcessToken()
        {
            return DuplicateProcessToken(Kernel32.GetCurrentProcessId());
        }

        public static IntPtr DuplicateProcessToken(uint pid)
        {
            try
            {
                IntPtr hProcess = IntPtr.Zero;
                IntPtr hToken = IntPtr.Zero;

                hProcess = Kernel32.OpenProcess(ProcessAccessFlags.All, false, pid);
                if (hProcess == IntPtr.Zero)
                    return IntPtr.Zero;

                //var uPriv = (uint)(TOKEN_PRIVILEGE_FLAGS.TOKEN_DUPLICATE
                //    | TOKEN_PRIVILEGE_FLAGS.TOKEN_IMPERSONATE
                //    | TOKEN_PRIVILEGE_FLAGS.TOKEN_QUERY
                //    | TOKEN_PRIVILEGE_FLAGS.TOKEN_QUERY_SOURCE);

                uint uPriv = (uint)TOKEN_PRIVILEGE_FLAGS.TOKEN_ALL_ACCESS;

                if (Advapi32.OpenProcessToken(hProcess, uPriv, ref hToken) == 0)
                    return IntPtr.Zero;

                return hToken;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }

            return IntPtr.Zero;
        }

        public static IntPtr GetPrimaryToken(int pid)
        {
            IntPtr token = IntPtr.Zero;
            IntPtr primaryToken = IntPtr.Zero;
            bool retVal = false;
            Process p = Process.GetProcessById(pid);

            //Gets impersonation token
            retVal = Advapi32.OpenProcessToken(p.Handle, (uint)TOKEN_PRIVILEGE_FLAGS.TOKEN_DUPLICATE, ref token) != 0;
            if (retVal)
            {
                var sa = new SECURITY_ATTRIBUTES();
                sa.nLength = (uint)Marshal.SizeOf(sa);

                uint desiredAccess = (uint)(TOKEN_PRIVILEGE_FLAGS.TOKEN_DUPLICATE
                    | TOKEN_PRIVILEGE_FLAGS.TOKEN_QUERY
                    | TOKEN_PRIVILEGE_FLAGS.TOKEN_ASSIGN_PRIMARY);

                //Convert the impersonation token into Primary token
                retVal = Advapi32.DuplicateTokenEx(
                    token,
                    desiredAccess,
                    ref sa,
                    (int)SECURITY_IMPERSONATION_LEVEL.SecurityIdentification, //works, but process not elevated locally
                                                                              //(int)SECURITY_IMPERSONATION_LEVEL.SecurityDelegation,
                    (int)TOKEN_TYPE.TokenPrimary,
                    ref primaryToken
                );

                //Close the Token that was previously opened.
                Kernel32.CloseHandle(token);

                if (retVal == false)
                    Log.Exception(new Win32Exception(Marshal.GetLastWin32Error()), "DuplicateTokenEx Error");
            }
            else
            {
                Log.Exception(new Win32Exception(Marshal.GetLastWin32Error()), "OpenProcessToken Error");
            }


            // Assign values to the TOKEN_PRIVILEGE structure.
            var tLuid = new LUID();
            var newState = new TOKEN_PRIVILEGES();

            if (!Advapi32.LookupPrivilegeValue(null, SePrivilegeNames.SE_RELABEL_NAME, ref tLuid))
            {
                Kernel32.CloseHandle(primaryToken);
                return IntPtr.Zero;
            }

            newState.PrivilegeCount = 1;
            newState.Privileges.pLuid = tLuid;
            newState.Privileges.Attributes = SePrivileges.SE_PRIVILEGE_ENABLED;

            if (!Advapi32.AdjustTokenPrivileges(primaryToken, false, ref newState, (uint)Marshal.SizeOf(newState), IntPtr.Zero, IntPtr.Zero))
            {
                Kernel32.CloseHandle(primaryToken);
                return IntPtr.Zero;
            }

            //We'll Close this token after it is used.
            return primaryToken;
        }
    }
}
