using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;

namespace Utilizr.Win.Extensions
{
    public class EventWaitHandleEx
    {
        [DllImport("kernel32", EntryPoint = "OpenEventW", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern SafeWaitHandle OpenEvent(uint desiredAccess, bool inheritHandle, string name);

        [DllImport("kernel32", EntryPoint = "CreateEventW", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern SafeWaitHandle CreateEvent(SECURITY_ATTRIBUTES lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [StructLayout(LayoutKind.Sequential)]
        struct SECURITY_ATTRIBUTES{
            public int length;
            public IntPtr securityDesc;
            public bool inherit;
        }

        public static EventWaitHandle Create(string name, EventWaitHandleSecurity security, EventResetMode mode, bool initialState)
        {
            var securityAttributes = new SECURITY_ATTRIBUTES();
            securityAttributes.length = Marshal.SizeOf(securityAttributes);

            var descriptor = security.GetSecurityDescriptorBinaryForm();

            IntPtr ptrDescriptor = Marshal.AllocHGlobal(descriptor.Length);
            Marshal.Copy(descriptor, 0, ptrDescriptor, descriptor.Length);

            securityAttributes.securityDesc = ptrDescriptor;

            SafeWaitHandle handle =
                CreateEvent(securityAttributes, mode == EventResetMode.ManualReset, initialState, name);

            var evt = new EventWaitHandle(false, mode);
            var old = evt.SafeWaitHandle;
            evt.SafeWaitHandle = handle;
            old.Dispose();

            //required?
            //Marshal.FreeHGlobal(ptrDescriptor);

            return evt;
        }

        public static bool TryOpenExisting(string name, EventWaitHandleRights rights, EventResetMode mode, out EventWaitHandle? result)
        {
            try
            {
                result = OpenExisting(name, rights, mode);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        public static EventWaitHandle OpenExisting(string name, EventWaitHandleRights rights, EventResetMode mode)
        {
            SafeWaitHandle handle = OpenEvent((uint)rights, false, name);

            if (handle.IsInvalid)
            {
                var err = Marshal.GetLastWin32Error();
                throw new Win32Exception(err);
            }

            var result = new EventWaitHandle(false, mode);
            SafeWaitHandle old = result.SafeWaitHandle;
            result.SafeWaitHandle = handle;
            old.Dispose();

            return result;
        }
    }
}