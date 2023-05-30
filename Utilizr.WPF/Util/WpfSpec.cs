using System;
using System.Windows;
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
    }
}
