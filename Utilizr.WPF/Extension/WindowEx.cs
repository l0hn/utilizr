using System;
using System.Windows;
using System.Windows.Interop;
using Utilizr.Win32.User32;

namespace Utilizr.WPF.Extension
{
    public static class WindowEx
    {
        public static void OpenWindowAndBringIntoFocus(this Window window, Action<Exception>? errorCallback = null)
        {
            try
            {
                if (window.WindowState == WindowState.Minimized)
                    window.WindowState = WindowState.Normal;

                window.Show();

                var winInterop = new WindowInteropHelper(window);
                User32.SetForegroundWindow(winInterop.Handle);
            }
            catch (Exception ex)
            {
                errorCallback?.Invoke(ex);
            }

            // Try old method, too. Win32 approach not always working, apparently
            try
            {
                window.Show();
                window.Activate();
                window.WindowState = WindowState.Normal;
                window.Topmost = true;
                window.Topmost = false;
                window.Focus();
            }
            catch (Exception innerEx)
            {
                errorCallback?.Invoke(innerEx);
            }
        }
    }
}