using System;

namespace Utilizr.Win32.Kernel32.Flags
{
    public static class ProcessCreationFlags
    {
        public const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;
        public const uint CREATE_NEW_CONSOLE = 0x00000010;
        public const uint CREATE_NEW_PROCESS_GROUP = 0x00000200;
        public const uint CREATE_NO_WINDOW = 0x08000000;
        public const uint CREATE_BREAKAWAY_FROM_JOB = 0x01000000;
        public const uint CREATE_PROTECTED_PROCESS = 0x00040000;
        public const uint CREATE_SUSPENDED = 0x00000004;
    }
}
