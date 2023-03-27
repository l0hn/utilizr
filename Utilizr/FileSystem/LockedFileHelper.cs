using System;
using System.Collections.Generic;
using System.Diagnostics;
using Utilizr.Logging;
using Utilizr.Win32.Kernel32.Flags;
using Kernel32 = Utilizr.Win32.Kernel32.Kernel32;

namespace Utilizr.FileSystem
{
    public static class LockedFileHelper
    {
        public static IEnumerable<Process> WhosLocking(string path)
        {
            var _procDict = new Dictionary<uint, Process>();
            foreach (var handleInfo in GetHandlesForFile(path))
            {
                if (_procDict.ContainsKey(handleInfo.ProcessId))
                    continue;

                Process process;
                try
                {
                    process = Process.GetProcessById((int)handleInfo.ProcessId);
                }
                catch (Exception)
                {
                    continue;
                }

                _procDict[handleInfo.ProcessId] = process;
                yield return process;
            }
        }

        public static IEnumerable<HandleInfo> GetHandlesForFile(string file)
        {
            foreach (var handleInfo in HandleUtil.GetHandles())
            {
                if (handleInfo.Name != null && handleInfo.Name.Contains("testlock"))
                {
                    Console.WriteLine(handleInfo.Name);
                }

                if (string.IsNullOrEmpty(handleInfo.DosName))
                    continue;

                if (!file.Equals(handleInfo.DosName, StringComparison.InvariantCultureIgnoreCase))
                {
                    Log.Info($"{handleInfo.ProcessId} {handleInfo.DosName}");
                    continue;
                }

                yield return handleInfo;
            }
        }

        public static void CloseHandlesForFile(string file)
        {
            foreach (var handleInfo in GetHandlesForFile(file))
            {
                //close the handle? can it be this easy?
                var hnd = (IntPtr)handleInfo.Handle;
                Console.WriteLine($"Attempting to close handle [{hnd:x2}] for [{file}]");
                Log.Info($"Attempting to close handle [{hnd:x2}] for [{file}]");

                CloseDuplicateHandle(handleInfo.Handle, handleInfo.ProcessId);
            }
        }

        public static void CloseDuplicateHandle(IntPtr handle, uint processId)
        {
            var sourceProcessHandle = Kernel32.OpenProcess(ProcessAccessFlags.DupHandle, true, processId);
            if (sourceProcessHandle != IntPtr.Zero)
            {
                try
                {
                    IntPtr realHandle = IntPtr.Zero;
                    var result = Kernel32.DuplicateHandle(
                        sourceProcessHandle,
                        handle,
                        Process.GetCurrentProcess().Handle,
                        out realHandle,
                        0,
                        true,
                        Kernel32.DUPLICATE_CLOSE_SOURCE
                    );

                    if (result)
                    {
                        Console.WriteLine($"closing real handle [{realHandle:x2}]");
                        int closeResult = Kernel32.CloseHandle(realHandle);
                        Log.Info($"Attempt to close handle [{realHandle:x2}] result = {closeResult}");
                        Console.WriteLine($"Attempt to close handle [{realHandle:x2}] result = {closeResult}");
                    }
                }
                finally
                {
                    Kernel32.CloseHandle(sourceProcessHandle);
                }
            }
        }
    }
}