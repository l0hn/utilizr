using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Advapi32.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TOKEN_PRIVILEGES
    {
        public int PrivilegeCount;
        public LUID_AND_ATTRIBUTES Privileges;
    }
}
