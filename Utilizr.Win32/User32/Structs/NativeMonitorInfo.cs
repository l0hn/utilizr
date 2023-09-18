using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.User32.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public sealed class NativeMonitorInfo
    {
        public Int32 Size = Marshal.SizeOf(typeof(NativeMonitorInfo));
        public NativeRect Monitor;
        public NativeRect Work;
        public Int32 Flags;
    }
}
