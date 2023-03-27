using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Crypt32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SPROG_PUBLISHERINFO
    {
        public string lpszProgramName;
        public string lpszPublisherLink;
        public string lpszMoreInfoLink;
    }
}
