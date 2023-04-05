using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Utilizr.Info;
using Utilizr.Win32.Shell32;
using Utilizr.Win32.Shell32.Flags;

namespace Utilizr.Win.Info
{
    /// <summary>
    /// http://stackoverflow.com/a/21953690/1229237
    /// </summary>
    public static class KnownFolders
    {
        private static readonly string[] _knownFolderGuids = new string[]
        {
            "{56784854-C6CB-462B-8169-88E350ACB882}", // Contacts
            "{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}", // Desktop
            "{FDD39AD0-238F-46AF-ADB4-6C85480369C7}", // Documents
            "{374DE290-123F-4565-9164-39C4925E467B}", // Downloads
            "{1777F761-68AD-4D8A-87BD-30B759FA33DD}", // Favourites
            "{BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968}", // Links
            "{4BD8D571-6D19-48D3-BE97-422220080E43}", // Music
            "{33E28130-4E1E-4676-835A-98395C3BC3BB}", // Pictures
            "{4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4}", // SavedGames
            "{7D1D3A04-DEBB-4115-95CF-2F29DA2920DA}", // SavedSearches
            "{18989B1D-99B5-455B-841C-AB7C74E4DDFC}", // Videos
            "{82A5EA35-D9CD-47C5-9629-E15D2F714E6E}", // Common startup
        };

        /// <summary>
        /// Gets the current path to the specified known folder as currently configured. This does
        /// not require the folder to exist.
        /// </summary>
        /// <param name="knownFolder">The known folder which current path will be returned.</param>
        /// <returns>The default path of the known folder.</returns>
        /// <exception cref="ExternalException">Thrown if the path could not be retrieved.</exception>
        public static string GetPath(KnownFolder knownFolder)
        {
            return GetPath(knownFolder, false);
        }

        /// <summary>
        /// Gets the current path to the specified known folder as currently configured. This does
        /// not require the folder to be existent.
        /// </summary>
        /// <param name="knownFolder">The known folder which current path will be returned.</param>
        /// <param name="defaultUser">Specifies if the paths of the default user (user profile
        /// template) will be used. This requires administrative rights.</param>
        /// <returns>The default path of the known folder.</returns>
        /// <exception cref="ExternalException">Thrown if the path could not be retrieved.</exception>
        public static string GetPath(KnownFolder knownFolder, bool defaultUser)
        {
            return GetPath(knownFolder, KnownFolderFlags.DontVerify, defaultUser);
        }

        private static string GetPath(KnownFolder knownFolder, KnownFolderFlags flags, bool defaultUser)
        {
            int result = Shell32.SHGetKnownFolderPath(
                new Guid(_knownFolderGuids[(int)knownFolder]),
                (uint)flags,
                new IntPtr(defaultUser ? -1 : 0),
                out IntPtr outPath
            );

            if (result >= 0)
            {
                var path = Marshal.PtrToStringUni(outPath);
                if (!string.IsNullOrEmpty(path))
                    return path;
            }

            throw new ExternalException("Unable to retrieve the known folder path. It may not be available on this system.", result);
        }

        public static string SafeGetDownloadsFolder()
        {
            string downloadsFolder;
            try
            {
                downloadsFolder = GetPath(KnownFolder.Downloads);
            }
            catch (Exception)
            {
                var profilePath = Environment.ExpandEnvironmentVariables("%userprofile%");
                downloadsFolder = Path.Combine(profilePath, "Downloads");
            }
            return downloadsFolder;
        }

        /// <summary>
        /// Gets the 32 bit version of Program Files. Should be something like 
        /// C:\Program Files (x86) for 64 bit Windows and C:\Program Files for 
        /// 32 bit Windows. On 32 bit systems, this will return the same as
        /// <see cref="GetProgramFiles"/>
        /// </summary>
        public static string GetProgramFilesx86()
        {
            if (!Platform.Is64BitOS)
                return GetProgramFiles();

            var regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion");
            if (regKey != null)
            {
                var path = regKey.GetValue("ProgramFilesDir (x86)") as string;
                if (!string.IsNullOrEmpty(path))
                    return path;
            }

            //var path = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");
            //if (path.IsNotNullOrEmpty())
            //    return path;

            var sysDrive = Environment.ExpandEnvironmentVariables("%SystemDrive%");
            if (!string.IsNullOrEmpty(sysDrive))
                return Path.Combine(sysDrive, "Program Files (x86)");

            // Machine probably quite messed up if we got here...
            return "C:\\Program Files (x86)";
        }

        /// <summary>
        /// Get the Program Files directory, usually C:\Program Files.
        /// On 64 bit systems, this will be the 64 bit folder.
        /// On 32 bit, it will be the 32 bit folder.
        /// </summary>
        public static string GetProgramFiles()
        {
            var regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion");
            if (regKey != null)
            {
                var path = regKey.GetValue("ProgramFilesDir") as string;
                if (!string.IsNullOrEmpty(path))
                    return path;
            }

            // Fails on x64 Windows in x86 process, always returns (x86) folder
            //var path = Environment.ExpandEnvironmentVariables("%ProgramFiles%");
            //if (path.IsNotNullOrEmpty())
            //    return path;

            var sysDrive = Environment.ExpandEnvironmentVariables("%SystemDrive%");
            if (!string.IsNullOrEmpty(sysDrive))
                return Path.Combine(sysDrive, "Program Files");

            // Machine probably quite messed up if we got here...
            return "C:\\Program Files";
        }
    }


    /// <summary>
    /// Standard folders registered with the system. These folders are installed with Windows Vista
    /// and later operating systems, and a computer will have only folders appropriate to it
    /// installed.
    /// </summary>
    public enum KnownFolder
    {
        Contacts,
        Desktop,
        Documents,
        Downloads,
        Favorites,
        Links,
        Music,
        Pictures,
        SavedGames,
        SavedSearches,
        Videos,
        CommonStartup
    }
}