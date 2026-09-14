using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Utilizr.Util;

namespace Utilizr.Crypto
{
    public static class StringEncrypter
    {
        private static readonly Encoding _encoding = Encoding.UTF8;
        private static readonly int _aesGcmNonceSize = 12; // Recommended GCM nonce size
        private static readonly int _aeaGcmTagSize = 16; // 128-bit authentication tag

        //http://www.dijksterhuis.org/encrypting-decrypting-string/
        /// <summary>
        /// Encrypt a byte[] with a passphrase using TDES
        /// </summary>
        /// <param name="message"></param>
        /// <param name="passphrase"></param>
        /// <returns>Encrypted message as byte[]</returns>
        [Obsolete("This is using obsolete .NET APIs, use the AesGcm alternative")]
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
        [Obsolete("This is using obsolete .NET APIs, use the AesGcm alternative")]
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
        [Obsolete("This is using obsolete .NET APIs, use the SecureString version using the AesGcm implementation")]
        public static string EncryptString(string message, string passphrase)
        {
            if (string.IsNullOrEmpty(message))
                return message;

            return Convert.ToBase64String(EncryptTDES(_encoding.GetBytes(message), passphrase));
        }


        /// <summary>
        /// Decrypt a string with a passphrase using TDES.
        /// </summary>
        /// <param name="encryptedMessage"></param>
        /// <param name="passphrase"></param>
        /// <returns>decrypted string</returns>
        [Obsolete("This is using obsolete .NET APIs, use the SecureString version using the AesGcm implementation")]
        public static string DecryptString(string encryptedMessage, string passphrase)
        {
            if (string.IsNullOrEmpty(encryptedMessage))
                return encryptedMessage;

            return _encoding.GetString(DecryptTDES(Convert.FromBase64String(encryptedMessage), passphrase)).TrimEnd('\0');
        }






        /// <summary>
        /// Encrypt a SecureString with AEC-GCM with modern .NET APIs that are not depreciated.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="passphrase"></param>
        /// <returns>Encrypted message as string</returns>
        public static string EncryptString(SecureString? message, SecureString passphrase, Action<string>? msgBoxCallback = null)
        {
            if (message == null || message.Length < 1)
                return string.Empty;

            using var pinnedMessage = new PinnedString(message);
            var messageBytes = pinnedMessage.ReadBytes();
            if (messageBytes.Length < 1)
                return string.Empty;

            using var pinnedPhrase = new PinnedString(passphrase);
            var phraseBytes = pinnedPhrase.ReadBytes();
            if (phraseBytes.Length < 1)
                return string.Empty;

            msgBoxCallback?.Invoke("message + passphrase in memory (x2)");

            return Convert.ToBase64String(EncryptAesGcm(messageBytes, phraseBytes));
        }

        /// <summary>
        /// Decrypt a string with passphrase using AES-GCM.
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="passphrase"></param>
        /// <returns>Decrypted SecureString</returns>
        public static SecureString? DecryptString(string cipherText, SecureString passphrase)
        {
            var cipherBytes = Convert.FromBase64String(cipherText);
            var cipherSpanBytes = new ReadOnlySpan<byte>(cipherBytes);
            if (cipherSpanBytes.Length < 1)
                return null;

            using var pinnedPhrase = new PinnedString(passphrase);
            var phraseBytes = pinnedPhrase.ReadBytes();
            if (phraseBytes.Length < 1)
                return null;

            return DecryptAesGcm(cipherSpanBytes, phraseBytes);
        }

        /// <summary>
        /// Encrypt a byte span with AES-GCM with modern .NET APIs that are not depreciated.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="passphrase"></param>
        /// <returns>Encrypted message as byte array.</returns>
        public static byte[] EncryptAesGcm(ReadOnlySpan<byte> message, ReadOnlySpan<byte> passphrase)
        {
            var result = new byte[_aesGcmNonceSize + message.Length + _aeaGcmTagSize];

            Span<byte> nonce = result.AsSpan(0, _aesGcmNonceSize);
            Span<byte> ciphertext = result.AsSpan(_aesGcmNonceSize, message.Length);
            Span<byte> tag = result.AsSpan(_aesGcmNonceSize + message.Length, _aeaGcmTagSize);

            RandomNumberGenerator.Fill(nonce);

            var key = SHA256.HashData(passphrase);

            using var aes = new AesGcm(key, _aeaGcmTagSize);
            aes.Encrypt(nonce, message, ciphertext, tag);

            return result;
        }

        /// <summary>
        /// Decrypt a byte span with passphrase using AES-GCM.
        /// </summary>
        /// <param name="encrypted"></param>
        /// <param name="passphrase"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static SecureString DecryptAesGcm(ReadOnlySpan<byte> encrypted, ReadOnlySpan<byte> passphrase)
        {
            var key = SHA256.HashData(passphrase);

            if (encrypted.Length < _aesGcmNonceSize + _aeaGcmTagSize)
                throw new ArgumentException("Encrypted data is too short.", nameof(encrypted));

            ReadOnlySpan<byte> nonce = encrypted[.._aesGcmNonceSize];
            ReadOnlySpan<byte> ciphertext = encrypted[_aesGcmNonceSize..^_aeaGcmTagSize];
            ReadOnlySpan<byte> tag = encrypted[^_aeaGcmTagSize..];

            byte[] plaintext = new byte[ciphertext.Length];

            try
            {
                using var aes = new AesGcm(key, _aeaGcmTagSize);
                aes.Decrypt(nonce, ciphertext, tag, plaintext);

                var result = new SecureString();
                var chars = MemoryMarshal.Cast<byte, char>(plaintext);

                foreach (char c in chars)
                    result.AppendChar(c);

                result.MakeReadOnly();
                return result;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(plaintext);
            }
        }
    }
}