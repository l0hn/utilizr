using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Utilizr.Win32.Kernel32.Flags;

namespace Utilizr.Win32.Kernel32
{
    public static class ResourceHelper
    {
        /// <summary>
        /// Load a resource from an exe / dll
        /// </summary>
        /// <param name="resourceName">Name of the resource to load</param>
        /// <param name="fromFile">Path to the exe / dll. Leave null to use currently executing assembly</param>
        /// <param name="resourceType">type of resource e.g. ICON RCDATA etc..</param>
        /// <returns></returns>
        public static byte[]? LoadResourceFile(string resourceName, string? fromFile = null, uint resourceType = ResourceTypes.RT_RCDATA)
        {
            if (string.IsNullOrEmpty(fromFile))
                fromFile = Process.GetCurrentProcess().MainModule?.FileName;

            IntPtr hMod = Kernel32.LoadLibraryEx(fromFile!, IntPtr.Zero, LoadLibraryFlags.LOAD_LIBRARY_AS_DATAFILE);
            if (hMod == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            IntPtr hRes = Kernel32.FindResource(hMod, resourceName, resourceType);
            if (hRes == IntPtr.Zero)
                return null;

            uint size = Kernel32.SizeofResource(hMod, hRes);
            if (size == 0)
                return null;

            IntPtr pt = Kernel32.LoadResource(hMod, hRes);
            if (pt == IntPtr.Zero)
                return null;

            byte[] bPtr = new byte[size];
            Marshal.Copy(pt, bPtr, 0, (int)size);
            return bPtr;
        }
    }
}
