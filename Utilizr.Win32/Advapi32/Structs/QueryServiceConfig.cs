using System.Runtime.InteropServices;

namespace Utilizr.Win32.Advapi32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public class QueryServiceConfig
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint dwServiceType;
        [MarshalAs(UnmanagedType.U4)]
        public uint dwStartType;
        [MarshalAs(UnmanagedType.U4)]
        public uint dwErrorControl;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string? lpBinaryPathName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string? lpLoadOrderGroup;
        [MarshalAs(UnmanagedType.U4)]
        public uint dwTagID;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string? lpDependencies;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string? lpServiceStartName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string? lpDisplayName;
    };
}
