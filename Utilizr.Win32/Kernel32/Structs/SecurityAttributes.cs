using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Kernel32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
        public uint nLength;
        public IntPtr lpSecurityDescriptor;
        public bool bInheritHandle;
    }
}
