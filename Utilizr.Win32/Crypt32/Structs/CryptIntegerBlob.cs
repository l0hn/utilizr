using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Crypt32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CRYPT_INTEGER_BLOB
    {
        public uint cbData;
        public IntPtr pbData;
    }
}
