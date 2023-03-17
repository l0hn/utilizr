using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Utilizr.Globalisation.Helpers
{
    public static class SourceList
    {
        private static readonly string[] _extensions = new string[]{
            ".cs",
            ".h",
            ".m",
            ".kt"//kotlin
        };

        public static long GenerateSourceList(string srcRootDirectory, string outputFilePath)
        {
            int count = 0;
            using (var writer = new StreamWriter(File.Open(outputFilePath, FileMode.Create, FileAccess.Write)))
            {
                foreach (string file in GetSourceFiles(srcRootDirectory))
                {
                    writer.WriteLine(file);
                    Console.WriteLine($"file: {file}");
                    count++;
                }
            }
            return count;
        }

        private static IEnumerable<string> GetSourceFiles(string directory)
        {
            foreach (var file in Directory.EnumerateFiles(directory))
            {
                if (!_extensions.Contains(Path.GetExtension(file)))
                    continue;

                yield return file;
            }

            foreach (string dir in Directory.GetDirectories(directory))
            {
                foreach (string file in GetSourceFiles(dir))
                {
                    yield return file;
                }
            }
        }
    }
}
