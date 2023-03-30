namespace Utilizr.Win32.Kernel32.Flags
{
    public static class ResourceTypes
    {
        /// <summary>
        /// Accelerator table.
        /// </summary>
        public const uint RT_ACCELERATOR = 9;

        /// <summary>
        /// Animated cursor.
        /// </summary>
        public const uint RT_ANICURSOR = 21;

        /// <summary>
        /// Animated icon.
        /// </summary>
        public const uint RT_ANIICON = 22;

        /// <summary>
        /// Bitmap resource.
        /// </summary>
        public const uint RT_BITMAP = 2;

        /// <summary>
        /// Hardware-dependent cursor resource.
        /// </summary>
        public const uint RT_CURSOR = 1;

        /// <summary>
        /// Dialog box.
        /// </summary>
        public const uint RT_DIALOG = 5;

        /// <summary>
        /// Allows a resource editing tool to associate a string with an.rc file.Typically, the string is the
        /// name of the header file that provides symbolic names. The resource compiler parses the string but
        /// otherwise ignores the value. For example, 1 DLGINCLUDE "MyFile.h"
        /// </summary>
        public const uint RT_DLGINCLUDE = 17;

        /// <summary>
        /// Font resource.
        /// </summary>
        public const uint RT_FONT = 8;

        /// <summary>
        /// Font directory resource.
        /// </summary>
        public const uint RT_FONTDIR = 7;

        /// <summary>
        /// Hardware-independent cursor resource.
        /// </summary>
        public const uint RT_GROUP_CURSOR = RT_CURSOR + 11;

        /// <summary>
        /// Hardware-independent icon resource.
        /// </summary>
        public const uint RT_GROUP_ICON = RT_ICON + 11;

        /// <summary>
        /// HTML resource.
        /// </summary>
        public const uint RT_HTML = 23;

        /// <summary>
        /// Hardware-dependent icon resource.
        /// </summary>
        public const uint RT_ICON = 3;

        /// <summary>
        /// Side-by-Side Assembly Manifest.
        /// </summary>
        public const uint RT_MANIFEST = 24;

        /// <summary>
        /// Menu resource.
        /// </summary>
        public const uint RT_MENU = 4;

        /// <summary>
        /// Message-table entry.
        /// </summary>
        public const uint RT_MESSAGETABLE = 11;

        /// <summary>
        /// Plug and Play resource.
        /// </summary>
        public const uint RT_PLUGPLAY = 19;

        /// <summary>
        /// Application-defined resource(raw data).
        /// </summary>
        public const uint RT_RCDATA = 10;

        /// <summary>
        /// String-table entry.
        /// </summary>
        public const uint RT_STRING = 6;

        /// <summary>
        /// Version resource.
        /// </summary>
        public const uint RT_VERSION = 16;

        /// <summary>
        /// VXD.
        /// </summary>
        public const uint RT_VXD = 20;
    }
}
