using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Crypt32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CMSG_SIGNER_INFO
    {
        public int dwVersion;
        private CERT_NAME_BLOB Issuer;
        private CRYPT_INTEGER_BLOB SerialNumber;
        private CRYPT_ALGORITHM_IDENTIFIER HashAlgorithm;
        private CRYPT_ALGORITHM_IDENTIFIER HashEncryptionAlgorithm;
        private BLOB EncryptedHash;
        private CRYPT_ATTRIBUTE[] AuthAttrs;
        private CRYPT_ATTRIBUTE[] UnauthAttrs;
    }
}
