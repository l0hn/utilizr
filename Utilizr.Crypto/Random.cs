using System.Security.Cryptography;

namespace Utilizr.Crypto
{
    public static class Random
    {
        public static byte NextByte(this RNGCryptoServiceProvider rand)
        {
            byte[] buffer = new byte[1];
            rand.GetBytes(buffer);
            return buffer[0];
        }
    }
}
