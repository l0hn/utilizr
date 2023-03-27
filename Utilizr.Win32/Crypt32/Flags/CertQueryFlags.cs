namespace Utilizr.Win32.Crypt32.Flags
{
    public static class CertQueryFlags
    {
        //-------------------------------------------------------------------------
        //dwObjectType for CryptQueryObject
        //-------------------------------------------------------------------------
        public const int CERT_QUERY_OBJECT_FILE = 0x00000001;

        public const int CERT_QUERY_OBJECT_BLOB = 0x00000002;

        //-------------------------------------------------------------------------
        //dwContentType for CryptQueryObject
        //-------------------------------------------------------------------------
        //encoded single certificate
        public const int CERT_QUERY_CONTENT_CERT = 1;

        //encoded single CTL
        public const int CERT_QUERY_CONTENT_CTL = 2;

        //encoded single CRL
        public const int CERT_QUERY_CONTENT_CRL = 3;

        //serialized store
        public const int CERT_QUERY_CONTENT_SERIALIZED_STORE = 4;

        //serialized single certificate
        public const int CERT_QUERY_CONTENT_SERIALIZED_CERT = 5;

        //serialized single CTL
        public const int CERT_QUERY_CONTENT_SERIALIZED_CTL = 6;

        //serialized single CRL
        public const int CERT_QUERY_CONTENT_SERIALIZED_CRL = 7;

        //a PKCS#7 signed message
        public const int CERT_QUERY_CONTENT_PKCS7_SIGNED = 8;

        //a PKCS#7 message, such as enveloped message.  But it is not a signed message,
        public const int CERT_QUERY_CONTENT_PKCS7_UNSIGNED = 9;

        //a PKCS7 signed message embedded in a file
        public const int CERT_QUERY_CONTENT_PKCS7_SIGNED_EMBED = 10;

        //an encoded PKCS#10
        public const int CERT_QUERY_CONTENT_PKCS10 = 11;

        //an encoded PKX BLOB
        public const int CERT_QUERY_CONTENT_PFX = 12;

        //an encoded CertificatePair (contains forward and/or reverse cross certs)
        public const int CERT_QUERY_CONTENT_CERT_PAIR = 13;

        //-------------------------------------------------------------------------
        //dwExpectedConentTypeFlags for CryptQueryObject
        //-------------------------------------------------------------------------
        //encoded single certificate
        public const int CERT_QUERY_CONTENT_FLAG_CERT = 1 << CERT_QUERY_CONTENT_CERT;

        //encoded single CTL
        public const int CERT_QUERY_CONTENT_FLAG_CTL = 1 << CERT_QUERY_CONTENT_CTL;

        //encoded single CRL
        public const int CERT_QUERY_CONTENT_FLAG_CRL = 1 << CERT_QUERY_CONTENT_CRL;

        //serialized store
        public const int CERT_QUERY_CONTENT_FLAG_SERIALIZED_STORE = 1 << CERT_QUERY_CONTENT_SERIALIZED_STORE;

        //serialized single certificate
        public const int CERT_QUERY_CONTENT_FLAG_SERIALIZED_CERT = 1 << CERT_QUERY_CONTENT_SERIALIZED_CERT;

        //serialized single CTL
        public const int CERT_QUERY_CONTENT_FLAG_SERIALIZED_CTL = 1 << CERT_QUERY_CONTENT_SERIALIZED_CTL;

        //serialized single CRL
        public const int CERT_QUERY_CONTENT_FLAG_SERIALIZED_CRL = 1 << CERT_QUERY_CONTENT_SERIALIZED_CRL;

        //an encoded PKCS#7 signed message
        public const int CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED = 1 << CERT_QUERY_CONTENT_PKCS7_SIGNED;

        //an encoded PKCS#7 message.  But it is not a signed message
        public const int CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED = 1 << CERT_QUERY_CONTENT_PKCS7_UNSIGNED;

        //the content includes an embedded PKCS7 signed message
        public const int CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED_EMBED = 1 << CERT_QUERY_CONTENT_PKCS7_SIGNED_EMBED;

        //an encoded PKCS#10
        public const int CERT_QUERY_CONTENT_FLAG_PKCS10 = 1 << CERT_QUERY_CONTENT_PKCS10;

        //an encoded PFX BLOB
        public const int CERT_QUERY_CONTENT_FLAG_PFX = 1 << CERT_QUERY_CONTENT_PFX;

        //an encoded CertificatePair (contains forward and/or reverse cross certs)
        public const int CERT_QUERY_CONTENT_FLAG_CERT_PAIR = 1 << CERT_QUERY_CONTENT_CERT_PAIR;

        //content can be any type
        public const int CERT_QUERY_CONTENT_FLAG_ALL =
            CERT_QUERY_CONTENT_FLAG_CERT |
            CERT_QUERY_CONTENT_FLAG_CTL |
            CERT_QUERY_CONTENT_FLAG_CRL |
            CERT_QUERY_CONTENT_FLAG_SERIALIZED_STORE |
            CERT_QUERY_CONTENT_FLAG_SERIALIZED_CERT |
            CERT_QUERY_CONTENT_FLAG_SERIALIZED_CTL |
            CERT_QUERY_CONTENT_FLAG_SERIALIZED_CRL |
            CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED |
            CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED |
            CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED_EMBED |
            CERT_QUERY_CONTENT_FLAG_PKCS10 |
            CERT_QUERY_CONTENT_FLAG_PFX |
            CERT_QUERY_CONTENT_FLAG_CERT_PAIR;

        //-------------------------------------------------------------------------
        //dwFormatType for CryptQueryObject
        //-------------------------------------------------------------------------
        //the content is in binary format
        public const int CERT_QUERY_FORMAT_BINARY = 1;

        //the content is base64 encoded
        public const int CERT_QUERY_FORMAT_BASE64_ENCODED = 2;

        //the content is ascii hex encoded with "{ASN}" prefix
        public const int CERT_QUERY_FORMAT_ASN_ASCII_HEX_ENCODED = 3;

        //-------------------------------------------------------------------------
        //dwExpectedFormatTypeFlags for CryptQueryObject
        //-------------------------------------------------------------------------
        //the content is in binary format
        public const int CERT_QUERY_FORMAT_FLAG_BINARY = 1 << CERT_QUERY_FORMAT_BINARY;

        //the content is base64 encoded
        public const int CERT_QUERY_FORMAT_FLAG_BASE64_ENCODED = 1 << CERT_QUERY_FORMAT_BASE64_ENCODED;

        //the content is ascii hex encoded with "{ASN}" prefix
        public const int CERT_QUERY_FORMAT_FLAG_ASN_ASCII_HEX_ENCODED = 1 << CERT_QUERY_FORMAT_ASN_ASCII_HEX_ENCODED;

        public const int CERT_CLOSE_STORE_CHECK_FLAG = 2;

        //the content can be of any format
        public const int CERT_QUERY_FORMAT_FLAG_ALL =
            CERT_QUERY_FORMAT_FLAG_BINARY |
            CERT_QUERY_FORMAT_FLAG_BASE64_ENCODED |
            CERT_QUERY_FORMAT_FLAG_ASN_ASCII_HEX_ENCODED;
    }
}