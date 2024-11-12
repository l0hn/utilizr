using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Advapi32.Flags
{
    //https://learn.microsoft.com/en-us/windows/win32/secauthz/access-rights-for-access-token-objects

    [Flags]
    public enum TOKEN_PRIVILEGE_FLAGS : uint
    {
        TOKEN_ASSIGN_PRIMARY =                                  0x1,
        TOKEN_DUPLICATE =                                       0x2,
        TOKEN_IMPERSONATE =                                     0x4,
        TOKEN_QUERY =                                           0x8,
        TOKEN_QUERY_SOURCE =                                    0x10,
        TOKEN_ADJUST_PRIVILEGES =                               0x20,
        TOKEN_ADJUST_GROUPS =                                   0x40,
        TOKEN_ADJUST_DEFAULT =                                  0x80,
        TOKEN_ADJUST_SESSIONID =                                0x100,
        TOKEN_STANDARD_RIGHTS_READ =                            0x2000,
        TOKEN_STANDARD_RIGHTS_REQUIRED =                        0xF0000,
        TOKEN_READ = TOKEN_STANDARD_RIGHTS_REQUIRED 
            | TOKEN_QUERY,
        TOKEN_ALL_ACCESS = TOKEN_STANDARD_RIGHTS_REQUIRED
            | TOKEN_ASSIGN_PRIMARY
            | TOKEN_DUPLICATE
            | TOKEN_IMPERSONATE
            | TOKEN_QUERY
            | TOKEN_QUERY_SOURCE
            | TOKEN_ADJUST_PRIVILEGES
            | TOKEN_ADJUST_GROUPS
            | TOKEN_ADJUST_DEFAULT
            | TOKEN_ADJUST_SESSIONID
    }

    public enum ServiceStartupType : uint
    {
        /// <summary>
        /// A device driver started by the system loader. This value is valid only for driver services.
        /// </summary>
        BootStart = 0,

        /// <summary>
        /// A device driver started by the IoInitSystem function. This value is valid only for driver services.
        /// </summary>
        SystemStart = 1,

        /// <summary>
        /// A service started automatically by the service control manager during system startup.
        /// </summary>
        Automatic = 2,

        /// <summary>
        /// A service started by the service control manager when a process calls the StartService function.
        /// </summary>
        Manual = 3,

        /// <summary>
        /// A service that cannot be started. Attempts to start the service result in the error code ERROR_SERVICE_DISABLED.
        /// </summary>
        Disabled = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    public class QueryServiceConfig
    {
        [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
        public uint dwServiceType;
        [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
        public uint dwStartType;
        [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
        public uint dwErrorControl;
        [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
        public string? lpBinaryPathName;
        [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
        public string? lpLoadOrderGroup;
        [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
        public uint dwTagID;
        [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
        public string? lpDependencies;
        [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
        public string? lpServiceStartName;
        [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
        public string? lpDisplayName;
    };
}
