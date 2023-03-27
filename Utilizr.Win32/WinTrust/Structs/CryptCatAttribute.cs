using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.WinTrust.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CRYPTCATATTRIBUTE
    {
        private uint _cbStruct;
        [MarshalAs(UnmanagedType.LPWStr)] public string pwszReferenceTag;
        private uint _dwAttrTypeAndAction;
        public uint cbValue;
        public IntPtr pbValue;
        private uint _dwReserved;
    };
}
