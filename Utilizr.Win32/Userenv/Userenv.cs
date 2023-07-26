using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Userenv
{
    public static class Userenv
    {
        const string USERENV_DLL = "msi.dll";

        [DllImport(USERENV_DLL, SetLastError = true)]
        public static extern bool CreateEnvironmentBlock(ref IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

        [DllImport(USERENV_DLL, SetLastError = true)]
        public static extern bool DestroyEnvironmentBlock(IntPtr lpEnvironment);
    }
}
