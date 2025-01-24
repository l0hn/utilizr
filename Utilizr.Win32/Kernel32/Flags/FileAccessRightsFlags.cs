namespace Utilizr.Win32.Kernel32.Flags
{
    public enum FileAccessRightsFlags : uint
    {
        GENERIC_ALL =               0x10000000,
        GENERIC_EXECUTE =           0x20000000,
        GENERIC_WRITE =             0x40000000,
        GENERIC_READ =              0x80000000,
    }
}
