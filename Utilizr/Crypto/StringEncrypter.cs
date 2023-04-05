using System;
using System.Security.Cryptography;
using System.Text;

namespace Utilizr.Crypto
{
    public static class StringEncrypter
    {
        private static readonly Encoding _encoding = Encoding.UTF8;

        //http://www.dijksterhuis.org/encrypting-decrypting-string/
        /// <summary>
        /// Encrypt a byte[] with a passphrase using TDES
        /// </summary>
        /// <param name="message"></param>
        /// <param name="passphrase"></param>
        /// <returns>Encrypted message as byte[]</returns>
        public static byte[] EncryptTDES(byte[] message, string passphrase)
        {
            byte[] results;

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below


            var hashProvider = new MD5CryptoServiceProvider();
            byte[] tdesKey = hashProvider.ComputeHash(_encoding.GetBytes(passphrase));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            var tdesAlgorithm = new TripleDESCryptoServiceProvider
            {
                // Step 3. Setup the encoder
                Key = tdesKey,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.Zeros
            };
            tdesAlgorithm.GenerateIV();

            // Step 4. Attempt to encrypt the string
            try
            {
                ICryptoTransform Encryptor = tdesAlgorithm.CreateEncryptor();
                results = Encryptor.TransformFinalBlock(message, 0, message.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                tdesAlgorithm.Clear();
                hashProvider.Clear();
            }

            // Step 5. Return the encrypted string as a base64 encoded string
            return results;
        }

        /// <summary>
        /// Decrypt a byte array with a passphrase
        /// </summary>
        /// <param name="message"></param>
        /// <param name="passphrase"></param>
        /// <returns>decrypted bytes</returns>
        public static byte[] DecryptTDES(byte[] message, string passphrase)
        {
            byte[] results;

            int rem = message.Length % 8;
            int padding = 0;
            if (rem > 0)
            {
                padding = 8 - rem;
            }
            byte[] paddedMessage = new byte[message.Length + padding];
            paddedMessage.Initialize();
            Array.Copy(message, paddedMessage, message.Length);
            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            byte[] tdesKey;
            MD5CryptoServiceProvider hashProvider;

            hashProvider = new MD5CryptoServiceProvider();
            tdesKey = hashProvider.ComputeHash(_encoding.GetBytes(passphrase));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            var tdesAlgorithm = new TripleDESCryptoServiceProvider
            {
                // Step 3. Setup the decoder
                Key = tdesKey,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.Zeros
            };
            tdesAlgorithm.GenerateIV();

            // Step 4. Attempt to decrypt the string
            results = new byte[0];
            try
            {
                ICryptoTransform Decryptor = tdesAlgorithm.CreateDecryptor();
                results = Decryptor.TransformFinalBlock(paddedMessage, 0, paddedMessage.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                tdesAlgorithm.Clear();
                hashProvider.Clear();
            }

            // Step 5. Return the decrypted string in UTF8 format
            return results;
        }

        /// <summary>
        /// Encrypt a string with a passphrase using TDES
        /// </summary>
        /// <param name="message"></param>
        /// <param name="passphrase"></param>
        /// <returns>Encrypted string</returns>
        public static string EncryptString(string message, string passphrase)
        {
            if (string.IsNullOrEmpty(message))
                return message;

            return Convert.ToBase64String(EncryptTDES(_encoding.GetBytes(message), passphrase));
        }


        /// <summary>
        /// Decrypt a string with a passphrase using TDES
        /// </summary>
        /// <param name="encryptedMessage"></param>
        /// <param name="passphrase"></param>
        /// <returns>decrypted string</returns>
        public static string DecryptString(string encryptedMessage, string passphrase)
        {
            if (string.IsNullOrEmpty(encryptedMessage))
                return encryptedMessage;

            return _encoding.GetString(DecryptTDES(Convert.FromBase64String(encryptedMessage), passphrase)).TrimEnd('\0');
        }
    }
}
