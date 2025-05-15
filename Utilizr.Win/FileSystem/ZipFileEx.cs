using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Utilizr.Win.FileSystem
{
    public static class ZipFileEx
    {
        public delegate void ExtractProgressFileCountUpdate(long filesExtracted, long filesInArchive, double percentageExtractedCount);
        public delegate void ExtractProgressFileSizeUpdate(long sizeExtracted, long uncompressedTotalSize, double percentageExtractedSize);

        /// <summary>
        /// Extract a zip to a given location, preserving any relative folders within the archive to the destination directory.
        /// Creates any necessary folders to ensure extraction does not fail.
        /// </summary>
        /// <param name="zipArchive">An opened ZipArchive with read permissions.</param>
        /// <param name="extractDestination">The destination folder where to place the extracted archive.</param>
        /// <param name="countProgressCallback">Optional progress callback with file counts, percentage based on these values.</param>
        /// <param name="sizeProgressCallback">Optional progress callback with file sizes, percentage based on these values.</param>
        public static void ExtractToDirectory(
            this ZipArchive zipArchive,
            string extractDestination,
            bool overwriteDestinationFiles,
            ExtractProgressFileCountUpdate? countProgressCallback = null,
            ExtractProgressFileSizeUpdate? sizeProgressCallback = null)
        {
            double safeSizeExtractedPercent = 0;
            long uncompressedSizeNowExtracted = 0;
            long uncompressedSize = Math.Max(zipArchive.Entries.Sum(p => p.Length), 1); // avoid divide by 0

            long filesNowExtracted = 0;
            long filesToExtract = zipArchive.Entries.Count;
            double extractedCountPercent = 0;

            foreach (var zipEntry in zipArchive.Entries)
            {
                // Observed folders within an archive using unix style '/' rather than '\' for Windows directory separator character
                var entryDestination = Path.Combine(extractDestination, zipEntry.FullName).Replace('/', '\\');

                if (!zipEntry.Name.Equals(zipEntry.FullName))
                {
                    var folder = PathHelper.GetDirectoryName(entryDestination);
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                }

                zipEntry.ExtractToFile(entryDestination, overwriteDestinationFiles);

                filesNowExtracted++;
                extractedCountPercent = filesNowExtracted / (double)filesToExtract;
                countProgressCallback?.Invoke(filesNowExtracted, filesToExtract, extractedCountPercent);

                uncompressedSizeNowExtracted += zipEntry.Length;
                var sizePercentage = uncompressedSizeNowExtracted / (double)uncompressedSize;
                safeSizeExtractedPercent = Math.Max(sizePercentage, safeSizeExtractedPercent); // ensure we're returning something sensible
                sizeProgressCallback?.Invoke(uncompressedSizeNowExtracted, uncompressedSize, safeSizeExtractedPercent);
            }
        }
    }
}
