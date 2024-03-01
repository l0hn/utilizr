using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Utilizr.Util
{
    public class PinnedString : IDisposable
    {
        public SecureString SecureString { get; }
        public string? String { get; protected set; }

        private GCHandle _gcHandle;

        public PinnedString(SecureString secureString)
        {
            SecureString = secureString;
            UpdateStringValue();
        }


        void UpdateStringValue()
        {
            Deallocate();

            unsafe
            {
                if (SecureString == null)
                    return;

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
                catch (Exception)
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

                    // Free the handle so the garbage collector
                    // can dispose of it properly.
                    _gcHandle.Free();
                }
            }
            catch (Exception)
            {

            }
        }

        public void Dispose()
        {
            Deallocate();
        }
    }
}
