using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Crypt32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CERT_ID
    {
        public int dwIdChoice;
        public BLOB IssuerSerialNumberOrKeyIdOrHashId;
    }
}
