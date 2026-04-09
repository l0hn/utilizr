using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Kernel32.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    public struct PROCESSOR_INFORMATION_UNION
    {
        [FieldOffset(0)]
        public PROCESSORCORE ProcessorCore;

        [FieldOffset(0)]
        public NUMANODE NumaNode;

        [FieldOffset(0)]
        public CACHE_DESCRIPTOR Cache;

        [FieldOffset(0)]
        private UInt64 Reserved1;

        [FieldOffset(8)]
        private UInt64 Reserved2;
    }
}
