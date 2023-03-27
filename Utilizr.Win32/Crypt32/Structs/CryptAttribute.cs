using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Crypt32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CRYPT_ATTRIBUTE
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string pszObjId;

        public uint cValue;

        [MarshalAs(UnmanagedType.LPStruct)]
        public CRYPT_ATTR_BLOB rgValue;
    }
}
