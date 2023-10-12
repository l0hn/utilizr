using System.IO;

namespace Utilizr.Win32.Kernel32.Flags
{
    public enum FileAttributeFlags : int
    {
        INVALID =                    -1,
        READONLY =                   0x00000001,
        HIDDEN =                     0x00000002,
        SYSTEM =                     0x00000004,
        DIRECTORY =                  0x00000010,
        ARCHIVE =                    0x00000020,
        DEVICE =                     0x00000040,
        NORMAL =                     0x00000080,
        TEMPORARY =                  0x00000100,
        SPARSE_FILE =                0x00000200,
        REPARSE_POINT =              0x00000400,
        COMPRESSED =                 0x00000800,
        OFFLINE =                    0x00001000,
        NOT_CONTENT_INDEXED =        0x00002000,
        ENCRYPTED =                  0x00004000,
    }


    public static class FileAttributeFlagsHelper
    {
        public static FileAttributes ToFileAttributes(this int attrFlags)
        {
            var attrs = (FileAttributes)0;

            if ((attrFlags & (int)FileAttributeFlags.ARCHIVE) != 0)
                attrs |= FileAttributes.Archive;

            if ((attrFlags & (int)FileAttributeFlags.COMPRESSED) != 0)
                attrs |= FileAttributes.Compressed;

            if ((attrFlags & (int)FileAttributeFlags.DEVICE) != 0)
                attrs |= FileAttributes.Device;

            if ((attrFlags & (int)FileAttributeFlags.DIRECTORY) != 0)
                attrs |= FileAttributes.Directory;

            if ((attrFlags & (int)FileAttributeFlags.ENCRYPTED) != 0)
                attrs |= FileAttributes.Encrypted;

            if ((attrFlags & (int)FileAttributeFlags.HIDDEN) != 0)
                attrs |= FileAttributes.Hidden;

            if ((attrFlags & (int)FileAttributeFlags.NORMAL) != 0)
                attrs |= FileAttributes.Normal;

            if ((attrFlags & (int)FileAttributeFlags.NOT_CONTENT_INDEXED) != 0)
                attrs |= FileAttributes.NotContentIndexed;

            if ((attrFlags & (int)FileAttributeFlags.OFFLINE) != 0)
                attrs |= FileAttributes.Offline;

            if ((attrFlags & (int)FileAttributeFlags.READONLY) != 0)
                attrs |= FileAttributes.ReadOnly;

            if ((attrFlags & (int)FileAttributeFlags.REPARSE_POINT) != 0)
                attrs |= FileAttributes.ReparsePoint;

            if ((attrFlags & (int)FileAttributeFlags.SPARSE_FILE) != 0)
                attrs |= FileAttributes.SparseFile;

            if ((attrFlags & (int)FileAttributeFlags.SYSTEM) != 0)
                attrs |= FileAttributes.System;

            if ((attrFlags & (int)FileAttributeFlags.TEMPORARY) != 0)
                attrs |= FileAttributes.Temporary;

            return attrs;
        }
    }
}
