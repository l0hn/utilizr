using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Utilizr.Win32.Msi.Flags;

namespace Utilizr.Win32.Msi
{
    public static class Msi
    {
        const string MSI_DLL = "msi.dll";

        [DllImport(MSI_DLL, CharSet = CharSet.Auto)]
        public static extern uint MsiGetShortcutTarget(string targetFile, StringBuilder productCode, StringBuilder featureID, StringBuilder componentCode);

        [DllImport(MSI_DLL, CharSet = CharSet.Auto)]
        public static extern InstallState MsiGetComponentPath(string productCode, string componentCode, StringBuilder componentPath, ref int componentPathBufferSize);
    }
}
