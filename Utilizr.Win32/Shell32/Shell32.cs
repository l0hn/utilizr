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


        [DllImport(SHELL32_DLL, SetLastError = true)]
        public static extern int SHOpenFolderAndSelectItems(
            IntPtr pidlFolder,
            uint cidl,
            [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl,
            uint dwFlags);


        [DllImport(SHELL32_DLL, SetLastError = true)]
        public static extern void SHParseDisplayName(
            [MarshalAs(UnmanagedType.LPWStr)] string name,
            IntPtr bindingContext,
            [Out] out IntPtr pidl,
            uint sfgaoIn,
            [Out] out uint psfgaoOut);
    }
}