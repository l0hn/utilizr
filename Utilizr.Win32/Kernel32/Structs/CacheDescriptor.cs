using System.Runtime.InteropServices;
using Utilizr.Win32.Kernel32.Flags;

namespace Utilizr.Win32.Kernel32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CACHE_DESCRIPTOR
    {
        public byte Level;
        public byte Associativity;
        public ushort LineSize;
        public uint Size;
        public PROCESSOR_CACHE_TYPE Type;
    }
}
