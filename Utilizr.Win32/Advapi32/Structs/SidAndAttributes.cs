using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Advapi32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SID_AND_ATTRIBUTES
    {
        public IntPtr Sid;
        public int Attributes;
    }
}
