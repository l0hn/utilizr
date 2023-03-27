using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.WinTrust.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CRYPTCATSTORE
    {
        private uint _cbStruct;
        public uint dwPublicVersion;
        [MarshalAs(UnmanagedType.LPWStr)] public string pwszP7File;
        private IntPtr _hProv;
        private uint _dwEncodingType;
        private uint _fdwStoreFlags;
        private IntPtr _hReserved;
        private IntPtr _hAttrs;
        private IntPtr _hCryptMsg;
        private IntPtr _hSorted;
    };
}
