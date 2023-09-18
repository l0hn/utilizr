using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.User32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
