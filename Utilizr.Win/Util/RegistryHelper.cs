using Microsoft.Win32;
using System;
using System.Text;
using Utilizr.Logging;


namespace Utilizr.Win.Util
{
    public static class RegistryHelper
    {
        /// <summary>
        /// Defaults to bitness view of the OS
        /// </summary>
        public static RegistryKey? GetKeyFromPath(string path, string userSID = "", bool isWritable = false)
        {
            bool isCurrentUserOrUsersHive;
            var key = GetHiveFromPath(path, out string uri, out isCurrentUserOrUsersHive);

            if (isCurrentUserOrUsersHive)
            {
                if (string.IsNullOrEmpty(userSID))
                    throw new InvalidOperationException("UserSID cannot be null when accessing HKCU under Local_System");

                key = OpenCurrentUserKey(userSID, isWritable);
            }

            var result = key?.OpenSubKey(uri, isWritable);
            return result; //null if path doesn't exist
        }

        /// <summary>
        /// Defaults to bitness view of the OS
        /// </summary>
        public static RegistryKey? OpenCurrentUserKey(string sid = "", bool isWritable = false)
        {
            if (string.IsNullOrEmpty(sid))
                return Registry.CurrentUser;

            return Registry.Users.OpenSubKey(sid, isWritable);
        }

        private static RegistryKey? GetHiveFromPath(string path, out string uri, out bool isCurrentUserOrUsersHive)
        {
            isCurrentUserOrUsersHive = false;

            if (path.StartsWith(@"HKEY_CLASSES_ROOT\", StringComparison.OrdinalIgnoreCase))
            {
                uri = path.Replace(@"HKEY_CLASSES_ROOT\", string.Empty);
                return Registry.ClassesRoot;
            }
            else if (path.StartsWith(@"HKEY_CURRENT_USER\", StringComparison.OrdinalIgnoreCase))
            {
                uri = path.Replace(@"HKEY_CURRENT_USER\", string.Empty);
                isCurrentUserOrUsersHive = true;
                return Registry.CurrentUser;
            }
            else if (path.StartsWith(@"HKEY_LOCAL_MACHINE\", StringComparison.OrdinalIgnoreCase))
            {
                uri = path.Replace(@"HKEY_LOCAL_MACHINE\", string.Empty);
                return Registry.LocalMachine;
            }
            else if (path.StartsWith("HKEY_USERS", StringComparison.OrdinalIgnoreCase))
            {
                string[] sections = path.Split('\\');
                var sb = new StringBuilder();

                for (int i = 2; i < sections.Length; i++)
                {
                    sb.Append(string.Format("{0}\\", sections[i]));
                }

                uri = sb.ToString();
                isCurrentUserOrUsersHive = true;
                return Registry.CurrentUser;
            }

            uri = string.Empty;
            return null;
        }


        public static RegistryValueKind GetRegistryValueKind(string typeStr)
        {
            return typeStr.ToLower() switch
            {
                "binary" => RegistryValueKind.Binary,
                "dword" => RegistryValueKind.DWord,
                "expandstring" => RegistryValueKind.ExpandString,
                "multistring" => RegistryValueKind.MultiString,
                "qword" => RegistryValueKind.QWord,
                "string" => RegistryValueKind.String,
                _ => RegistryValueKind.Unknown,
            };
        }

        public static object ConvertToCorrectType(string value, string registryType)
        {
            RegistryValueKind dataType = GetRegistryValueKind(registryType);
            return ConvertToCorrectType(value, dataType);
        }

        public static object ConvertToCorrectType(string value, RegistryValueKind regType)
        {
            switch (regType)
            {
                case RegistryValueKind.Binary:
                    string[] strBytes = value.Split('-');
                    byte[] bytes = new byte[strBytes.Length];
                    for (int i = 0; i < strBytes.Length; i++)
                    {   //TODO: Not hardcode as hex
                        bytes[i] = Convert.ToByte(strBytes[i], 16);
                    }
                    return bytes;
                case RegistryValueKind.DWord:
                    return Convert.ToInt32(value);
                case RegistryValueKind.QWord:
                    return Convert.ToInt64(value);
                case RegistryValueKind.MultiString:
                    return value.Split(',');
                case RegistryValueKind.String:
                case RegistryValueKind.ExpandString:
                case RegistryValueKind.Unknown:
                default:
                    return value;
            }
        }

        public static string GetDataString(RegistryKey key, string valueName)
        {
            RegistryValueKind dataType = key.GetValueKind(valueName);
            string result = string.Empty;

            switch (dataType)
            {
                case RegistryValueKind.Binary:
                    if (key.GetValue(valueName) is byte[] binary)
                        result = BitConverter.ToString(binary);
                    break;
                case RegistryValueKind.ExpandString:
                    var expandStringVal = key.GetValue(valueName, result, RegistryValueOptions.DoNotExpandEnvironmentNames)?.ToString();
                    if (expandStringVal != null)
                        result = expandStringVal;
                    break;
                case RegistryValueKind.MultiString:
                    if (key.GetValue(valueName) is string[] multi)
                        result = string.Join(",", multi);
                    break;
                case RegistryValueKind.DWord:
                case RegistryValueKind.QWord:
                case RegistryValueKind.String:
                case RegistryValueKind.Unknown:
                default:
                    var unkownVal = key.GetValue(valueName)?.ToString();
                    if (unkownVal != null)
                        result = unkownVal;
                    break;
            }

            return result.ToLowerInvariant();
        }

        public static string? GetUserExplorerShellFolder(string keyName)
        {
            try
            {
                const string path = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";
                using (var key = Registry.CurrentUser.OpenSubKey(path, false))
                {
                    if (key != null)
                    {
                        return key.GetValue(keyName)?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception(nameof(RegistryHelper), ex);
            }

            return string.Empty;
        }
    }
}