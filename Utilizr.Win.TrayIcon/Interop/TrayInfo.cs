// Some interop code taken from Mike Marshall's AnyForm

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Media;
using Utilizr.Logging;
using Utilizr.WPF.Util;

namespace Utilizr.Win.TrayIcon.Interop
{
    /// <summary>
    /// Resolves the current tray position.
    /// </summary>
    public static class TrayInfo
    {
        /// <summary>
        /// Gets the position of the system tray.
        /// </summary>
        /// <returns>Tray coordinates.</returns>
        public static Point GetTrayLocation(Visual v)
        {
            // Occasionally doesn't seem to get the correct position, although it's quite rare.
            // Notification is shown over the task bar in these circumstances. Try up to 3 times
            // if an error occurred or the coordinates don't add up.

            const int retryLimit = 3;
            int retryCount = 0;
            int x = 0;
            int y = 0;

            AppBarInfo appBarInfo = null;
            Rectangle rcWorkArea = new Rectangle();
            bool isAutoHiddenBar = false;
            for (retryCount = 0; retryCount < retryLimit; retryCount++)
            {
                try
                {
                    int paddingFromTaskBar = 0;
                    appBarInfo = new AppBarInfo();
                    appBarInfo.GetSystemTaskBarPosition(out isAutoHiddenBar);

                    rcWorkArea = appBarInfo.WorkArea;
                    string procName = string.Empty;
                    if (WindowHelper.IsForegroundWindowFullScreen(out procName)) paddingFromTaskBar = -rcWorkArea.Height;

                    if (appBarInfo.Edge == AppBarInfo.ScreenEdge.Left)
                    {
                        if (rcWorkArea.Right <= 0)
                            continue; // incorrect

                        // X cannot be 0, otherwise notification appears on the monitor to the left of 
                        // the taskbar, rather than on the primary monitor to the right of the taskbar.
                        x = isAutoHiddenBar
                            ? 1 + paddingFromTaskBar // Don't move right.
                            : rcWorkArea.Right + paddingFromTaskBar;
                        y = rcWorkArea.Bottom;
                    }
                    else if (appBarInfo.Edge == AppBarInfo.ScreenEdge.Bottom)
                    {
                        if (rcWorkArea.Bottom - rcWorkArea.Height <= 0)
                            continue; // incorrect

                        x = rcWorkArea.Right;
                        y = isAutoHiddenBar
                            ? rcWorkArea.Bottom - paddingFromTaskBar // Don't move up
                            : rcWorkArea.Bottom - rcWorkArea.Height - paddingFromTaskBar;
                    }
                    else if (appBarInfo.Edge == AppBarInfo.ScreenEdge.Top)
                    {
                        if (rcWorkArea.Top + rcWorkArea.Height <= 0)
                            continue; // incorrect

                        x = rcWorkArea.Right;
                        y = isAutoHiddenBar
                            ? rcWorkArea.Top + paddingFromTaskBar // Don't move down
                            : rcWorkArea.Top + rcWorkArea.Height + paddingFromTaskBar;
                    }
                    else if (appBarInfo.Edge == AppBarInfo.ScreenEdge.Right)
                    {
                        if (rcWorkArea.Right - rcWorkArea.Width <= 0)
                            continue; // incorrect

                        x = isAutoHiddenBar
                            ? rcWorkArea.Right - paddingFromTaskBar // Don't move left
                            : rcWorkArea.Right - rcWorkArea.Width - paddingFromTaskBar;
                        y = rcWorkArea.Bottom;
                    }

                    break; // Seems to be okay
                }
                catch (Exception)
                {
                    if (retryCount >= (retryLimit - 1))
                        throw;
                }
            }

            if (appBarInfo == null)
            {
                Log.Warning(nameof(TrayInfo), "Failed to get {0}", nameof(AppBarInfo));
            }
            else
            {
                Log.Info(
                    nameof(TrayInfo),
                    "Got tray position ({0},{1}) with alignment={2}, Left={3}, Right={4}, Top={5}, Bottom={6}, Height={7}, Width={8}, Retries={9}, AutoHidden={10}",
                    x,
                    y,
                    appBarInfo.Edge,
                    rcWorkArea.Left,
                    rcWorkArea.Right,
                    rcWorkArea.Top,
                    rcWorkArea.Bottom,
                    rcWorkArea.Height,
                    rcWorkArea.Width,
                    retryCount,
                    isAutoHiddenBar
                );
            }

            try
            {
                var result = VisualTreeHelper.GetDpi(v);
                if (result.DpiScaleX != 1 || result.DpiScaleY != 1)
                {
                    Log.Info(
                        nameof(TrayInfo),
                        "Detected scaling, adjusting position. DpiScale=({0}, {1}), PixelsPerInch=({2}, {3}), PixelsPerDip={4}",
                        result.DpiScaleX,
                        result.DpiScaleY,
                        result.PixelsPerInchX,
                        result.PixelsPerInchY,
                        result.PixelsPerDip
                    );

                    x = (int)(x / result.DpiScaleX);
                    y = (int)(y / result.DpiScaleY);
                }
            }
            catch (Exception ex)
            {
                Log.Exception(nameof(TrayInfo), ex);
            }

            return new Point { X = x, Y = y };
        }
    }


    public class AppBarInfo
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(String lpClassName, String lpWindowName);

        [DllImport("shell32.dll")]
        private static extern UInt32 SHAppBarMessage(UInt32 dwMessage, ref APPBARDATA data);

        [DllImport("user32.dll")]
        private static extern Int32 SystemParametersInfo(UInt32 uiAction, UInt32 uiParam, IntPtr pvParam, UInt32 fWinIni);


        private const int ABE_BOTTOM = 3;
        private const int ABE_LEFT = 0;
        private const int ABE_RIGHT = 2;
        private const int ABE_TOP = 1;

        private const int ABM_GETTASKBARPOS = 0x00000005;
        private const int ABM_GETAUTOHIDBAR = 0x00000007;

        // SystemParametersInfo constants
        private const UInt32 SPI_GETWORKAREA = 0x0030;

        private APPBARDATA m_data;

        public ScreenEdge Edge => (ScreenEdge)m_data.uEdge;
        public Rectangle WorkArea => GetRectangle(m_data.rc);

        private Rectangle GetRectangle(RECT rc)
        {
            return new Rectangle(rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top);
        }

        public void GetSystemTaskBarPosition(out bool isAutoHide)
        {
            GetPosition("Shell_TrayWnd", null);
            isAutoHide = GetAutoHide();
        }

        public void GetPosition(string strClassName, string strWindowName)
        {
            m_data = new APPBARDATA();
            m_data.cbSize = (UInt32)Marshal.SizeOf(m_data.GetType());

            IntPtr hWnd = FindWindow(strClassName, strWindowName);
            m_data.hWnd = hWnd;

            if (hWnd != IntPtr.Zero)
            {
                UInt32 uResult = SHAppBarMessage(ABM_GETTASKBARPOS, ref m_data);

                if (uResult != 1)
                {
                    throw new Exception("Failed to communicate with the given AppBar");
                }
            }
            else
            {
                throw new Exception("Failed to find an AppBar that matched the given criteria");
            }
        }

        bool GetAutoHide()
        {
            try
            {
                UInt32 uResult = SHAppBarMessage(ABM_GETAUTOHIDBAR, ref m_data);
                bool isAutoHide = uResult != 0;
                return isAutoHide;
            }
            catch (Exception ex)
            {
                Log.Exception(nameof(AppBarInfo), ex, "Failed to check whether task bar is using auto hide feature. Assuming it's not.");
            }

            return false;
        }

        public enum ScreenEdge
        {
            Undefined = -1,
            Left = ABE_LEFT,
            Top = ABE_TOP,
            Right = ABE_RIGHT,
            Bottom = ABE_BOTTOM
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct APPBARDATA
        {
            public UInt32 cbSize;
            public IntPtr hWnd;
            public UInt32 uCallbackMessage;
            public UInt32 uEdge;
            public RECT rc;
            public Int32 lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public Int32 left;
            public Int32 top;
            public Int32 right;
            public Int32 bottom;
        }
    }
}