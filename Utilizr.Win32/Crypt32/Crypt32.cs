using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Crypt32
{
    public static class Crypt32
    {
        const string CRYPT32_DLL = "crypt32.dll";

        [DllImport(CRYPT32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int CertGetNameString(
            IntPtr CertContext,
            uint lType,
            uint lFlags,
            [MarshalAs(UnmanagedType.LPWStr)] string? pTypeParameter,
            IntPtr str,
            int cch);


        [DllImport(CRYPT32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CryptQueryObject(
            int dwObjectType,
            IntPtr pvObject,
            int dwExpectedContentTypeFlags,
            int dwExpectedFormatTypeFlags,
            int dwFlags,
            out int pdwMsgAndCertEncodingType,
            out int pdwContentType,
            out int pdwFormatType,
            ref IntPtr phCertStore,
            ref IntPtr phMsg,
            ref IntPtr ppvContext);

        [DllImport(CRYPT32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CryptMsgGetParam(
            IntPtr hCryptMsg,
            int dwParamType,
            int dwIndex,
            IntPtr pvData,
            ref int pcbData
        );

        [DllImport(CRYPT32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CryptMsgClose(IntPtr hCryptMsg);

        [DllImport(CRYPT32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CertCloseStore(IntPtr hCertStore, int dwFlags);

        [DllImport(CRYPT32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CertFreeCertificateContext(IntPtr pCertContext);

        [DllImport(CRYPT32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CryptMsgGetParam(
            IntPtr hCryptMsg,
            int dwParamType,
            int dwIndex,
            [In, Out] byte[] vData,
            ref int pcbData
        );

        [DllImport(CRYPT32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CryptDecodeObject(
          uint CertEncodingType,
          UIntPtr lpszStructType,
          byte[] pbEncoded,
          uint cbEncoded,
          uint flags,
          [In, Out] byte[] pvStructInfo,
          ref uint cbStructInfo);

        public const int CRYPT_ASN_ENCODING = 0x00000001;
        public const int CRYPT_NDR_ENCODING = 0x00000002;
        public const int X509_ASN_ENCODING = 0x00000001;
        public const int X509_NDR_ENCODING = 0x00000002;
        public const int PKCS_7_ASN_ENCODING = 0x00010000;
        public const int PKCS_7_NDR_ENCODING = 0x00020000;

        public static UIntPtr PKCS7_SIGNER_INFO = new UIntPtr(500);
        public static UIntPtr CMS_SIGNER_INFO = new UIntPtr(501);

        public static string szOID_RSA_signingTime = "1.2.840.113549.1.9.5";
        public static string szOID_RSA_counterSign = "1.2.840.113549.1.9.6";

        public static string szOID_COMMON_NAME = "2.5.4.3";
        public static uint CERT_NAME_ATTR_TYPE = 3;
        public static string szOID_ORGANIZATIONAL_UNIT_NAME = "2.5.4.11";
        public static uint CERT_NAME_SIMPLE_DISPLAY_TYPE = 4;

    }
}
