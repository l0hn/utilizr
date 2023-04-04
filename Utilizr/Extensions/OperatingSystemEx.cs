using System;

namespace Utilizr.Extensions
{
    public static class OperatingSystemEx
    {
        public static string ToHuman(this OperatingSystem os)
        {
            var osMajor = os.Version.Major;
            var osMinor = os.Version.Minor;
            var osBuild = os.Version.Build;

            if (osMajor >= 10)
            {
                if (osBuild >= 22000) // 11 10.0.22000
                {
                    return string.IsNullOrEmpty(os.ServicePack)
                        ? "Microsoft Windows 11"
                        : $"Microsoft Windows 11 {os.ServicePack}";
                }

                return string.IsNullOrEmpty(os.ServicePack)
                    ? "Microsoft Windows 10"
                    : $"Microsoft Windows 10 {os.ServicePack}";
            }

            if (osMajor >= 6)
            {
                if (osMinor >= 3) // 8.1 6.3
                {
                    return string.IsNullOrEmpty(os.ServicePack)
                        ? "Microsoft Windows 8.1"
                        : $"Microsoft Windows 8.1 {os.ServicePack}";
                }

                if (osMinor >= 2) // 8.0 6.2
                {
                    return string.IsNullOrEmpty(os.ServicePack)
                        ? "Microsoft Windows 8"
                        : $"Microsoft Windows 8 {os.ServicePack}";
                }

                if (osMinor >= 1) // 7 6.1
                {
                    return string.IsNullOrEmpty(os.ServicePack)
                        ? "Microsoft Windows 7"
                        : $"Microsoft Windows 7 {os.ServicePack}";
                }

                // Vista 6.0
                return string.IsNullOrEmpty(os.ServicePack)
                    ? "Microsoft Windows Vista"
                    : $"Microsoft Windows Vista {os.ServicePack}";
            }

            if (osMajor >= 5)
            {
                if (osMajor >= 1) // XP 5.1
                {
                    return string.IsNullOrEmpty(os.ServicePack)
                        ? "Microsoft Windows XP"
                        : $"Microsoft Windows XP {os.ServicePack}";
                }

                // 2000 5.0
                return string.IsNullOrEmpty(os.ServicePack)
                    ? "Microsoft Windows 2000"
                    : $"Microsoft Windows 2000 {os.ServicePack}";
            }

            return string.Empty;
        }
    }
}
