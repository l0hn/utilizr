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
        public string? String { get; private set; }

        private GCHandle _gcHandle;

        readonly Action<Exception>? _exceptionCallback;

        public PinnedString(SecureString? secureString, Action<Exception>? exceptionCallback = null)
        {
            _exceptionCallback = exceptionCallback;
            SecureString = secureString;
            UpdateStringValue();
        }


        void UpdateStringValue()
        {
            Deallocate();

            unsafe
            {
                if (SecureString == null)
                {
                    String = null;
                    return;
                }

                var length = SecureString.Length;
                String = new string('\0', length);

                var stringPtr = IntPtr.Zero;
                try
                {
                    _gcHandle = new GCHandle();

                    // Pin our string, disallowing the garbage collector from moving it around.
                    _gcHandle = GCHandle.Alloc(String, GCHandleType.Pinned);
                    stringPtr = Marshal.SecureStringToBSTR(SecureString);

                    // Copy the SecureString content to our pinned string
                    char* pString = (char*)stringPtr;
                    char* pInsecureString = (char*)_gcHandle.AddrOfPinnedObject();
                    for (int index = 0; index < length; index++)
                    {
                        pInsecureString[index] = pString[index];
                    }
                }
                catch (Exception ex)
                {
                    _exceptionCallback?.Invoke(ex);
                }
                finally
                {
                    if (stringPtr != IntPtr.Zero)
                    {
                        // Free the SecureString BSTR that was generated
                        Marshal.ZeroFreeBSTR(stringPtr);
                    }
                }
            }
        }

        void Deallocate()
        {
            if (!_gcHandle.IsAllocated)
                return;

            try
            {
                unsafe
                {
                    // Determine the length of the string
                    var length = String!.Length;

                    // Zero each character of the string.
                    char* pInsecureString = (char*)_gcHandle.AddrOfPinnedObject();
                    for (int index = 0; index < length; index++)
                    {
                        pInsecureString[index] = '\0';
                    }

                    // Free the handle so the garbage collector can dispose of it properly.
                    _gcHandle.Free();
                }
            }
            catch (Exception ex)
            {
                _exceptionCallback?.Invoke(ex);
            }
        }






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
            if (SecureString == null)
                return new ReadOnlySpan<char>();

            int length = SecureString.Length;
            var chars = (char*)_bstr;
            return new ReadOnlySpan<char>(chars, length);
        }

        public unsafe ReadOnlySpan<byte> ReadBytes()
        {
            if (SecureString == null)
                return new ReadOnlySpan<byte>();

            var bytes = (byte*)_bstr;
            var byteLength = SecureString.Length * sizeof(char);
            return new ReadOnlySpan<byte>(bytes, byteLength);
        }

        public void Dispose()
        {
            Deallocate();

            if (_bstr != IntPtr.Zero)
            {
                Marshal.ZeroFreeBSTR(_bstr);
                _bstr = IntPtr.Zero;
            }
        }
    }
}
