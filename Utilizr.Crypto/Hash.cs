using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Utilizr.Crypto
{
    public static class Hash
    {
        public static byte[] MD5ToBytes(string input, Encoding encoding)
        {
            try
            {
                using MD5CryptoServiceProvider crypto = new MD5CryptoServiceProvider();
                byte[] inputBytes = encoding.GetBytes(input);
                byte[] hash = crypto.ComputeHash(inputBytes);
                return hash;
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }

        public static string MD5(string input)
        {
            return MD5(input, Encoding.UTF8);
        }

        public static string MD5(string input, Encoding encoding)
        {
            try
            {
                var hash = MD5ToBytes(input, encoding);
                var hashString = BitConverter.ToString(hash).Replace("-", "");
                return hashString.ToLower();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string MD5(byte[] input)
        {
            try
            {
                using MD5CryptoServiceProvider crypto = new MD5CryptoServiceProvider();
                byte[] hash = crypto.ComputeHash(input);
                var hashString = BitConverter.ToString(hash).Replace("-", "");
                return hashString.ToLower();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string MD5(Stream input)
        {
            try
            {
                using MD5CryptoServiceProvider crypto = new MD5CryptoServiceProvider();
                byte[] hash = crypto.ComputeHash(input);
                var hashString = BitConverter.ToString(hash).Replace("-", "");
                return hashString.ToLower();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string SHA256(string input) {
            return SHA256(input, Encoding.UTF8);
        }

        public static string SHA256(string input, Encoding encoding)
        {
            var inputBytes = encoding.GetBytes(input);
            return SHA256(inputBytes);
        }

        public static string SHA256(byte[] input) 
        {
            using var sha = new SHA256Managed();
            byte[] result;
            result = sha.ComputeHash(input);
            var formattedResult = BitConverter.ToString(result).Replace("-", "").ToLower();
            return formattedResult;
        }

        public static string SHA256(Stream input)
        {
            using var sha = new SHA256Managed();
            byte[] result;
            result = sha.ComputeHash(input);
            var formattedResult = BitConverter.ToString(result).Replace("-", "").ToLower();
            return formattedResult;
        }

        public static string SHA1(string input) {
            return SHA1(input, Encoding.UTF8);
        }

        public static string SHA1(string input, Encoding encoding)
        {
            var inputBytes = encoding.GetBytes(input);
            return SHA1(inputBytes);
        }

        public static string SHA1(byte[] input)
        {
            using var sha = new SHA1Managed();
            byte[] result;
            result = sha.ComputeHash(input);
            var formattedResult = BitConverter.ToString(result).Replace("-", "").ToLower();
            return formattedResult;
        }

        public static string SHA1(Stream input)
        {
            using var sha = new SHA1Managed();
            byte[] result;
            result = sha.ComputeHash(input);
            var formattedResult = BitConverter.ToString(result).Replace("-", "").ToLower();
            return formattedResult;
        }
    }
}
