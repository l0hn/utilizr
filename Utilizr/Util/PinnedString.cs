using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Utilizr.Util
{
    /// <summary>
    /// A wrapper to get a the contents of a SecureString, pinned it to a certain address with GC.Alloc.
    /// This means sensitive strings won't be left floating in memory.
    /// </summary>
    public sealed class PinnedString : IDisposable
    {
        public SecureString? SecureString { get; }

        private IntPtr _bstr;

        public PinnedString(SecureString? secureString)
        {
            SecureString = secureString;
            
            _bstr = IntPtr.Zero;

            if (secureString != null)
                _bstr = Marshal.SecureStringToBSTR(secureString);
        }

        public unsafe ReadOnlySpan<char> ReadChars()
        {
            if (SecureString == null || _bstr == IntPtr.Zero)
                return new ReadOnlySpan<char>();

            int length = SecureString.Length;
            var chars = (char*)_bstr;
            return new ReadOnlySpan<char>(chars, length);
        }

        public unsafe ReadOnlySpan<byte> ReadBytes()
        {
            if (SecureString == null || _bstr == IntPtr.Zero)
                return new ReadOnlySpan<byte>();

            var bytes = (byte*)_bstr;
            var byteLength = SecureString.Length * sizeof(char);
            return new ReadOnlySpan<byte>(bytes, byteLength);
        }

        public void Dispose()
        {
            if (_bstr != IntPtr.Zero)
            {
                Marshal.ZeroFreeBSTR(_bstr);
                _bstr = IntPtr.Zero;
            }
        }
    }
}
