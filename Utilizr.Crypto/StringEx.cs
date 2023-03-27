using System;
using System.Collections.Generic;
using System.Text;

namespace Utilizr.Crypto
{
    public static class StringEx
    {
        public static string HashSHA256(this string str)
        {
            return Hash.SHA256(str);
        }

        public static string HashMD5(this string str)
        {
            return Hash.MD5(str);
        }
    }
}
