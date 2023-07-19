using System.Runtime.InteropServices;

namespace Utilizr.Win32.Wininet
{
    public static class Wininet
    {
        const string WININET_DLL = "wininet.dll";

        [DllImport(WININET_DLL)]
        public static extern bool InternetGetConnectedState(out int flags, int reserved);
    }
}