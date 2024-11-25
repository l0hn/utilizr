using System;
using System.Runtime.InteropServices;
using Utilizr.Win32.Ntdll.Flags;

namespace Utilizr.Win32.Ntdll;

public static class Ntdll
{
    const string NTDLL_DLL = "ntdll.dll";

    [DllImport(NTDLL_DLL)]
    public static extern NT_STATUS NtQuerySystemInformation(
        [In] SYSTEM_INFORMATION_CLASS SystemInformationClass,
        [In] IntPtr SystemInformation,
        [In] int SystemInformationLength,
        [Out] out int ReturnLength);


    [DllImport(NTDLL_DLL)]
    public static extern NT_STATUS NtQueryObject(
        [In] IntPtr Handle,
        [In] OBJECT_INFORMATION_CLASS ObjectInformationClass,
        [In] IntPtr ObjectInformation,
        [In] int ObjectInformationLength,
        [Out] out int ReturnLength);


    [DllImport("ntdll.dll")]
    public static extern int NtQueryInformationProcess(
        IntPtr ProcessHandle,
        int ProcessInformationClass,
        ref PROCESS_BASIC_INFORMATION ProcessInformation,
        int ProcessInformationLength,
        ref int ReturnLength
    );
}
