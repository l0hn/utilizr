using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.WinTrust.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CRYPT_ALGORITHM_IDENTIFIER
    {
        /// LPSTR->CHAR*
        [MarshalAs(UnmanagedType.LPStr)]
        public string pszObjId;

        /// CRYPT_OBJID_BLOB->_CRYPTOAPI_BLOB
        public CRYPT_ATTR_BLOB Parameters;
    }
}
