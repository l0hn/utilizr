using System;
using Kernel32 = Utilizr.Win32.Kernel32.Kernel32;

namespace Utilizr.Win.FileSystem
{
    public class DisableRedirectionFS: IDisposable
    {
        private IntPtr _wow64 = IntPtr.Zero;

        public DisableRedirectionFS()
        {
            Kernel32.Wow64DisableWow64FsRedirection(ref _wow64);
        }

        public void Dispose()
        {
            Kernel32.Wow64RevertWow64FsRedirection(_wow64);
        }
    }
}
