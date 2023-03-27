using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Crypt32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CRYPT_ATTR_BLOB
    {
        public uint cbData;

        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]
        public byte[] pbData;
    }
}
