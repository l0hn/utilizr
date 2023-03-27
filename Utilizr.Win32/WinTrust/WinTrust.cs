using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Utilizr.Win32.WinTrust
{
    public static class WinTrust
    {
        const string WIN_TRUST_DLL = "wintrust.dll";

        [DllImport(WIN_TRUST_DLL, PreserveSig = true, SetLastError = false)]
        public static extern uint WinVerifyTrust(IntPtr hWnd, IntPtr pgActionID, IntPtr pWinTrustData);

        [DllImport(WIN_TRUST_DLL)]
        public static extern bool IsCatalogFile(SafeFileHandle hFile, [MarshalAs(UnmanagedType.LPWStr)] string pwszFileName);

        [DllImport(WIN_TRUST_DLL)]
        public static extern uint CryptCATClose(IntPtr hCatalog);

        [DllImport(WIN_TRUST_DLL, CharSet = CharSet.Unicode)]
        public static extern IntPtr CryptCATEnumerateCatAttr(IntPtr hCatalog, IntPtr pPrevAttr);

        [DllImport(WIN_TRUST_DLL, CharSet = CharSet.Unicode)]
        public static extern IntPtr CryptCATEnumerateMember(IntPtr hCatalog, IntPtr pPrevMember);

        [DllImport(WIN_TRUST_DLL, CharSet = CharSet.Unicode)]
        public static extern IntPtr CryptCATEnumerateAttr(IntPtr hCatalog, IntPtr pCatMember, IntPtr pPrevAttr);

        [DllImport(WIN_TRUST_DLL, CharSet = CharSet.Unicode)]
        public static extern IntPtr CryptCATStoreFromHandle(IntPtr hCatalog);

        [DllImport(WIN_TRUST_DLL, CharSet = CharSet.Unicode)]
        public static extern bool CryptCATAdminAcquireContext2(
            ref IntPtr phCatAdmin,
            IntPtr pgSubsystem,
            [MarshalAs(UnmanagedType.LPWStr)] string pwszHashAlgorithm,
            IntPtr pStrongHashPolicy,
            uint dwFlags
        );

        [DllImport(WIN_TRUST_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CryptCATAdminCalcHashFromFileHandle2(
            IntPtr hCatAdmin,
            IntPtr hFile,
            [In, Out] ref uint pcbHash,
            IntPtr pbHash,
            uint dwFlags
        );

        [DllImport(WIN_TRUST_DLL, CharSet = CharSet.Unicode)]
        public static extern bool CryptCATAdminReleaseContext(IntPtr phCatAdmin, uint dwFlags);

        [DllImport(WIN_TRUST_DLL, CharSet = CharSet.Unicode)]
        public static extern IntPtr CryptCATOpen(
            [MarshalAs(UnmanagedType.LPWStr)] string pwszFilePath,
            uint fdwOpenFlags,
            IntPtr hProv,
            uint dwPublicVersion,
            uint dwEncodingType
        );
    }
}