using System;
using System.IO;
using System.Runtime.InteropServices;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace Utilizr.Win32.Kernel32.Structs
{
    public static class FIND_DATA_CONSTS
    {
        public const Int32 MAX_PATH = 260;
        public const Int32 MAX_ALTERNATE = 14;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WIN32_FIND_DATAW
    {
        public Int32 dwFileAttributes;
        public FILETIME ftCreationTime;
        public FILETIME ftLastAccessTime;
        public FILETIME ftLastWriteTime;
        public UInt32 nFileSizeHigh;
        public UInt32 nFileSizeLow;
        public UInt32 dwReserved0;
        public UInt32 dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = FIND_DATA_CONSTS.MAX_PATH)]
        public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = FIND_DATA_CONSTS.MAX_ALTERNATE)]
        public string cAlternateFileName;
    }

    public static class Win32FindDataHelper
    {
        public static long GetSize(this WIN32_FIND_DATAW data)
        {
            const long maxDword = 0xffffffff;
            return (data.nFileSizeHigh * (maxDword + 1)) + data.nFileSizeLow;
        }

        public static string GetExtension(this WIN32_FIND_DATAW data)
        {
            return Path.GetExtension(data.cFileName);
        }
    }
}
