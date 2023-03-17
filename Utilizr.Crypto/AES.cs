using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace Utilizr.Crypto
{
    /// <summary>
    /// Helper to decrypt AES-128 encrypted data
    /// </summary>
    public static class AES
    {
        public static string Decrypt(
            string cipherText,
            string passPhrase,
            byte[] salt,
            int iterations,
            byte[] iv)
        {
            return Decrypt(
                Encoding.UTF8.GetBytes(cipherText),
                Encoding.UTF8.GetBytes(passPhrase),
                salt,
                iterations,
                iv
            );
        }


        public static string Decrypt(
            byte[] cipherTextBytes,
            byte[] passPhrase,
            byte[] saltValueBytes,
            int passwordIterations,
            byte[] initVector,
            int keySize = 128)
        {

            if (cipherTextBytes == null || cipherTextBytes.Length == 0)
                throw new ArgumentException($"{nameof(cipherTextBytes)} must not be null or empty");

            var password = new Rfc2898DeriveBytes(passPhrase, saltValueBytes, passwordIterations);

            byte[] keyBytes = password.GetBytes(keySize / 8);

            var symmetricKey = new RijndaelManaged
            {
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.CBC,
                BlockSize = 128
            };

            ICryptoTransform decryptor = symmetricKey.CreateDecryptor
            (
                keyBytes,
                initVector
            );

            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = 0;
            using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
            {
                using var cryptoStream = new CryptoStream(
                    memoryStream,
                    decryptor,
                    CryptoStreamMode.Read
                );

                decryptedByteCount = cryptoStream.Read(
                    plainTextBytes,
                    0,
                    plainTextBytes.Length
                );
            }

            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }
    }
}
