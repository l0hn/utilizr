namespace Utilizr.Win32.Kernel32.Flags
{
    public enum FileCreationDispositionFlags : uint
    {
        CREATE_NEW =                        0x1,
        CREATE_ALWAYS =                     0x2,
        OPEN_EXISTING =                     0x3,
        OPEN_ALWAYS =                       0x4,
        TRUNCATE_EXISTING =                 0x5,
    }
}
