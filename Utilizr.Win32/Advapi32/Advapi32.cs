using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Utilizr.Win32.Advapi32.Flags;
using Utilizr.Win32.Advapi32.Structs;
using Utilizr.Win32.Kernel32.Structs;

namespace Utilizr.Win32.Advapi32
{
    public static class Advapi32
    {
        public const Int32 TOKEN_QUERY = 0x00000008;
        [StructLayout(LayoutKind.Sequential)]
        public struct TOKEN_USER
        {
            public SID_AND_ATTRIBUTES User;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SID_AND_ATTRIBUTES
        {
            public IntPtr Sid;
            public int Attributes;
        }

        const string ADVAPI32_DLL = "advapi32.dll";

        [DllImport(ADVAPI32_DLL, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AdjustTokenPrivileges(
            IntPtr TokenHandle,
            [MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState,
            uint BufferLength,
            IntPtr PreviousState,
            IntPtr ReturnLength);


        [DllImport(ADVAPI32_DLL, SetLastError = true)]
        public static extern int OpenProcessToken(
            IntPtr ProcessHandle,
            uint DesiredAccess,
            ref IntPtr TokenHandle);


        [DllImport(ADVAPI32_DLL, CharSet = CharSet.Auto)]
        public static extern bool GetTokenInformation(
            IntPtr hToken,
            TOKEN_INFORMATION_CLASS tokenInfoClass,
            IntPtr TokenInformation,
            int tokeInfoLength,
            ref int reqLength);


        [DllImport(ADVAPI32_DLL, CharSet = CharSet.Auto)]
        public static extern bool ConvertSidToStringSid(
            IntPtr pSID,
            [In, Out, MarshalAs(UnmanagedType.LPTStr)] ref string pStringSid);


        [DllImport(ADVAPI32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool LookupPrivilegeValue(
            string? lpSystemName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpName,
            ref LUID lpLuid);

        [DllImport(ADVAPI32_DLL, EntryPoint = "DuplicateTokenEx", SetLastError = true)]
        public static extern bool DuplicateTokenEx(
            IntPtr hExistingToken,
            uint dwDesiredAccess,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            int ImpersonationLevel,
            int dwTokenType,
            ref IntPtr phNewToken
        );

        [DllImport(ADVAPI32_DLL, SetLastError = true)]
        public static extern bool SetServiceStatus(
            IntPtr handle,
            ref ServiceStatus serviceStatus
        );

        public static bool ProcessOwnedByUser(int pid, string userSID)
        {
            IntPtr pToken = IntPtr.Zero;
            var process = Process.GetProcessById(pid);

            if (OpenProcessToken(process.Handle, TOKEN_QUERY, ref pToken) != 0)
            {
                IntPtr pSidPtr = IntPtr.Zero;
                if (ProcessTokenToSID(pToken, out pSidPtr))
                {
                    string pSidStr = string.Empty;
                    ConvertSidToStringSid(pSidPtr, ref pSidStr);
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
                result = GetTokenInformation(token, TOKEN_INFORMATION_CLASS.TokenUser, tu, cb, ref cb);
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
    }
}