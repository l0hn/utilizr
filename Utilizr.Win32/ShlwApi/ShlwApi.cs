using System;
using System.Runtime.InteropServices;
using System.Text;
using Utilizr.Win32.ShlwApi.Flags;

namespace Utilizr.Win32.ShlwApi
{
    public class ShlwApi
    {
        const string SHLWAPI_DLL = "shlwapi.dll";

        [DllImport(SHLWAPI_DLL)]
        public static extern int UrlCanonicalize(
            string pszUrl,
            StringBuilder pszCanonicalized,
            ref int pcchCanonicalized,
            Shlwapi_URL dwFlags);

        [DllImport(SHLWAPI_DLL, BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
        public static extern int SHLoadIndirectString(string pszSource, 
                            StringBuilder 
                            pszOutBuf, 
                            uint cchOutBuf, 
                            IntPtr ppvReserved);
    }
}
