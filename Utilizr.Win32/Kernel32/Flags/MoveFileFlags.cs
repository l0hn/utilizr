using System;

namespace Utilizr.Win32.Kernel32.Flags;

[Flags]
public enum MoveFileFlags : uint
{
    MOVEFILE_REPLACE_EXISTING = 0x1,
    MOVEFILE_COPY_ALLOWED = 0x2,
    MOVEFILE_DELAY_UNTIL_REBOOT = 0x4,
    MOVEFILE_WRITE_THROUGH = 0x8
}