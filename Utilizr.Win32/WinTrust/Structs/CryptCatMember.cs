using System;
using System.Runtime.InteropServices;
using static Utilizr.Win32.WinTrust.WinTrust;

namespace Utilizr.Win32.WinTrust.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CRYPTCATMEMBER
    {
        public uint cbStruct;
        [MarshalAs(UnmanagedType.LPWStr)] public string pwszReferenceTag;
        [MarshalAs(UnmanagedType.LPWStr)] public string pwszFileName;
        public GUID gSubjectType;
        public uint fdwMemberFlags;
        public IntPtr pIndirectData;
        public uint dwCertVersion;
        public uint dwReserved;
        public IntPtr hReserved;
        public CRYPT_ATTR_BLOB sEncodedIndirectData;
        public CRYPT_ATTR_BLOB sEncodedMemberInfo;
    };
}
