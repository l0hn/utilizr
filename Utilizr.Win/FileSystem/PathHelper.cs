﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Utilizr.Extensions;
using Utilizr.Logging;
using Utilizr.Win32.Kernel32;
using Utilizr.Win32.Kernel32.Flags;
using Utilizr.Win32.Kernel32.Structs;
using Utilizr.Win32.Shell32;

namespace Utilizr.Win.FileSystem
{
    public static class PathHelper
    {
        const string _uncPrefix = @"\\?\";
        const int _maxPath = 260;


        /// <summary>
        /// Create a UNC equivalent for the specified path.
        /// </summary>
        /// <param name="source">Original directory.</param>
        /// <param name="optionalSuffix">Optionally add to the end of the path, e.g. '*'</param>
        /// <returns></returns>
        public static string UncFullPath(string source, string? optionalSuffix = null)
        {
            if (source.StartsWith(_uncPrefix) || IsUncPath(source))
                return optionalSuffix == null
                    ? source
                    : Path.Combine(source, optionalSuffix);

            // Don't include environment variables
            source = Environment.ExpandEnvironmentVariables(source);

            return optionalSuffix == null
                ? $"{_uncPrefix}{source}"
                : $"{_uncPrefix}{Path.Combine(source, optionalSuffix)}";
        }


        public static bool IsUncPath(string path)
        {
            return Uri.TryCreate(path, UriKind.Absolute, out Uri? uri) && uri.IsUnc;
        }


        /// <summary>
        /// Same as Path.GetPathRoot() but safe on long paths
        /// </summary>
        /// <returns></returns>
        public static string? GetPathRoot(string? path)
        {
            // Path.GetPathRoot(null) returns null
            if (path == null)
                return path;

            // Path.GetPathRoot(string.Empty) throws ArgumentException
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException($"The {nameof(path)} is not of legal form");

            var vIndex = path.IndexOf(Path.VolumeSeparatorChar);
            if (vIndex == -1)
                return string.Empty;

            var root = $"{path.Substring(0, vIndex)}{Path.VolumeSeparatorChar}{Path.DirectorySeparatorChar}";

            // remove any potential UNC prefix
            return root.TrimStart(@"\\?\");
        }


        /// <summary>
        /// Same functionality as Path.GetDirectoryName but safe on long paths,
        /// will not throw PathTooLongException.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static string GetDirectoryName(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                return directory;

            var dirSepIndex = directory.LastIndexOf(Path.DirectorySeparatorChar);
            if (dirSepIndex >= 0)
                return directory[..dirSepIndex];

            var volSepIndex = directory.IndexOf(Path.VolumeSeparatorChar);
            if (volSepIndex >= 0)
                return directory[..volSepIndex];

            return string.Empty;
        }


        /// <summary>
        /// Same functionality as Path.GetFileName but safe on long paths,
        /// will not throw PathTooLongException.
        public static string? GetFileName(string filePath)
        {
            var trimmed = filePath?.TrimEnd(Path.DirectorySeparatorChar);
            var dirCharIndex = trimmed?.LastIndexOf(Path.DirectorySeparatorChar);
            if (dirCharIndex == null)
                return null;

            if (dirCharIndex < 0)
            {
                if (trimmed?.Length > 0 == true)
                    return trimmed;

                return null;
            }

            var result = trimmed?.Substring(dirCharIndex.Value);
            return result?.TrimStart(Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Increments filename with numbers, only returning when the file path doesn't exist.
        /// The same path will be returned to that provided if the file doesn't already exist.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string DistinctlyNumberedFileName(string filePath)
        {
            var ext = Path.GetExtension(filePath); // handles long paths
            var filePathNoExtension = filePath;
            if (ext.Length < filePathNoExtension.Length)
                filePathNoExtension = filePathNoExtension.Substring(0, filePathNoExtension.Length - ext.Length);

            int count = 1;
            while (FileExists(filePath))
            {
                filePath = $"{filePathNoExtension} - ({count}){ext}";
                count++;
            }

            return filePath;
        }

        /// <summary>
        /// Make an absolute path a subdirectory to the provided parameter.
        /// E.g. F:\Homework\Maths\test.txt => C:\Users\student\Desktop\F\Homework\Maths\test.txt
        /// </summary>
        /// <param name="subPathRoot">The absolute path to a folder which will contain the subdirectory.</param>
        /// <param name="absolutePath">The absolute path that will become a subdirectory within <paramref name="subPathRoot"/></param>
        /// <returns></returns>
        public static string AbsolutePathToSubdirectory(string subPathRoot, string absolutePath)
        {
            var root = GetPathRoot(absolutePath);
            var rootAsFolder = root.TrimEnd(new char[] { Path.VolumeSeparatorChar, Path.DirectorySeparatorChar });

            var combined = Path.Combine(subPathRoot, rootAsFolder);

            if (root.Length < absolutePath.Length)
            {
                combined = Path.Combine(combined, absolutePath.Substring(root.Length));
                combined = UncFullPath(combined);
            }

            return combined;
        }

        /// <summary>
        /// Equivalent of File.Exists() but can accept long paths. Converts to UNC, safe if already UNC path.
        /// </summary>
        public static bool FileExists(string file)
        {
            file = UncFullPath(file);

            if (!Kernel32.GetFileAttributesEx(file, GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out FILE_ATTRIBUTE_DATA fData))
                return false;

            return fData.dwFileAttributes != (int)FileAttributeFlags.INVALID &&
                   (fData.dwFileAttributes & (int)FileAttributeFlags.DIRECTORY) == 0;
        }

        /// <summary>
        /// Equivalent of Directory.Exists() but can accept long paths. Converts to UNC, safe if already UNC path.
        /// </summary>
        public static bool DirectoryExists(string directory)
        {
            directory = UncFullPath(directory);

            if (!Kernel32.GetFileAttributesEx(directory, GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out FILE_ATTRIBUTE_DATA fData))
                return false;

            return fData.dwFileAttributes != (int)FileAttributeFlags.INVALID &&
                   (fData.dwFileAttributes & (int)FileAttributeFlags.DIRECTORY) != 0;
        }


        /// <summary>
        /// Must be disposed after finished. Converts to UNC only if not already a UNC path.
        /// </summary>
        public static SafeFileHandle GetCreateFileForWrite(
            string file,
            FileShareRightsFlags shareRights = FileShareRightsFlags.FILE_SHARE_NONE)
        {
            file = UncFullPath(file);

            var hFile = Kernel32.CreateFileW(
                file,
                FileAccessRightsFlags.GENERIC_ALL,
                shareRights,
                IntPtr.Zero,
                FileCreationDispositionFlags.CREATE_NEW,
                FileAttributeFlags.NORMAL,
                IntPtr.Zero
            );

            if (hFile.ToInt32() < 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return new SafeFileHandle(hFile, true);
        }

        /// <summary>
        /// Equivalent of Directory.CreateDirectory() but can accept long paths. Converts to UNC only if not already a UNC path.
        /// </summary>
        public static void CreateDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentNullException(nameof(directory));

            var directoryNames = directory.TrimStart(@"\\?\").Split(Path.DirectorySeparatorChar);
            string path = string.Empty;
            foreach (var dName in directoryNames)
            {
                if (string.IsNullOrEmpty(dName))
                    continue;

                path = string.IsNullOrEmpty(path)
                    ? UncFullPath($"{dName}{Path.DirectorySeparatorChar}")
                    : Path.Combine(path, dName);

                if (DirectoryExists(path))
                    continue;

                if (!Kernel32.CreateDirectoryW(path, IntPtr.Zero))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), $"Failed to create directory {directory}");
            }
        }


        /// <summary>
        /// Highlights a file or folder within Windows's explorer. Handles long paths unlike
        /// using the /select argument with explorer.exe.
        /// </summary>
        /// <param name="folder">Parent folder of the items.</param>
        /// <param name="items">Individual folder and file names to select in the containing folder.
        /// Can be empty if only wanting to show folder without selecting items.</param>
        public static void ShowItemsInExplorer(string folder, params string[] items)
        {
            try
            {
                Shell32.SHParseDisplayName(folder, IntPtr.Zero, out IntPtr nativeFolder, 0, out uint psfgaoOut);

                if (nativeFolder == IntPtr.Zero)
                    return; // folder not found

                var nativeItems = new List<IntPtr>();
                foreach (var item in items)
                {
                    IntPtr nativeItem = IntPtr.Zero;
                    Shell32.SHParseDisplayName(Path.Combine(folder, item), IntPtr.Zero, out nativeItem, 0, out psfgaoOut);

                    if (nativeItem == IntPtr.Zero)
                        continue; // not found in folder

                    nativeItems.Add(nativeItem);
                }

                Shell32.SHOpenFolderAndSelectItems(nativeFolder, (uint)nativeItems.Count, nativeItems.ToArray(), 0);

                Marshal.FreeCoTaskMem(nativeFolder);
                foreach (var item in nativeItems)
                {
                    Marshal.FreeCoTaskMem(item);
                }
            }
            catch (Exception ex)
            {
                Log.Exception(nameof(PathHelper), ex);
            }
        }


        /// <summary>
        /// Enumerate child folders for the given directory supporting long paths.
        /// </summary>
        public static IEnumerable<string> GetChildFolders(string sourceDirectory, bool includeReparsePoints = false)
        {
            return GetChildPaths(sourceDirectory, true, includeReparsePoints);
        }

        /// <summary>
        /// Enumerate child files for the given directory supporting long paths.
        /// </summary>
        public static IEnumerable<string> GetChildFiles(string sourceDirectory, bool includeReparsePoints = false)
        {
            return GetChildPaths(sourceDirectory, false, includeReparsePoints);
        }

        static IEnumerable<string> GetChildPaths(string sourceDirectory, bool dirsOnly, bool includeReparsePoints)
        {
            var data = OpenPath(UncFullPath(sourceDirectory, "*"), out IntPtr handle);
            do
            {
                if (!includeReparsePoints && (data.dwFileAttributes & (int)FileAttributeFlags.REPARSE_POINT) == (int)FileAttributeFlags.REPARSE_POINT)
                    continue;

                bool isDir = (data.dwFileAttributes & (int)FileAttributeFlags.DIRECTORY) == (int)FileAttributeFlags.DIRECTORY;

                // avoid listening current and parent folder
                if (isDir && (data.cFileName.EndsWith(".") || data.cFileName.EndsWith("..")))
                    continue;

                if (isDir && dirsOnly) // dir
                    yield return Path.Combine(sourceDirectory, data.cFileName);

                if (!isDir && !dirsOnly) // file
                    yield return Path.Combine(sourceDirectory, data.cFileName);

            } while (Kernel32.FindNextFileW(handle, out data));

            OpenPathFinished(handle);
        }

        /// <summary>
        /// Get FIND_DATAW struct returned from FindFirstFileW.
        /// Important to call <see cref="OpenPathFinished(IntPtr)"/> when done.
        /// </summary>
        public static WIN32_FIND_DATAW OpenPath(string path, out IntPtr handle)
        {
            var data = new WIN32_FIND_DATAW();
            handle = Kernel32.FindFirstFileW(UncFullPath(path), out data);

            if (handle == new IntPtr(-1))
            {
                var error = Marshal.GetLastWin32Error();
                var win32 = new Win32Exception(error, $"Unable to open {path} with native error code {error}.");

                if (error == 5) // access denied
                    throw new UnauthorizedAccessException($"Unable to open {path}", win32);

                throw win32;
            }

            return data;
        }

        /// <summary>
        /// Close the handle from a path opened with <see cref="OpenPathFinished(IntPtr)"/>.
        /// </summary>
        /// <param name="handle"></param>
        public static void OpenPathFinished(IntPtr handle)
        {
            if (handle == new IntPtr(-1))
                return;

            Kernel32.FindClose(handle);
        }

        /// <summary>
        /// Calculates the total size of all files, and the number of all files within the given directory.
        /// Supports long file paths.
        /// </summary>
        /// <param name="sourceDirectory">Source directory.</param>
        /// <param name="fileCount">Variable that will contain the file count.</param>
        /// <param name="fileByteCount">Variable that will contain the sum of all file sizes.</param>
        /// <returns>Indicates whether one or more files were not included in the totals due to an error</returns>
        public static bool CalculateChildren(string sourceDirectory, out long fileCount, out long fileByteCount)
        {
            fileCount = 0;
            fileByteCount = 0;
            bool allFilesIncluded = true;

            var childDirs = new List<string>();
            var data = OpenPath(UncFullPath(sourceDirectory, "*"), out IntPtr hFile);

            do
            {
                if ((data.dwFileAttributes & (int)FileAttributeFlags.DIRECTORY) != (int)FileAttributeFlags.DIRECTORY)
                {
                    fileCount++;
                    fileByteCount += data.GetSize();
                }
                else if (data.cFileName != "." && data.cFileName != "..")
                {
                    childDirs.Add(Path.Combine(sourceDirectory, data.cFileName));
                }

            } while (Kernel32.FindNextFileW(hFile, out data));

            Kernel32.FindClose(hFile);

            foreach (var childDir in childDirs)
            {
                try
                {
                    // don't short circuit
                    allFilesIncluded = CalculateChildren(childDir, out long childDirFileCount, out long childDirFileByteCount) && allFilesIncluded;
                    fileCount += childDirFileCount;
                    fileByteCount += childDirFileByteCount;
                }
                catch (Exception ex)
                {
                    allFilesIncluded = false;

                    Log.Exception(
                        LoggingLevel.INFO,
                        ex,
                        $"Unable to include {sourceDirectory} in {nameof(fileCount)} and {nameof(fileByteCount)} totals."
                    );
                }
            }

            return allFilesIncluded;
        }
    }
}
