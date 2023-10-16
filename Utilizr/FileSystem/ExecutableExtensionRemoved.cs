using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Utilizr.FileSystem
{
    public static class ExecutableExtensionRemover
    {
        // Not safe to use Path.GetFileNameWithoutExtension(), FileInfo, etc, or remove after last '.'.
        // E.g. 'f.lux' application will show incorrectly as 'f'.
        // Hence get all executable extension types, and remove if path ends with matched extension.

        static readonly object EXT_LOCK = new();

        private static List<string>? _fileExtensions;
        public static List<string> FileExtensions
        {
            get
            {
                lock (EXT_LOCK)
                {
                    if (_fileExtensions == null)
                    {
                        SetExecutableExtensionsWindows();
                    }

                    return _fileExtensions!;
                }
            }
        }


        public static string GetFileNameWithoutExecutableExtension(string file)
        {
            foreach (string ext in FileExtensions)
            {
                if (file.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                    return file.Substring(0, file.Length - ext.Length);
            }

            return file;
        }

        static void SetExecutableExtensionsWindows()
        {
            var execStrs = Environment.GetEnvironmentVariable("PATHEXT");

            if (string.IsNullOrEmpty(execStrs))
            {
                _fileExtensions = new List<string>() { ".exe", ".lnk", ".cpl" };
                return;
            }

            var extArray = execStrs.Split(Path.PathSeparator);
            _fileExtensions = new List<string>
            {
                ".lnk", // shortcut won't be in PATHTEXT environment variable
                ".cpl" // same for control panel items
            };
            _fileExtensions.AddRange(extArray);
        }

        public static bool NamesMatchWithoutExecutableExtension(string fileA, string fileB)
        {
            return NamesMatchWithoutExecutableExtension(fileA, fileB, true);
        }

        public static bool NamesMatchWithoutExecutableExtension(string fileA, string fileB, bool ignoreCase)
        {
            string aNoExecExt = GetFileNameWithoutExecutableExtension(fileA);
            string bNoExecExt = GetFileNameWithoutExecutableExtension(fileB);

            return ignoreCase
                ? aNoExecExt.Equals(bNoExecExt)
                : aNoExecExt.Equals(bNoExecExt, StringComparison.OrdinalIgnoreCase);
        }

        public static string GetExecutablePathWithoutArguments(string filePathWithPossibleArgs, params string[] extraFileExtensions)
        {
            var extensions = new List<string>();
            extensions.AddRange(FileExtensions);
            if (extraFileExtensions?.Any() == true)
                extensions.AddRange(extraFileExtensions);

            char[] dotTrim = new char[] { '.' };
            var allExecutableExtensionsPattern = string.Join(
                "|",
                extensions.Select(p => $@".*\.{p.TrimStart(dotTrim)}'?""?").ToArray()
            );

            string regexString = $@"^(?<exe>{allExecutableExtensionsPattern}) ?(?<args>.*)";
            var regex = new Regex(regexString, RegexOptions.IgnoreCase);
            var result = regex.Match(filePathWithPossibleArgs);
            string pathNoArgs = result.Groups[1].Value.Replace("\"", string.Empty);
            return pathNoArgs;
        }
    }
}
