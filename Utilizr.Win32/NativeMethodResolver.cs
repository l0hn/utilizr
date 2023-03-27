using System;
using static Utilizr.Win32.Kernel32.Kernel32;

namespace Utilizr.Win32
{
    public static class NativeMethodResolver
    {
        public static bool MethodExists(string libraryName, string methodName)
        {
            var libraryPtr = LoadLibrary(libraryName);
            var procPtr = GetProcAddress(libraryPtr, methodName);

            return libraryPtr != UIntPtr.Zero && procPtr != UIntPtr.Zero;
        }
    }

    public abstract class SafeNativeMethodResolver
    {
        bool _exists;
        bool _resolved;
        readonly string _libraryName;
        readonly string _methodName;

        protected SafeNativeMethodResolver(string libraryName, string methodName)
        {
            _libraryName = libraryName;
            _methodName = methodName;
        }

        protected bool CanInvoke
        {
            get
            {
                if (!_resolved)
                {
                    _exists = NativeMethodResolver.MethodExists(_libraryName, _methodName);
                    _resolved = true;
                }

                return _exists;
            }
        }
    }
}