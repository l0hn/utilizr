using System;
using System.Runtime.InteropServices;
using Utilizr.Win32.Kernel32;
using Utilizr.Win32.Kernel32.Structs;

namespace Utilizr.Win.Info
{
    public class WindowsJobObject : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        struct JOBOBJECT_ASSOCIATE_COMPLETION_PORT
        {
            public UIntPtr CompletionKey;
            public IntPtr CompletionPort;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        // Resume the previously created and suspended process
        public bool StartProcessAndWait(PROCESS_INFORMATION? pi, nint? hProcess = null, nint? hThread = null)
        {
            IntPtr job = Kernel32.CreateJobObject(IntPtr.Zero, null);
            IntPtr ioPort = Kernel32.CreateIoCompletionPort(IntPtr.Zero, IntPtr.Zero, UIntPtr.Zero, 1);
            var success = false;

            JOBOBJECT_ASSOCIATE_COMPLETION_PORT completionPort = new JOBOBJECT_ASSOCIATE_COMPLETION_PORT
            {
                CompletionKey = (UIntPtr)job,
                CompletionPort = ioPort
            };

            IntPtr completionPortPtr = Marshal.AllocHGlobal(Marshal.SizeOf(completionPort));
            Marshal.StructureToPtr(completionPort, completionPortPtr, false);

            Kernel32.SetInformationJobObject(job, 7, completionPortPtr, (uint)Marshal.SizeOf(completionPort));

            if (pi != null)
            {
                Kernel32.AssignProcessToJobObject(job, ((PROCESS_INFORMATION)pi).hProcess);
                Kernel32.ResumeThread(((PROCESS_INFORMATION)pi).hThread);
            }
            else
            {
                Kernel32.AssignProcessToJobObject(job, (nint)hProcess!);
                Kernel32.ResumeThread((nint)hThread!);
            }

            uint numberOfBytes;
            UIntPtr completionKey;
            IntPtr overlapped;

            // Wait for job notifications
            while (Kernel32.GetQueuedCompletionStatus(ioPort, out numberOfBytes, out completionKey, out overlapped, uint.MaxValue))
            {
                if (completionKey == (UIntPtr)job)
                {
                    success = true;
                    break;
                }
            }

            Marshal.FreeHGlobal(completionPortPtr);

            return success;
        }
    }
}