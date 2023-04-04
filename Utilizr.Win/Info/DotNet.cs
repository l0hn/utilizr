using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilizr.Logging;

namespace Utilizr.Win.Info
{
    public static class DotNet
    {
        /// <summary>
        /// List .net frameworks found on a user's system.
        /// </summary>
        /// <param name="logErrorCallback">Optional callback to log exceptions to remove logging depedency</param>
        /// <returns></returns>
        public static IEnumerable<string> InstalledFrameworks()
        {
            // Uses examples from https://msdn.microsoft.com/en-us/library/hh925568(v=vs.110).aspx
            var frameworks = new List<string>();
            frameworks.AddRange(InstalledFrameworksUpTo45().Where(p => !string.IsNullOrEmpty(p)));
            frameworks.AddRange(InstalledFrameworksFrom45AndAbove().Where(p => !string.IsNullOrEmpty(p)));
            return frameworks;
        }

        static IEnumerable<string> InstalledFrameworksUpTo45()
        {
            var installed = new List<string>();
            try
            {
                using var ndpKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\");
                if (ndpKey == null)
                    return installed;

                foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                {
                    if (!versionKeyName.StartsWith("v"))
                        continue;

                    var versionKey = ndpKey.OpenSubKey(versionKeyName);
                    if (versionKey == null)
                        continue;

                    string? version = (string?)versionKey.GetValue("Version", string.Empty);
                    string? servicePack = versionKey.GetValue("SP", string.Empty)?.ToString();
                    string? install = versionKey.GetValue("Install", string.Empty)?.ToString();

                    if (!string.IsNullOrEmpty(install)) //if no install info, must be later.
                    {
                        if (!string.IsNullOrEmpty(servicePack) && install == "1")
                            installed.Add($"{versionKeyName} {version} SP{servicePack}");
                    }

                    if (!string.IsNullOrEmpty(version))
                        continue;

                    foreach (string subKeyName in versionKey.GetSubKeyNames())
                    {
                        var subKey = versionKey.OpenSubKey(subKeyName);
                        if (subKey == null)
                            continue;

                        version = (string?)subKey.GetValue("Version", string.Empty);

                        if (!string.IsNullOrEmpty(version))
                            servicePack = subKey.GetValue("SP", string.Empty)?.ToString();

                        install = subKey.GetValue("Install", string.Empty)?.ToString();

                        if (string.IsNullOrEmpty(install)) //no install info, must be later.
                        {
                            installed.Add($"{versionKeyName} {version}");
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(servicePack) && install == "1")
                            {
                                installed.Add($"{versionKeyName} {subKeyName} {version} SP{servicePack}");
                            }
                            else if (install == "1")
                            {
                                installed.Add($"{versionKeyName} {subKeyName} {version}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }

            return installed;
        }

        static IEnumerable<string> InstalledFrameworksFrom45AndAbove()
        {
            var installed = new List<string>();
            try
            {
                const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
                using var ndpKey = Registry.LocalMachine.OpenSubKey(subkey);
                if (ndpKey == null || ndpKey.GetValue("Release") == null)
                    return installed;

                var releaseValue = ndpKey.GetValue("Release");
                if (releaseValue == null)
                    return installed;

                int releaseVersion = Convert.ToInt32(releaseValue);
                if (releaseVersion >= 533320)
                    installed.Add("v4.8.1+");
                else if (releaseVersion >= 528040)
                    installed.Add("v4.8");
                else if (releaseVersion >= 461808)
                    installed.Add("v4.7.2");
                else if (releaseVersion >= 461308)
                    installed.Add("v4.7.1");
                else if (releaseVersion >= 460798)
                    installed.Add("v4.7");
                else if (releaseVersion >= 394802)
                    installed.Add("v4.6.2");
                else if (releaseVersion >= 394254)
                    installed.Add("v4.6.1");
                else if (releaseVersion >= 393295)
                    installed.Add("v4.6");
                else if (releaseVersion >= 379893)
                    installed.Add("v4.5.2");
                else if (releaseVersion >= 378675)
                    installed.Add("v4.5.1");
                else if (releaseVersion >= 378389)
                    installed.Add("v4.5");
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
            return installed;
        }
    }
}