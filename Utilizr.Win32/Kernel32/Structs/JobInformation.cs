using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Kernel32.Structs;


[Flags]
public enum JobObjectLimitFlags : uint
{
    None = 0x00000000,
    // JOBOBJECT_BASIC_LIMIT_INFORMATION
    Workingset = 0x00000001,
    ProcessTime = 0x00000002,
    JobTime = 0x00000004,
    ActiveProcess = 0x00000008,
    Affinity = 0x00000010,
    PriorityClass = 0x00000020,
    PreserveJobTime = 0x00000040,
    SchedulingClass = 0x00000080,

    // JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    ProcessMemory = 0x00000100,
    JobMemory = 0x00000200,
    DieOnUnhandledException = 0x00000400,
    BreakawayOk = 0x00000800,
    SilentBreakawayOk = 0x00001000,
    KillOnJobClose = 0x00002000,
    SubsetAffinity = 0x00004000,
    JobMemoryLow = 0x00008000,
}

[StructLayout(LayoutKind.Sequential)]
public struct JOBOBJECT_BASIC_LIMIT_INFORMATION
{
    public long PerProcessUserTimeLimit;
    public long PerJobUserTimeLimit;
    public uint LimitFlags;
    public UIntPtr MinimumWorkingSetSize;
    public UIntPtr MaximumWorkingSetSize;
    public uint ActiveProcessLimit;
    public UIntPtr Affinity;
    public uint PriorityClass;
    public uint SchedulingClass;
}


[StructLayout(LayoutKind.Sequential)]
public struct IO_COUNTERS
{
    public ulong ReadOperationCount, WriteOperationCount, OtherOperationCount;
    public ulong ReadTransferCount, WriteTransferCount, OtherTransferCount;
}


[StructLayout(LayoutKind.Sequential)]
public struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
{
    public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
    public IO_COUNTERS IoInfo;
    public UIntPtr ProcessMemoryLimit;
    public UIntPtr JobMemoryLimit;
    public UIntPtr PeakProcessMemoryUsed;
    public UIntPtr PeakJobMemoryUsed;
}
