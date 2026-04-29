using System;
using System.Runtime.InteropServices;
using Utilizr.Win32.Kernel32.Flags;

namespace Utilizr.Win32.Kernel32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEM_LOGICAL_PROCESSOR_INFORMATION
    {
        public IntPtr ProcessorMask;
        public LOGICAL_PROCESSOR_RELATIONSHIP Relationship;
        public PROCESSOR_INFORMATION_UNION ProcessorInformation;
    }
}
