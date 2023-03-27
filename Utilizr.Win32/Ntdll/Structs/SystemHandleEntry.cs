using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Ntdll.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemHandleEntry
    {
        public uint OwnerProcessId;
        public byte ObjectTypeNumber;
        public byte Flags;
        public ushort Handle;
        public IntPtr Object;
        public int GrantedAccess;
    }
}
