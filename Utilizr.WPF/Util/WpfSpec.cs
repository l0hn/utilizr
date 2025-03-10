using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Utilizr.Logging;

namespace Utilizr.WPF.Util
{
    public static class WpfSpec
    {
        public static bool IsHardwareRendering => RenderCapability.Tier >> 16 > 0;
        public static bool IsFullHardwareRendering => RenderCapability.Tier >> 16 > 1;

        public static void SetDefaultFrameRateForSystemSpec()
        {
            try
            {
                if (IsFullHardwareRendering)
                    return;

                var frameRate = IsHardwareRendering ? 40 : 30;

                Timeline.DesiredFrameRateProperty.OverrideMetadata(
                    typeof(Timeline),
                    new FrameworkPropertyMetadata() { DefaultValue = frameRate }
                );
            }
            catch (Exception ex)
            {
                Log.Exception("wpf_spec", ex);
            }
        }

        /// <summary>
        /// <see cref="IsHardwareRendering"/> and <see cref="IsFullHardwareRendering"/> uses <see cref="RenderCapability.Tier"/> which
        /// is purely hardware based - does this machine have the hardware capable of hardware acceleration. However, WPF may still be
        /// using software only rendering even when the machine is capable. The precedence order for software rendering is:
        /// - DisableHWAcceleration registry key
        /// - ProcessRenderMode (whole WPF process)
        /// - RenderMode per-target (window)
        /// This function checks all of the above in an attempt to detect software only rendering.
        /// </summary>
        /// <param name="window">Optional window to use when checking per target. Defaults to MainWindow.</param>
        /// <returns>False if software rendering detected, null on error, otherwise true.</returns>
        public static bool? IsUsingHardwareRendering(Window? window = null)
        {
            if (!IsHardwareRendering)
                return false; // no DirectX 9 support

            // window level
            var win = window ?? Application.Current.MainWindow;
            if (win == null)
            {
                Log.Warning(nameof(WpfSpec), "Unable to check per-target for software rendering due to null window.");
            }
            else
            {
                var hwndSource = PresentationSource.FromVisual(window) as HwndSource;
                var hwndTarget = hwndSource?.CompositionTarget;
                if (hwndTarget?.RenderMode == RenderMode.SoftwareOnly)
                    return false;
            }

            // wpf process level
            if (RenderOptions.ProcessRenderMode == RenderMode.SoftwareOnly)
                return false;

            // machine wide level
            try
            {
                var regKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Avalon.Graphics");
                var disabled = regKey?.GetValue("DisableHWAcceleration") as int?;
                if (disabled == 1)
                    return false; // WPF disabled system wide
            }
            catch (Exception ex)
            {
                Log.Exception(nameof(WpfSpec), ex, "Unable to check registry for WPF hardware rendering");
                return null;
            }

            return true;
        }
    }
}
