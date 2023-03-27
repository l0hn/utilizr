using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Crypt32.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    public struct SubjectChoiceUnion
    {
        /// SIGNER_FILE_INFO*
        [FieldOffset(0)]
        public IntPtr pSignerFileInfo;

        /// SIGNER_BLOB_INFO*
        [FieldOffset(0)]
        public IntPtr pSignerBlobInfo;
    }
}
