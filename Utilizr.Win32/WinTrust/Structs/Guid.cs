using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.WinTrust.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CRYPT_ATTR_BLOB
    {
        /// DWORD->unsigned int
        public uint cbData;

        /// BYTE*
        public IntPtr pbData;
    }
}
