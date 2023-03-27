using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using Utilizr.Extensions;
using Utilizr.Logging;


namespace Utilizr.Threading
{
    public class GlobalMutex: IDisposable
    {
        readonly string _path;

        public Mutex Mutex { get; private set; }

        /// <summary>
        /// Create a new system-wide global mutex, or open if already exists.
        /// </summary>
        /// <param name="name">Unique name of your mutex (e.g. "MyUniqueMutex")</param>
        /// <param name="initiallyOwned"></param>
        /// <returns></returns>
        public static GlobalMutex New(string name, bool initiallyOwned = false)
        {
            return new GlobalMutex($"Global\\com.utilizr.mutexes.{name}", initiallyOwned);
        }

        /// <summary>
        /// Create a new system-wide global mutex, or open if already exists.
        /// </summary>
        /// <param name="initiallyOwned"></param>
        /// <param name="fullPath">full path of the wait handle including scope (if you are unsure use a different constructor)</param>
        /// <returns></returns>
        public static GlobalMutex New(bool initiallyOwned, string fullPath)
        {
            return new GlobalMutex(fullPath, initiallyOwned);
        }

        private GlobalMutex(string fullPath, bool initiallyOwned)
        {
            _path = fullPath;

            try
            {
                Create(initiallyOwned);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                throw;
            }
        }

        private void Create(bool initiallyOwned)
        {
            var security = new MutexSecurity();
            var users = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var openRights = MutexRights.FullControl;

            security.AddAccessRule(new MutexAccessRule(users, openRights, AccessControlType.Allow));

            try
            {
                Mutex = Mutex.OpenExisting(_path);
            }
            catch (Exception)
            {
                Mutex = new Mutex(initiallyOwned, _path, out bool createdNew);
                Mutex.SetAccessControl(security);
            }
        }

        public void ReleaseMutex()
        {
            Mutex.ReleaseMutex();
        }

        /// <summary>
        /// SafeWaitOne performs a WaitOne with built-in check for abandoned mutexes
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="exitContext"></param>
        public bool SafeWaitOne(int timeout = -1, bool exitContext = false)
        {
            return Mutex.SafeWaitOne(timeout, exitContext);
        }

        public void Dispose()
        {
            Mutex?.Close();
        }

        public bool ExecuteSynchronized(System.Action action, int timeout = -1, bool exitContext = false)
        {
            var acquired = SafeWaitOne(timeout, exitContext);

            if (!acquired)
                return false;

            try
            {
                action();
                return true;
            }
            catch (Exception e)
            {
                Log.Exception(e);
                throw;
            }
            finally
            {
                if (acquired)
                    ReleaseMutex();
            }
        }
    }
}