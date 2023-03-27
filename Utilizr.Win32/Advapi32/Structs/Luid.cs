using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Advapi32.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LUID
    {
        public int LowPart;
        public int HighPart;
    }
}
