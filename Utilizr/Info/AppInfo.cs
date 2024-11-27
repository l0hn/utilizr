using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Utilizr.Extensions;


namespace Utilizr.Info
{
    /// <summary>
    /// Determine the root location for any of the app's folders.
    /// </summary>
    public enum AppInfoRoot
    {
        /// <summary>
        /// Inside app's install directory (default)
        /// </summary>
        InstallDirectory,

        /// <summary>
        /// Use the user local appdata folder (or the C:\ProgramData folder for non-user processes).
        /// </summary>
        AppData,

        /// <summary>
        /// Always use non-user specific location of C:\ProgramData.
        /// </summary>
        ProgramData,
    }

    public static class AppInfo
    {
        public static string? AppName { get; set; }

        private static string? _appDirectory;
        /// <summary>
        /// Directory of executing assembly (install directory) and will not differ depending on <see cref="DirectoryRoot"/>.
        /// </summary>
        public static string AppDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_appDirectory))
                {
                    _appDirectory = AppContext.BaseDirectory;
                }
                return _appDirectory!;
            }
        }

        /// <summary>
        /// Specify the root for any of the folders specified on properties within this class.
        /// See <see cref="AppInfoRoot"/> for valid values.
        /// </summary>
        public static AppInfoRoot DirectoryRoot { get; set; }

        private static string? _directoryRootPath;
        /// <summary>
        /// Root path for supporting folders (/logs, /data, etc). Will differ depending on <see cref="DirectoryRoot"/>.
        /// Note: Not the same as <see cref="AppDirectory"/>!
        /// </summary>
        public static string DirectoryRootPath
        {
            get
            {
                CreateAppDirectory(ref _directoryRootPath, "");
                return _directoryRootPath!;
            }
        }

        private static string? _urlDataDirectory;
        public static string UrlDataDirectory
        {
            get
            {
                CreateAppDirectory(ref _urlDataDirectory, "url_data");
                return _urlDataDirectory!;
            }
        }

        private static string? _libsDirectory;
        public static string LibsDirectory
        {
            get
            {
                CreateAppDirectory (ref _libsDirectory, "libs");
                return _libsDirectory!;
            }
        }

        private static string? _dataDirectory;
        public static string DataDirectory
        {
            get
            {
                CreateAppDirectory(ref _dataDirectory, "data");
                return _dataDirectory!;
            }
        }

        private static string? _queueDirectory;
        public static string QueueDirectory
        {
            get
            {
                CreateAppDirectory(ref _queueDirectory, "queues");
                return _queueDirectory!;
            }
        }

        private static string? _logDirectory;
        public static string LogDirectory
        {
            get
            {
                CreateAppDirectory(ref _logDirectory, "logs");
                return _logDirectory!;
            }
        }

        private static string? _updatesDirectory;
        public static string UpdatesDirectory
        {
            get
            {
                CreateAppDirectory(ref _updatesDirectory, "updates");
                return _updatesDirectory!;
            }
        }

        private static string? _cacheDir;
        public static string CacheDir
        {
            get
            {
                CreateAppDirectory(ref _cacheDir, "cache");
                return _cacheDir!;
            }
        }


        private static void CreateAppDirectory(ref string? directory_string, string dir_name)
        {
            if (string.IsNullOrEmpty(directory_string))
            {
                directory_string = GetAppDirectory(dir_name);
            }

            if (!Directory.Exists(directory_string))
            {
                Directory.CreateDirectory(directory_string);
            }
        }

        private static string GetAppDirectory(string dirName)
        {
            return GetAppDirectory(dirName, DirectoryRoot);
        }

#if WINDOWS
        private static string GetAppDirectory(string dirName, AppInfoRoot rootDir)
        {
            if (string.IsNullOrEmpty(AppName))
                throw new InvalidOperationException($"Unable to get window's directory with null/empty '{nameof(AppName)}' for '{dirName}' with root '{rootDir}'");

            static string programData(string appName, string dirName)
            {
                return Path.Combine(Environment.ExpandEnvironmentVariables("%PROGRAMDATA%"), appName, dirName);
            }

            if (rootDir == AppInfoRoot.AppData)
            {
                if (!Environment.UserInteractive) //is most likely a service
                {
                    return programData(AppName, dirName);
                }
                return Path.Combine(Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%"), AppName, dirName);
            }
            else if (rootDir == AppInfoRoot.ProgramData)
            {
                return programData(AppName, dirName);
            }

            //AppInfoRoot.InstallDirectory
            return Path.Combine(AppDirectory, dirName);
        }
#else
        private static string GetAppDirectory(string dirName, AppInfoRoot rootDir) {
            if (AppName.IsNullOrEmpty())
            {
                throw new InvalidOperationException($"AppName must be specified before calling {nameof(GetAppDirectory)}");
            }
            var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(userHome, "Library", "Application Support", AppName, dirName);
        }
#endif


        public static void ZipLogs(string destinationFile)
        {
            ZipLogs(destinationFile, Array.Empty<ZipLogsAdditionalItem>(), Array.Empty<ZipLogsAdditionalItem>());
        }

        public static void ZipLogs(string destinationFile, ZipLogsAdditionalItem[] additionalDirs, ZipLogsAdditionalItem[] additionalFiles) 
        {
            if (File.Exists(destinationFile))
            {
                File.Delete(destinationFile);
            }

            using (var fs = new FileStream(destinationFile, FileMode.Create, FileAccess.ReadWrite))
            using (var zip = new ZipArchive(fs, ZipArchiveMode.Create, true))
            {
                var enumOptions = new EnumerationOptions() { RecurseSubdirectories = true };

                void addFolderToZip(ZipLogsAdditionalItem folder)
                {
                    //var filesToAdd = Directory.EnumerateFiles(folder.Path, "*", enumOptions);
                    var filesToAdd = Directory.GetFiles(folder.Path, "*", SearchOption.AllDirectories);
                    foreach (var file in filesToAdd)
                    {
                        try
                        {
                            // failsafe to remove the start of the path
                            var relativeToFolderFilePath = file.Substring(folder.Path.Length).TrimStart(Path.DirectorySeparatorChar);
                            var nameInZip = Path.Combine(folder.PathInArchive, relativeToFolderFilePath);
                            //zip.CreateEntryFromFile(file, nameInZip);

                            var archiveEntry = zip.CreateEntry(nameInZip);
                            using var archiveStream = archiveEntry.Open();
                            using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            fs.CopyTo(archiveStream);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to add {file} to archive {destinationFile}. Error = {ex.Message}");
                        }
                    }
                }

                addFolderToZip(new ZipLogsAdditionalItem { Path = LogDirectory, PathInArchive = "logs" });
                addFolderToZip(new ZipLogsAdditionalItem { Path = DataDirectory, PathInArchive = "data" });

                foreach (var additionalDirectory in additionalDirs)
                {
                    try
                    {
                        if (!Directory.Exists(additionalDirectory.Path))
                            continue;

                        addFolderToZip(additionalDirectory);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error adding additional directory '{additionalDirectory.Path}' to archive: {ex.Message}");
                    }
                }

                foreach (var additionalFile in additionalFiles)
                {
                    try
                    {
                        if (!File.Exists(additionalFile.Path))
                            continue;

                        zip.CreateEntryFromFile(additionalFile.Path, additionalFile.PathInArchive);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error adding additional file '{additionalFile.Path}' to archive: {ex.Message}");
                    }
                }
            }

            //using var zip = new ZipFile(destinationFile);
            //zip.AlternateEncoding = Encoding.UTF8;
            //zip.AlternateEncodingUsage = ZipOption.AsNecessary;

            //zip.AddDirectory(LogDirectory, "logs");
            //zip.AddDirectory(DataDirectory, "data");

            //foreach (var additionalDir in additionalDirs)
            //{
            //    try
            //    {
            //        if (!Directory.Exists(additionalDir.Path))
            //            continue;

            //        zip.AddDirectory(additionalDir.Path, additionalDir.PathInArchive);
            //    }
            //    catch (Exception ex)
            //    {
            //        System.Diagnostics.Debug.WriteLine($"Error adding additional directory '{additionalDir.Path}' to archive: {ex.Message}");
            //    }
            //}

            //foreach (var additionalFile in additionalFiles)
            //{
            //    try
            //    {
            //        if (!File.Exists(additionalFile.Path))
            //            continue;

            //        zip.AddFile(additionalFile.Path, additionalFile.PathInArchive);
            //    }
            //    catch (Exception ex)
            //    {
            //        System.Diagnostics.Debug.WriteLine($"Error adding additional file '{additionalFile.Path}' to archive: {ex.Message}");
            //    }
            //}

            //zip.Save();
        }
    }

    public struct ZipLogsAdditionalItem
    {
        public string Path;
        public string PathInArchive;
    }
}