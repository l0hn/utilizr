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

        public static byte[] Encrypt(
            string plainText,
            string key,
            out byte[] initVector,
            CipherMode cipherMode = CipherMode.CBC,
            int initVectorLength = 16,
            int blockSize = 128)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentException($"{nameof(plainText)} cannot be null or empty.");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"{nameof(key)} cannot be null or empty.");

            using (var aes = Aes.Create())
            {
                aes.Mode = cipherMode;
                aes.BlockSize = blockSize;
                aes.Key = Encoding.UTF8.GetBytes(key);

                var initVectorBytes = new Span<byte>(new byte[initVectorLength]);
                RandomNumberGenerator.Fill(initVectorBytes);
                initVector = initVectorBytes.ToArray();
                aes.IV = initVector;
                aes.Padding = PaddingMode.PKCS7;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memStream = new MemoryStream())
                using (CryptoStream cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write))
                {
                    var bytes = Encoding.UTF8.GetBytes(plainText);

                    cryptoStream.Write(bytes, 0, bytes.Length);
                    cryptoStream.FlushFinalBlock();

                    var cipherTextBytes = memStream.ToArray();
                    return cipherTextBytes;
                }
            }
        }
    }
}
