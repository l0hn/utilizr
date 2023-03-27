using System;
using System.Runtime.InteropServices;
using static Utilizr.Win32.Crypt32.Crypt32;

namespace Utilizr.Win32.Crypt32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SIGNER_SUBJECT_INFO
    {
        /// DWORD->unsigned int
        public uint cbSize;

        /// DWORD*
        public IntPtr pdwIndex;

        /// DWORD->unsigned int
        public uint dwSubjectChoice;

        /// SubjectChoiceUnion
        public SubjectChoiceUnion Union1;
    }
}
