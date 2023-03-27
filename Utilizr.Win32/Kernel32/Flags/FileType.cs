namespace Utilizr.Win32.Kernel32.Flags
{
    public enum FileType : uint
    {
        Char = 0x0002,
        Disk = 0x0001,
        Pipe = 0x0003,
        Remote = 0x8000,
        Unknown = 0x0000,
    }
}
