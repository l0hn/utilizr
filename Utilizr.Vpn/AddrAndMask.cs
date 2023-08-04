using System;
using System.Runtime.InteropServices;

namespace Utilizr.Vpn
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ADDR_AND_MASK
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string szIpAddr;

        [MarshalAs(UnmanagedType.LPStr)]
        public string szMask;
    }
}