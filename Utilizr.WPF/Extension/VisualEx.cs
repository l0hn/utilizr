using System;
using System.Windows;
using System.Windows.Media;

namespace Utilizr.WPF.Extension
{
    public static class VisualEx
    {
        public static double GetDpi(this Visual v)
        {
            try
            {
                var dpiScale = VisualTreeHelper.GetDpi(v);
                //System.Diagnostics.Debug.WriteLine($"*** {dpiScale.PixelsPerDip} ***");
                return dpiScale.PixelsPerDip;
            }
            catch (Exception)
            {
                // fail safe default
                // Default DPI of 96 in WPF, but this is how many pixels per that DPI
                // thus default is 1. Shame docs cannot also say that...
                return 1;
            }
        }

        public static bool RegisterDpiChanged(this Visual v, Action onChange)
        {
            var mainVm = Application.Current.MainWindow;
            if (mainVm == null)
                return false;

            mainVm.DpiChanged += (s, e) => onChange?.Invoke();
            return true;
        }
    }
}