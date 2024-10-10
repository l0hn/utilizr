using System;
using System.Runtime.InteropServices;
using Utilizr.Win32.Kernel32;
using Utilizr.Win32.Kernel32.Structs;

namespace Utilizr.Win.Info
{
    public class WindowsJobObject : IDisposable
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetInformationJobObject(IntPtr hJob, int JobObjectInfoClass, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateIoCompletionPort(IntPtr FileHandle, IntPtr ExistingCompletionPort, UIntPtr CompletionKey, uint NumberOfConcurrentThreads);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetQueuedCompletionStatus(IntPtr CompletionPort, out uint lpNumberOfBytes, out UIntPtr lpCompletionKey, out IntPtr lpOverlapped, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

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
        public void StartProcessAndWait(PROCESS_INFORMATION? pi, nint? hProcess = null, nint? hThread = null)
        {
            IntPtr job = CreateJobObject(IntPtr.Zero, null);
            IntPtr ioPort = CreateIoCompletionPort(IntPtr.Zero, IntPtr.Zero, UIntPtr.Zero, 1);

            JOBOBJECT_ASSOCIATE_COMPLETION_PORT completionPort = new JOBOBJECT_ASSOCIATE_COMPLETION_PORT
            {
                CompletionKey = (UIntPtr)job,
                CompletionPort = ioPort
            };

            IntPtr completionPortPtr = Marshal.AllocHGlobal(Marshal.SizeOf(completionPort));
            Marshal.StructureToPtr(completionPort, completionPortPtr, false);

            SetInformationJobObject(job, 7, completionPortPtr, (uint)Marshal.SizeOf(completionPort));

            if (pi != null)
            {
                AssignProcessToJobObject(job, ((PROCESS_INFORMATION)pi).hProcess);
                Kernel32.ResumeThread(((PROCESS_INFORMATION)pi).hThread);
            }
            else
            {
                AssignProcessToJobObject(job, (nint)hProcess!);
                Kernel32.ResumeThread((nint)hThread!);
            }

            uint numberOfBytes;
            UIntPtr completionKey;
            IntPtr overlapped;

            // Wait for job notifications
            while (GetQueuedCompletionStatus(ioPort, out numberOfBytes, out completionKey, out overlapped, uint.MaxValue))
            {
                if (completionKey == (UIntPtr)job)
                    break;
            }

            Marshal.FreeHGlobal(completionPortPtr);
        }
    }
}