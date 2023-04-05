using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using Utilizr.Win32.Crypt32;
using Utilizr.Win32.Crypt32.Flags;

namespace Utilizr.Win.Security
{
    public static class CertificateHelper
    {
        public static X509Certificate2? GetDigitalCertificate(string filename, out string CN)
        {
            CN = "";
            X509Certificate2? cert = null;

            int encodingType;
            int contentType;
            int formatType;
            IntPtr certStore = IntPtr.Zero;
            IntPtr cryptMsg = IntPtr.Zero;
            IntPtr context = IntPtr.Zero;
            IntPtr ptrFilename = Marshal.StringToHGlobalUni(filename);
            IntPtr commonName = IntPtr.Zero;

            try
            {
                if (!Crypt32.CryptQueryObject(
                    CertQueryFlags.CERT_QUERY_OBJECT_FILE,
                    ptrFilename,
                    (CertQueryFlags.CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED
                     | CertQueryFlags.CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED
                     | CertQueryFlags.CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED_EMBED
                    ), // <-- These are the attributes that makes it fast!!
                    CertQueryFlags.CERT_QUERY_FORMAT_FLAG_ALL,
                    0,
                    out encodingType,
                    out contentType,
                    out formatType,
                    ref certStore,
                    ref cryptMsg,
                    ref context))
                {
                    return null;
                }


                // Get size of the encoded message.
                int cbData = 0;
                if (!Crypt32.CryptMsgGetParam(
                    cryptMsg,
                    CmsgFlags.CMSG_ENCODED_MESSAGE,
                    0,
                    IntPtr.Zero,
                    ref cbData))
                {
                    return null;
                }

                var vData = new byte[cbData];

                // Get the encoded message.
                if (!Crypt32.CryptMsgGetParam(
                    cryptMsg,
                    CmsgFlags.CMSG_ENCODED_MESSAGE,
                    0,
                    vData,
                    ref cbData))
                {
                    return null;
                }

                var signedCms = new SignedCms();
                signedCms.Decode(vData);

                if (signedCms.SignerInfos.Count > 0)
                {
                    var signerInfo = signedCms.SignerInfos[0];
                    if (signerInfo.Certificate != null)
                    {
                        cert = signerInfo.Certificate;
                        commonName = Marshal.AllocHGlobal(255);
                        int len = Crypt32.CertGetNameString(
                            cert.Handle,
                            Crypt32.CERT_NAME_SIMPLE_DISPLAY_TYPE,
                            0,
                            null,
                            commonName,
                            255
                        );

                        CN = Marshal.PtrToStringAuto(commonName);
                    }
                }

                return cert;
            }
            finally
            {
                Marshal.FreeHGlobal(ptrFilename);
                if (cryptMsg != IntPtr.Zero)
                    Crypt32.CryptMsgClose(cryptMsg);

                if (certStore != IntPtr.Zero)
                    Crypt32.CertCloseStore(certStore, CertQueryFlags.CERT_CLOSE_STORE_CHECK_FLAG);

                if (context != IntPtr.Zero)
                    Crypt32.CertFreeCertificateContext(context);

                if (commonName != IntPtr.Zero)
                    Marshal.FreeHGlobal(commonName);
            }
        }
    }
}
