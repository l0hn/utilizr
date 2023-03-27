using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Crypt32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BLOB
    {
        public int cbData;
        public IntPtr pbData;
    }
}
