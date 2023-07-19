using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Utilizr.FileSystem
{
    public static class DirectoryHelper
    {
        public delegate void OnError(string filePath, Exception error);
        public delegate string[] DirectoryFiltererDelegate(string[] directories);

        /// <summary>
        /// For each directory in the directory tree rooted at the directoryPath given, yield a DirectoryResult which
        /// contains the current path and a string[] of files and a string[] of directories
        /// 
        /// When topDown is true, the caller can change the directories property of the resulting DirectoryResult,
        /// either by replacing it or by modifying it in place, and walk will only recurse into the subdirectories
        /// whose paths remain in the string[]. This can be used to prune the search, or to impose a specific order
        /// of visiting.  Modifying the directories property when topDown is false is ineffective.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="topDown">When topDown is true or not specified, the result is yielded before the results of any 
        /// if its subdirectories (directories are generated top down). If topDown is false, the results are yielded
        /// after all results from subdirectories have been yielded (directories are generated bottom up).</param>
        /// <param name="onError">By default the errors from listing directories are ignored. If optional argument
        /// onError is supplied it will be called with the current folder path an instance of the Exception that occured.
        /// It can report the error to continue with the walk, or raise the exception to abort the walk.</param>
        /// <returns></returns>
        public static IEnumerable<DirectoryResult> Walk(
            string directoryPath,
            bool topDown = true,
            OnError? onError = null)
        {
            return Walk(directoryPath, "*", topDown, onError);
        }

        public static IEnumerable<DirectoryResult> Walk(
            string directoryPath,
            string searchPattern,
            bool topDown = true,
            OnError? onError = null,
            DirectoryFiltererDelegate? directoryFilterer = null)
        {

            string[] files;
            string[] directories;

            try
            {
                files = Directory.GetFiles(directoryPath, searchPattern);
                directories = Directory.GetDirectories(directoryPath);
            }
            catch (Exception error)
            {
                onError?.Invoke(directoryPath, error);
                yield break;
            }

            directoryFilterer ??= dirs => dirs.Where(dir =>
            {
                var dirName = Path.GetFileName(dir);
                return string.IsNullOrEmpty(dirName?.Trim());
            }).ToArray();
            directories = directoryFilterer(directories);

            var directoryResult = new DirectoryResult(directoryPath, directories, files);

            if (topDown)
                yield return directoryResult;

            foreach (var subDirectoryResult in directoryResult.Directories.SelectMany(path => Walk(path, searchPattern, topDown, onError)))
            {
                yield return subDirectoryResult;
            }

            if (!topDown)
                yield return directoryResult;
        }

        /// <summary>
        /// Recursive directory copy helper. NOTE: this method will not copy any .ds_store files
        /// </summary>
        /// <param name="sourceDirectory">Source directory.</param>
        /// <param name="destinationDirectory">Destination directory.</param>
        /// <param name="onlyCopyIfAlreadyExists">If set to <c>true</c> only copy if already exists.</param>
        /// <param name="recursive">If set to <c>true</c> recursive.</param>
        /// <param name="ignoreFolderNames">Ignore folder names.</param>
        public static void CopyDirectoryContents(
            string sourceDirectory,
            string destinationDirectory,
            bool onlyCopyIfAlreadyExists = false,
            bool recursive = false,
            params string[] ignoreFolderNames)
        {
            CopyDirectoryContents(
                sourceDirectory,
                destinationDirectory,
                onlyCopyIfAlreadyExists,
                recursive,
                ignoreFolderNames: ignoreFolderNames,
                ignoreFileExtensions: null
            );
        }

        /// <summary>
        /// Recursive directory copy helper. NOTE: this method will not copy any .ds_store files
        /// </summary>
        /// <param name="sourceDirectory">Source directory.</param>
        /// <param name="destinationDirectory">Destination directory.</param>
        /// <param name="onlyCopyIfAlreadyExists">If set to <c>true</c> only copy if already exists.</param>
        /// <param name="recursive">If set to <c>true</c> recursive.</param>
        /// <param name="ignoreFolderNames">Ignore folder names.</param>
        /// <param name="ignoreFileExtensions">File without a matching extension will not be copied.</param>
        public static void CopyDirectoryContents(
            string sourceDirectory,
            string destinationDirectory,
            bool onlyCopyIfAlreadyExists,
            bool recursive,
            string[] ignoreFolderNames,
            string[]? ignoreFileExtensions)
        {
            if (!Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            foreach (var file in Directory.GetFiles(sourceDirectory))
            {
                var fileExt = file.ToLower();
                if (fileExt.EndsWith(".ds_store") || ignoreFileExtensions?.Any(p => p.Equals(fileExt, StringComparison.OrdinalIgnoreCase)) == true)
                    continue;

                var destinationFile = Path.Combine(destinationDirectory, Path.GetFileName(file));

                if (onlyCopyIfAlreadyExists && !File.Exists(destinationFile))
                    continue;

                File.Copy(file, destinationFile, true);
            }

            if (!recursive)
                return;

            foreach (var dir in Directory.GetDirectories(sourceDirectory))
            {
                if (ignoreFolderNames?.Any(i => i.Equals(Path.GetFileName(dir), StringComparison.InvariantCultureIgnoreCase)) == true)
                    continue;

                var destination = Path.Combine(destinationDirectory, Path.GetFileName(dir));
                Directory.CreateDirectory(destination);
                CopyDirectoryContents(dir, destination, onlyCopyIfAlreadyExists, recursive);
            }
        }
    }
}
