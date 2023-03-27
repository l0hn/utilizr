using System.Runtime.InteropServices;

namespace Utilizr.Win32.User32
{
    public static class User32
    {
        const string USER32_DLL = "user32.dll";

        [DllImport(USER32_DLL)]
        public static extern short GetKeyState(int vKey);
    }
}