using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using Utilizr.Logging;
using Utilizr.Win.Extensions;

namespace Utilizr.Win.Threading
{
    public class GlobalEventWaitHandle : IDisposable
    {
        private readonly string _path;
        public EventWaitHandle WaitHandle { get; private set; }

        /// <summary>
        /// Create a new global wait handle, or open if already exists.
        /// </summary>
        /// <param name="name">Unique name of your wait handle (e.g. "MyUniqueWaitHandle")</param>
        /// <param name="initialState"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static GlobalEventWaitHandle New(string name, bool initialState = false, EventResetMode mode = EventResetMode.AutoReset)
        {
            return new GlobalEventWaitHandle($"Global\\com.utilizr.waithandles.{name}", initialState, mode);
        }

        /// <summary>
        /// Create a new global wait handle, or open if already exists.
        /// </summary>
        /// <param name="initialState"></param>
        /// <param name="fullPath">full path of the wait handle including scope (if you are unsure use a different constructor)</param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static GlobalEventWaitHandle New(bool initialState, string fullPath, EventResetMode mode)
        {
            return new GlobalEventWaitHandle(fullPath, initialState, mode);
        }

        private GlobalEventWaitHandle(string fullPath, bool initialState, EventResetMode mode)
        {
            _path = fullPath;

            try
            {
                Create(initialState, mode);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                throw;
            }
        }

        [MemberNotNull(nameof(WaitHandle))]
        private void Create(bool initialState, EventResetMode mode)
        {
            var eventSecurity = new EventWaitHandleSecurity();
            var eventOpenRights = EventWaitHandleRights.FullControl;
            var users = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            eventSecurity.AddAccessRule(new EventWaitHandleAccessRule(users, eventOpenRights, AccessControlType.Allow));
            try
            {
                WaitHandle = EventWaitHandle.OpenExisting(_path);
            }
            catch (Exception)
            {
                WaitHandle = new EventWaitHandle(initialState, mode, _path, out _);
                WaitHandle.SetAccessControl(eventSecurity);
            }
        }

        /// <summary>
        /// SafeWaitOne performs a WaitOne with built-in check for abandoned mutexes
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="exitContext"></param>
        public bool SafeWaitOne(int timeout = -1, bool exitContext = false)
        {
            return WaitHandle.SafeWaitOne(timeout, exitContext);
        }

        public bool SafeWaitOne(out bool wasAbandoned, int timeout = -1, bool exitContext = false)
        {
            return WaitHandle.SafeWaitOne(out wasAbandoned, timeout, exitContext);
        }

        public void Set()
        {
            WaitHandle.Set();
        }

        public void Reset()
        {
            WaitHandle.Reset();
        }

        public void Dispose()
        {
            WaitHandle?.Close();
            GC.SuppressFinalize(this);
        }
    }
}