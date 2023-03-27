using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.WinTrust.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CRYPT_ATTRIBUTE_TYPE_VALUE
    {
        [MarshalAs(UnmanagedType.LPStr)] public string pszObjId;
        public CRYPT_ATTR_BLOB Value;
    }
}
