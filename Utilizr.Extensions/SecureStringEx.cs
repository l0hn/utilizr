using System;
using System.Security;
using System.Runtime.InteropServices;

namespace Utilizr.Extensions
{
    public static class SecureStringEx
    {
        public static string? ToUnsecureString(this SecureString secureString)
        {
            if (secureString == null)
                return null;

            var unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public static SecureString? ToSecureString(this string password)
        {
            if (password == null)
                return null;

            var secString = new SecureString();
            foreach (var c in password)
            {
                secString.AppendChar(c);
            }
            secString.MakeReadOnly();
            return secString;
        }
    }
}