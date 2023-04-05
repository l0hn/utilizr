using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Utilizr.Logging;
using Utilizr.Win32.Kernel32.Flags;
using Utilizr.Win32.Ntdll;
using Utilizr.Win32.Ntdll.Flags;
using Utilizr.Win32.Ntdll.Structs;
using Kernel32 = Utilizr.Win32.Kernel32.Kernel32;

namespace Utilizr.Win.FileSystem
{
    public enum HandleType
    {
        Unknown,
        Other,
        File,
        Directory,
        SymbolicLink,
        Key,
        Process,
        Thread,
        Job,
        Session,
        WindowStation,
        Timer,
        Desktop,
        Semaphore,
        Token,
        Mutant,
        Section,
        Event,
        KeyedEvent,
        IoCompletion,
        IoCompletionReserve,
        TpWorkerFactory,
        AlpcPort,
        WmiGuid,
        UserApcReserve,
    }

    public class HandleInfo
    {
        public uint ProcessId { get; private set; }
        public IntPtr Handle { get; private set; }
        public int GrantedAccess { get; private set; }
        public byte RawType { get; private set; }

        private HandleType _type;
        private string? _name, _typeStr, _dosName;
        private static readonly Dictionary<byte, string> _rawTypeMap = new Dictionary<byte, string>();

        public HandleInfo(uint processId, IntPtr handle, int grantedAccess, byte rawType)
        {
            ProcessId = processId;
            Handle = handle;
            GrantedAccess = grantedAccess;
            RawType = rawType;
        }

        public string Name { get { if (_name == null) InitTypeAndName(); return _name!; } }
        public string DosName { get { if (_name == null) InitTypeAndName(); return _dosName!; } }
        public string TypeString { get { if (_typeStr == null) InitType(); return _typeStr!; } }
        public HandleType Type { get { if (_typeStr == null) InitType(); return _type; } }

        private void InitType()
        {
            if (_rawTypeMap.ContainsKey(RawType))
            {
                _typeStr = _rawTypeMap[RawType];
                _type = HandleTypeFromString(_typeStr);
            }
            else
            {
                InitTypeAndName();
            }
        }

        bool _typeAndNameAttempted = false;

        private void InitTypeAndName()
        {
            if (_typeAndNameAttempted)
                return;
            _typeAndNameAttempted = true;

            IntPtr sourceProcessHandle = IntPtr.Zero;
            IntPtr handleDuplicate = IntPtr.Zero;
            try
            {
                sourceProcessHandle = Kernel32.OpenProcess(ProcessAccessFlags.DupHandle, true, ProcessId);

                // To read info about a handle owned by another process we must duplicate it into ours
                // For simplicity, current process handles will also get duplicated; remember that process handles cannot be compared for equality
                if (!Kernel32.DuplicateHandle(sourceProcessHandle, Handle, Kernel32.GetCurrentProcess(), out handleDuplicate, 0, false, 2 /* same_access */))
                    return;

                // Query the object type
                if (_rawTypeMap.ContainsKey(RawType))
                {
                    _typeStr = _rawTypeMap[RawType];
                }
                else
                {
                    int length;
                    Ntdll.NtQueryObject(handleDuplicate, OBJECT_INFORMATION_CLASS.ObjectTypeInformation, IntPtr.Zero, 0, out length);
                    IntPtr ptr = IntPtr.Zero;
                    try
                    {
                        ptr = Marshal.AllocHGlobal(length);
                        if (Ntdll.NtQueryObject(handleDuplicate, OBJECT_INFORMATION_CLASS.ObjectTypeInformation, ptr, length, out length) != NT_STATUS.STATUS_SUCCESS)
                            return;

                        _typeStr = Marshal.PtrToStringUni((IntPtr)((int)ptr + 0x58 + 2 * IntPtr.Size));
                        _rawTypeMap[RawType] = _typeStr!;
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                }
                _type = HandleTypeFromString(_typeStr!);

                var type = Kernel32.GetFileType(handleDuplicate);

                // Query the object name
                if (type == FileType.Disk
                    /*&& GrantedAccess != 0x0012019f /*&& GrantedAccess != 0x00120189 && GrantedAccess != 0x120089*/)
                    // don't query some objects that could get stuck
                {
                    NtQueryObjectWithTimeout(handleDuplicate, OBJECT_INFORMATION_CLASS.ObjectNameInformation, IntPtr.Zero, 0, out int length);
                    IntPtr ptr = IntPtr.Zero;
                    try
                    {
                        ptr = Marshal.AllocHGlobal(length);
                        if (NtQueryObjectWithTimeout(handleDuplicate, OBJECT_INFORMATION_CLASS.ObjectNameInformation, ptr, length, out length) != NT_STATUS.STATUS_SUCCESS)
                            return;

                        _name = Marshal.PtrToStringUni((IntPtr) ((int) ptr + 2*IntPtr.Size));
                        DevicePath.ConvertDevicePathToDosPath(_name!, out _dosName);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("open_files", ex);
                throw;
            }
            finally
            {
                Kernel32.CloseHandle(sourceProcessHandle);
                if (handleDuplicate != IntPtr.Zero)
                    Kernel32.CloseHandle(handleDuplicate);
            }
        }

        static NT_STATUS NtQueryObjectWithTimeout(IntPtr handle, OBJECT_INFORMATION_CLASS objectInformationClass, IntPtr objectInformation, int objectInformationLength, out int returnLength)
        {
            var outLength = 0;
            NT_STATUS status = NT_STATUS.STATUS_SUCCESS;
            var task = Task.Run(() =>
            {
                int length = 0;
                status = Ntdll.NtQueryObject(handle, objectInformationClass, objectInformation, objectInformationLength, out length);
                outLength = length;
            });

            if (!task.Wait(TimeSpan.FromSeconds(30)))
                throw new TimeoutException("Failed to query ntobjectinfo within the allotted timeout");

            returnLength = outLength;
            return status;
        }

        public static HandleType HandleTypeFromString(string typeStr)
        {
            switch (typeStr)
            {
                case null: return HandleType.Unknown;
                case "File": return HandleType.File;
                case "IoCompletion": return HandleType.IoCompletion;
                case "TpWorkerFactory": return HandleType.TpWorkerFactory;
                case "ALPC Port": return HandleType.AlpcPort;
                case "Event": return HandleType.Event;
                case "Section": return HandleType.Section;
                case "Directory": return HandleType.Directory;
                case "KeyedEvent": return HandleType.KeyedEvent;
                case "Process": return HandleType.Process;
                case "Key": return HandleType.Key;
                case "SymbolicLink": return HandleType.SymbolicLink;
                case "Thread": return HandleType.Thread;
                case "Mutant": return HandleType.Mutant;
                case "WindowStation": return HandleType.WindowStation;
                case "Timer": return HandleType.Timer;
                case "Semaphore": return HandleType.Semaphore;
                case "Desktop": return HandleType.Desktop;
                case "Token": return HandleType.Token;
                case "Job": return HandleType.Job;
                case "Session": return HandleType.Session;
                case "IoCompletionReserve": return HandleType.IoCompletionReserve;
                case "WmiGuid": return HandleType.WmiGuid;
                case "UserApcReserve": return HandleType.UserApcReserve;
                default: return HandleType.Other;
            }
        }
    }

    public static class HandleUtil
    {
        public static IEnumerable<HandleInfo> GetHandles()
        {
            // Attempt to retrieve the handle information
            int length = 0x10000;
            IntPtr ptr = IntPtr.Zero;
            try
            {
                while (true)
                {
                    ptr = Marshal.AllocHGlobal(length);
                    int wantedLength;
                    var result = Ntdll.NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemHandleInformation, ptr, length, out wantedLength);
                    if (result == NT_STATUS.STATUS_INFO_LENGTH_MISMATCH)
                    {
                        length = Math.Max(length, wantedLength);
                        Marshal.FreeHGlobal(ptr);
                        ptr = IntPtr.Zero;
                    }
                    else if (result == NT_STATUS.STATUS_SUCCESS)
                        break;
                    else
                        throw new Exception("Failed to retrieve system handle information.");
                }

                int handleCount = IntPtr.Size == 4 ? Marshal.ReadInt32(ptr) : (int)Marshal.ReadInt64(ptr);
                int offset = IntPtr.Size;
                int size = Marshal.SizeOf(typeof(SystemHandleEntry));
                for (int i = 0; i < handleCount; i++)
                {
                    var struc = Marshal.PtrToStructure<SystemHandleEntry>((int)ptr + offset);
                    yield return new HandleInfo(struc.OwnerProcessId, struc.Handle, struc.GrantedAccess, struc.ObjectTypeNumber);
                    offset += size;
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }
    }
}