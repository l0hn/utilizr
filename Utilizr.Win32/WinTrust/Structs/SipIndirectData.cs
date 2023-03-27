using System;
using System.Runtime.InteropServices;
using static Utilizr.Win32.WinTrust.WinTrust;

namespace Utilizr.Win32.WinTrust.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SIP_INDIRECT_DATA
    {
        public CRYPT_ATTRIBUTE_TYPE_VALUE Data;
        public CRYPT_ALGORITHM_IDENTIFIER DigestAlgorithm;
        public CRYPT_ATTR_BLOB Digest;
    }
}
