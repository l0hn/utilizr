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

        [DllImport(ADVAPI32_DLL, SetLastError = true)]
        public static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string? lpApplicationName,
            string lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string? lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation
        );

        [DllImport(ADVAPI32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenSCManager(string? machineName, string? databaseName, uint dwAccess);

        [DllImport(ADVAPI32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport(ADVAPI32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool ChangeServiceConfig(
            IntPtr hService,
            uint nServiceType,
            uint nStartType,
            uint nErrorControl,
            string? lpBinaryPathName,
            string? lpLoadOrderGroup,
            IntPtr lpdwTagId,
            [In] char[]? lpDependencies,
            string? lpServiceStartName,
            string? lpPassword,
            string? lpDisplayName);

        [DllImport(ADVAPI32_DLL, EntryPoint = "CloseServiceHandle")]
        public static extern int CloseServiceHandle(IntPtr hSCObject);

        [DllImport(ADVAPI32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool QueryServiceConfig(IntPtr hService, IntPtr intPtrQueryConfig, uint cbBufSize, out uint pcbBytesNeeded);
    }
}