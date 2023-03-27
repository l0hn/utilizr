using System;
using System.Text;
using System.IO;
using Utilizr.Logging;
using Utilizr.Info;
using Utilizr.Win32.Msi;
using Utilizr.Win32.Msi.Flags;

namespace Utilizr.FileSystem
{
    /// <summary>
    /// http://astoundingprogramming.wordpress.com/2012/12/17/how-to-get-the-target-of-a-windows-shortcut-c/
    /// </summary>
    public class ShortcutHelper
    {
        public static string GetTargetPath(string filePath)
        {
            var targetPath = ResolveMsiShortcut(filePath);
            targetPath ??= ResolveShortcut(filePath);

            return targetPath;
        }

        static string? ResolveMsiShortcut(string file)
        {
            var product = new StringBuilder(InstallStateConsts.MaxGuidLength + 1);
            var feature = new StringBuilder(InstallStateConsts.MaxFeatureLength + 1);
            var component = new StringBuilder(InstallStateConsts.MaxGuidLength + 1);

            Msi.MsiGetShortcutTarget(file, product, feature, component);

            int pathLength = InstallStateConsts.MaxPathLength;
            var path = new StringBuilder(pathLength);

            var installState = Msi.MsiGetComponentPath(product.ToString(), component.ToString(), path, ref pathLength);

            if (installState == InstallState.Local)
            {
                return path.ToString();
            }

            return null;
        }

        static string ResolveShortcut(string filePath)
        {
            // IWshRuntimeLibrary is in the COM library "Windows Script Host Object Model"
            // IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            using dynamic shell = COMObject.CreateObject("WScript.Shell");
            try
            {
                // IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(filePath);
                dynamic shortcut = shell.CreateShortcut(filePath);

                if (!File.Exists(shortcut.TargetPath))
                    return shortcut.TargetPath.Replace(" (x86)", "");

                return shortcut.TargetPath;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Failed to get shortcut target");
                throw;
            }
        }

        public static bool CreateAllUsersStartupShortcut(string shortcutTarget)
        {
            return CreateShortcut(KnownFolders.GetPath(KnownFolder.CommonStartup), shortcutTarget);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="shortcutDir">Folder to contain the shortcut</param>
        /// <param name="shortcutTarget">Target of the shortcut</param>
        /// <param name="throwOnError">Whether exceptions are thrown on an error.</param>
        /// <returns>True if successfully created</returns>
        public static bool CreateShortcut(string shortcutDir, string shortcutTarget, bool throwOnError = false)
        {
            var shortcutPath = Path.Combine(shortcutDir, $"{Path.GetFileNameWithoutExtension(shortcutTarget)}.lnk");
            try
            {
                if (File.Exists(shortcutPath))
                {
                    if (throwOnError)
                        throw new Exception($"Unable to create shortcut, file '{shortcutPath}' already exists");

                    return false;
                }
                dynamic shell = COMObject.CreateObject("WScript.Shell");
                dynamic shortcut = shell.CreateShortcut(shortcutPath);

                shortcut.TargetPath = shortcutTarget;
                shortcut.WorkingDirectory = Path.GetDirectoryName(shortcutTarget);
                shortcut.Save();

                shell.Dispose();
                return true;
            }
            catch (Exception e)
            {
                Log.Exception(nameof(ShortcutHelper), e, "Create startup shortcut failed");

                if (throwOnError)
                    throw;
            }

            return false;
        }

        public static bool DeleteAllUsersStartupShortcut(string shortcutPath)
        {
            try
            {
                string allUsersStartupDirectory = KnownFolders.GetPath(KnownFolder.CommonStartup);

                File.Delete(allUsersStartupDirectory + $"\\{Path.GetFileNameWithoutExtension(shortcutPath)}.lnk");
                return true;
            }
            catch (Exception e)
            {
                Log.Exception(nameof(ShortcutHelper), e, "Delete startup shortcut failed");
            }

            return false;
        }
    }
}