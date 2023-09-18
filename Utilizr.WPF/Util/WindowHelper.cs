using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using Utilizr.Win32.Shell32;
using Utilizr.Win32.Shell32.Flags;
using Utilizr.Win32.User32;
using Utilizr.Win32.User32.Flags;
using Utilizr.Win32.User32.Structs;

namespace Utilizr.WPF.Util
{
    public static class WindowHelper
    {
        public static void BringToFront(Window window)
        {
            IntPtr windowHandle = new WindowInteropHelper(window).Handle;
            User32.SetForegroundWindow(windowHandle);
        }

        public static Window GetActiveWindow()
        {
            IntPtr active = User32.GetActiveWindow();

            if (active == IntPtr.Zero)
            {
                return Application.Current.MainWindow;
            }
            else
            {
                var activeWindow = Application.Current.Windows.OfType<Window>()
                    .SingleOrDefault(window => new WindowInteropHelper(window).Handle == active);

                return activeWindow!;
            }
        }

        public static bool HasDetectedFullScreenApp()
        {
            try
            {
                Shell32.SHQueryUserNotificationState(out var state);

                return state != UserNotificationState.AcceptsNotifications &&
                       state != UserNotificationState.QuietTime;
            }
            catch (Exception)
            {
                return IsForegroundWindowFullScreen(out _);
            }
        }

        /// <summary>
        /// Whether the foreground window on the specified screen is using all screen space.
        /// Any error assumes the screen is not occupying all screen space.
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="processName"></param>
        public static bool IsForegroundWindowFullScreen(out string processName)
        {
            processName = string.Empty;

            var hWndForeground = User32.GetForegroundWindow();
            if (hWndForeground == IntPtr.Zero)
                return false;

            try
            {
                User32.GetWindowThreadProcessId(hWndForeground, out uint processID);
                var proc = Process.GetProcessById((int)processID);
                processName = proc.ProcessName;

                if (processName.ToLowerInvariant() == "explorer")
                {
                    return false;
                }
            }
            catch
            {

            }

            if (!User32.GetWindowRect(hWndForeground, out NativeRect rect))
                return false;

            var hMonitor = User32.MonitorFromWindow(hWndForeground, MonitorFromWindowFlags.DEFAULT_TO_NEAREST);
            if (hMonitor == IntPtr.Zero)
                return false;

            var monitorInfo = new NativeMonitorInfo();
            User32.GetMonitorInfo(hMonitor, monitorInfo);

            var screenWidth = (monitorInfo.Monitor.Right - monitorInfo.Monitor.Left);
            var screenHeight = (monitorInfo.Monitor.Bottom - monitorInfo.Monitor.Top);

            if (screenWidth <= (rect.Right - rect.Left) && screenHeight <= (rect.Bottom - rect.Top))
                return true;

            return false;
        }

        public static bool ApplicationIsActive()
        {
            var activatedHandle = User32.GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
                return false; // No window is currently activated

            var procId = Process.GetCurrentProcess().Id;
            User32.GetWindowThreadProcessId(activatedHandle, out uint activeProcId);

            return activeProcId == procId;
        }
    }
}
