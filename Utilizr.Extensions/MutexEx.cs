using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Utilizr.Extensions
{
    public static class MutexEx
    {
        public static bool SafeWaitOne(this Mutex mutex, int millisecondsTimeout = -1, bool exitContext = false)
        {
            return mutex.SafeWaitOne(out bool _, millisecondsTimeout, exitContext);
        }

        public static bool SafeWaitOne(this Mutex mutex, out bool wasAdandoned, int millisecondsTimeout = -1, bool exitContext = false)
        {
            try
            {
                wasAdandoned = false;
                return mutex.WaitOne(millisecondsTimeout, exitContext);
            }
            catch (AbandonedMutexException)
            {
                wasAdandoned = true;
                return true;
            }
        }

        public static bool SafeWaitOne(this EventWaitHandle eventWaitHandle, int millisecondsTimeout = -1, bool exitContext = false)
        {
            return eventWaitHandle.SafeWaitOne(out bool _, millisecondsTimeout, exitContext);
        }

        public static bool SafeWaitOne(this EventWaitHandle eventWaitHandle, out bool wasAbandoned, int millisecondsTimeout = -1, bool exitContext = false)
        {
            try
            {
                wasAbandoned = false;
                return eventWaitHandle.WaitOne(millisecondsTimeout, exitContext);
            }
            catch (AbandonedMutexException)
            {
                wasAbandoned = true;
                return true;
            }
        }

        [DllImport("kernel32", EntryPoint = "OpenMutexW", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern SafeWaitHandle OpenMutex(uint desiredAccess, bool inheritHandle, string name);

        public static bool TryOpenExisting(string name, MutexRights rights, out Mutex? result)
        {
            SafeWaitHandle handle = OpenMutex((uint)rights, false, name);

            if (handle.IsInvalid)
            {
                result = null;
                return false;
            }

            result = new Mutex(initiallyOwned: false);
            SafeWaitHandle old = result.SafeWaitHandle;
            result.SafeWaitHandle = handle;
            old.Dispose();

            return true;
        }
    }
}
