using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Utilizr.FileSystem;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using Utilizr.Win32.Msi;

namespace Utilizr.WPF.Util
{
    public static class IconExtractor
    {
        /// <summary>
        /// Extracts an uninstaller's icon, where the path may or may not contain arguments.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="backupIconUriPath">Backup image URI to use if the icon cannot be extracted from the file</param>
        /// <returns></returns>
        public static byte[] GetInstallerIcon(string? filePath, Uri? backupIconUri)
        {
            if (string.IsNullOrEmpty(filePath))
                return GetIcon(null, backupIconUri);

            filePath = Environment.ExpandEnvironmentVariables(filePath);
            string pathNoArgs = ExecutableExtensionRemover.GetExecutablePathWithoutArguments(filePath, ".ico");

            //TODO: Temp hack, find better method
            if (pathNoArgs.Equals("msiexec.exe", StringComparison.OrdinalIgnoreCase))
            {
                string msiIconStr = GetMsiProductIconString(filePath);
                pathNoArgs = string.IsNullOrEmpty(msiIconStr)
                    ? Path.Combine(Environment.SystemDirectory, "MsiExec.exe")
                    : msiIconStr;
            }

            //Noticed some paths contain %APPDATA% etc...
            pathNoArgs = Environment.ExpandEnvironmentVariables(pathNoArgs);

            return GetIcon(pathNoArgs, backupIconUri);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">If null or empty, the backup icon will be returned.</param>
        /// <param name="backupIconUri">Optional backup icon if an error retrieving the specified icon.</param>
        /// <returns></returns>
        public static byte[] GetIcon(string? path, Uri? backupIconUri = null)
        {
            byte[] backupIcon()
            {
                try
                {
                    if (backupIconUri == null)
                        return Array.Empty<byte>();

                    //Return default icon
                    var img = BitmapFrame.Create(backupIconUri, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                    byte[] data;
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(img);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        encoder.Save(ms);
                        data = ms.ToArray();
                    }
                    return data;
                }
                catch
                {
                    return Array.Empty<byte>();
                }
            }

            try
            {
                if (string.IsNullOrEmpty(path))
                    return backupIcon();

                var iconIndex = path.IndexOf(',');
                var safePath = iconIndex > 0
                    ? path.Substring(0, iconIndex)
                    : path;

                var icon = Icon.ExtractAssociatedIcon(safePath);
                if (icon == null)
                    return backupIcon();

                var bmp = icon.ToBitmap();
                byte[] imageData;
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Png);
                    imageData = ms.ToArray();
                }

                return imageData;
            }
            catch (Exception)
            {
                return backupIcon();
            }
        }

        private static string GetMsiProductIconString(string uninstallString)
        {
            var regexString = @".*(?<guid>{[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}})";
            var regex = new Regex(regexString, RegexOptions.IgnoreCase);
            var result = regex.Match(uninstallString);
            string guid = result.Groups[1].Value;
            int len = 512;
            var builder = new StringBuilder(len);

            try
            {
                Msi.MsiGetProductInfo(guid, "ProductIcon", builder, ref len); //"InstallLocation" "InstallProductName"
                return builder.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
