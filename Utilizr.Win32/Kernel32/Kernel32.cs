using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Text;
using Utilizr.Win32.Kernel32.Flags;
using Utilizr.Win32.Kernel32.Structs;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace Utilizr.Win32.Kernel32
{
    public static class Kernel32
    {
        const string KERNEL32_DLL = "kernel32.dll";

        [DllImport(KERNEL32_DLL, SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern UIntPtr GetProcAddress(UIntPtr hModule, string lpProcName);

        [DllImport(KERNEL32_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern UIntPtr LoadLibrary(string lpFileName);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern IntPtr FindResource(IntPtr hModule, string lpName, uint lpType);

        [DllImport(KERNEL32_DLL)]
        public static extern IntPtr FindResource(IntPtr hModule, int lpID, string lpType);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);

        [DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateFileW(
            string lpFileName,
            FileAccessRightsFlags dwDesiredAccess,
            FileShareRightsFlags dwShareMode,
            IntPtr lpSecurityAttributes,
            FileCreationDispositionFlags dwCreationDisposition,
            FileAttributeFlags dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CreateDirectoryW(string directory, IntPtr lpSecurityAttributes);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetFilePointerEx(IntPtr hFile, long liDistanceToMove, [Out, Optional] IntPtr lpNewFilePointer, uint dwMoveMethod);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool VirtualFree(IntPtr lpAddress, uint dwSize, uint dwFreeType);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetFileSizeEx(IntPtr hFile, out long lpFileSize);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern int WriteFile(IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToWrite, ref uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

        [DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int DeleteFileW([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

        [DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int MoveFileExW([MarshalAs(UnmanagedType.LPWStr)] string lpExistingFileName, [MarshalAs(UnmanagedType.LPWStr)] string lpNewFileName, MoveFileFlags dwFlags);

        [DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr FindFirstFileW([MarshalAs(UnmanagedType.LPWStr)] string lpFileName, out WIN32_FIND_DATAW lpFindFileData);

        [DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FindNextFileW(IntPtr hFindFile, out WIN32_FIND_DATAW lpFindFileData);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FindClose(IntPtr hFindFile);

        [DllImport(KERNEL32_DLL, ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize,
        IntPtr lpOutBuffer, [Optional] uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetShortPathNameW([MarshalAs(UnmanagedType.LPWStr)] string lLongPath, [MarshalAs(UnmanagedType.LPWStr)] string lShortPath, int lBuffer);

        [DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int RemoveDirectoryW([MarshalAs(UnmanagedType.LPWStr)] string lpPathName);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern int CloseHandle(IntPtr hObject);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern int ReadFile(IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToRead, ref uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern bool WriteFile(IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToWrite, uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);

        public const int DUPLICATE_SAME_ACCESS = 0x2;
        public const int DUPLICATE_CLOSE_SOURCE = 0x1;

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DuplicateHandle(
            IntPtr hSourceProcessHandle,
            IntPtr hSourceHandle,
            IntPtr hTargetProcessHandle,
            out IntPtr lpTargetHandle,
            uint dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            uint dwOptions);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern bool DuplicateHandle(
            IntPtr hSourceProcessHandle,
            SafeHandle hSourceHandle,
            IntPtr hTargetProcess,
            out SafeFileHandle targetHandle,
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwOptions);

        [DllImport(KERNEL32_DLL, CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern UInt32 GetCurrentProcessId();

        [DllImport(KERNEL32_DLL)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport(KERNEL32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule,
        [MarshalAs(UnmanagedType.LPStr)] string procName);

        //WOW32
        [DllImport(KERNEL32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

        [DllImport(KERNEL32_DLL)]
        public static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern bool GetFileInformationByHandleEx(IntPtr hFile, FILE_INFO_BY_HANDLE_CLASS infoClass, out FILE_ID_BOTH_DIR_INFO dirInfo, uint dwBufferSize);

        [DllImport(KERNEL32_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetFileAttributesEx(string lpFileName, GET_FILEEX_INFO_LEVELS fInfoLevelId, out FILE_ATTRIBUTE_DATA fileData);


        [DllImport(KERNEL32_DLL)]
        public static extern uint GetConsoleOutputCP();

        [DllImport(KERNEL32_DLL)]
        public static extern FileType GetFileType(IntPtr hFile);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);

        [DllImport(KERNEL32_DLL)]
        public static extern uint WaitForMultipleObjects(uint nCount, IntPtr[] pHandles,
        bool bWaitAll, uint dwMilliseconds);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool blnheritHandle, uint dwAppProcessId);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern uint SuspendThread(IntPtr hThread);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern int ResumeThread(IntPtr hThread);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern bool QueryFullProcessImageName(
              [In] IntPtr hProcess,
              [In] int dwFlags,
              [Out] StringBuilder lpExeName,
              ref int lpdwSize);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern bool CreateProcess(
            string? lpApplicationName,
            string? lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string? lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation
        );

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern bool CreatePipe(
            out SafeFileHandle hReadPipe,
            out SafeFileHandle hWritePipe,
            ref SECURITY_ATTRIBUTES lpPipeAttributes,
            int nSize
        );

        public const uint WAIT_FOR_OBJECT_INFINITE = 0xFFFFFFFF;

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport(KERNEL32_DLL, CharSet = CharSet.Auto)]
        public static extern bool FileTimeToSystemTime(ref FILETIME FileTime, ref SYSTEMTIME SystemTime);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string lpName);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern bool SetInformationJobObject(IntPtr hJob, int JobObjectInfoClass, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern IntPtr CreateIoCompletionPort(IntPtr FileHandle, IntPtr ExistingCompletionPort, UIntPtr CompletionKey, uint NumberOfConcurrentThreads);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern bool GetQueuedCompletionStatus(IntPtr CompletionPort, out uint lpNumberOfBytes, out UIntPtr lpCompletionKey, out IntPtr lpOverlapped, uint dwMilliseconds);

        [DllImport(KERNEL32_DLL, SetLastError = true)]
        public static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);
    }
}
