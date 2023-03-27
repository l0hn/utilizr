using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using Utilizr.Win32.WinTrust;
using Utilizr.Win32.WinTrust.Flags;
using Utilizr.Win32.WinTrust.Structs;

namespace Utilizr.Crypto
{
    public static class AuthenticodeTools
    {
        public static bool IsCatalogFile(string fileName)
        {
            using var sh = new SafeFileHandle(new IntPtr(-1), true);
            return WinTrust.IsCatalogFile(sh, fileName);
        }

        public static bool IsTrusted(string fileName)
        {
            return WinVerifyTrust(fileName) == 0;
        }

        private static uint WinVerifyTrust(string fileName)
        {
            var wintrust_action_generic_verify_v2 = new Guid("{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}");
            uint result = 0;
            using (var fileInfo = new WINTRUST_FILE_INFO(fileName, Guid.Empty))
            using (var guidPtr = new UnmanagedPointer(Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Guid))), AllocMethod.HGlobal))
            using (var wvtDataPtr = new UnmanagedPointer(Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WINTRUST_DATA))), AllocMethod.HGlobal))
            {
                var data = new WINTRUST_DATA(fileInfo);
                IntPtr pGuid = guidPtr;
                IntPtr pData = wvtDataPtr;
                Marshal.StructureToPtr(wintrust_action_generic_verify_v2, pGuid, true);
                Marshal.StructureToPtr(data, pData, true);
                result = WinTrust.WinVerifyTrust(IntPtr.Zero, pGuid, pData);
            }

            return result;
        }
    }
}
