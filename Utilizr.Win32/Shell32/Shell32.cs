using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Shell32
{
    public static class Shell32
    {
        const string SHELL32_DLL = "shell32.dll";

        [DllImport(SHELL32_DLL)]
        public static extern int SHGetKnownFolderPath(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
            uint dwFlags,
            IntPtr hToken,
            out IntPtr ppszPath);
    }
}