using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Utilizr.Logging;

namespace Utilizr.WPF
{

    public class PerMonitorDPIWindow: Window
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        [DllImport("user32.dll")]
        static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
        const int MONITOR_DEFAULTTONULL = 0;
        const int MONITOR_DEFAULTTOPRIMARY = 1;
        const int MONITOR_DEFAULTTONEAREST = 2;

        [DllImport("shcore.dll", CallingConvention = CallingConvention.StdCall)]
        protected static extern int GetDpiForMonitor(IntPtr hMonitor, int dpiType, ref uint xDpi, ref uint yDpi);

        [DllImport("shcore.dll", CallingConvention = CallingConvention.StdCall)]
        protected static extern int GetProcessDpiAwareness(IntPtr handle, ref ProcessDpiAwareness awareness);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_NOOWNERZORDER = 0x0200;
        public const int SWP_NOACTIVATE = 0x0010;


        public enum ProcessDpiAwareness
        {
            Process_DPI_Unaware = 0,
            Process_System_DPI_Aware = 1,
            Process_Per_Monitor_DPI_Aware = 2
        }

        protected enum MonitorDpiType
        {
            MDT_Effective_DPI = 0,
            MDT_Angular_DPI = 1,
            MDT_Raw_DPI = 2,
            MDT_Default = MDT_Effective_DPI
        }


        const int WM_DPICHANGED = 0x02E0;



        private Point _currentDpi;
        private double _wpfDpi;
        private HwndSource _hwndSource;
        private double _scaleFactor;
        private bool _perMonitorDpiAware;

        public PerMonitorDPIWindow()
        {
            Loaded += OnLoaded;
            try
            {
                _perMonitorDpiAware = GetCurrentProcessDpiAwareness() == ProcessDpiAwareness.Process_Per_Monitor_DPI_Aware;
            }
            catch (Exception)
            {
                
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_perMonitorDpiAware)
            {
                _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
                _wpfDpi = 96.0 * _hwndSource.CompositionTarget.TransformToDevice.M11;
                _currentDpi = GetDpiForHwnd(_hwndSource.Handle);
                _scaleFactor = _currentDpi.X / _wpfDpi;
                Width = Width * _scaleFactor;
                Height = Height * _scaleFactor;
                UpdateLayoutTransform(_scaleFactor);
                //UpdateLayoutTransform(1.0);
            }
        }

        void UpdateLayoutTransform(double scaleFactor)
        {
            if (_perMonitorDpiAware)
            {
                var child = GetVisualChild(0);
                if (_scaleFactor != 1.0)
                {
                    var dpiScale = new ScaleTransform(scaleFactor, scaleFactor);
                    child.SetValue(Window.LayoutTransformProperty, dpiScale);
                }
                else
                {
                    child.SetValue(Window.LayoutTransformProperty, null);
                }
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Handle messages...
            switch (msg)
            {
                case WM_DPICHANGED:

                    RECT lprNewRect = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));

                    var newWidth = lprNewRect.Right - lprNewRect.Left;
                    var newHeight = lprNewRect.Bottom - lprNewRect.Top;

                    Debug.WriteLine($"lprect width: {newWidth}, height: {newHeight}");

                    SetWindowPos(
                        hwnd,
                        IntPtr.Zero,
                        lprNewRect.Left,
                        lprNewRect.Top,
                        newWidth,
                        newHeight,
                        SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOZORDER);
                    var oldDpi = _currentDpi;
                    _currentDpi = new Point
                    {
                        X = (double)(wParam.ToInt32() >> 16),
                        Y = (double)(wParam.ToInt32() & 0x0000FFFF)
                    };
                    Debug.WriteLine($"current dpi: {_currentDpi}");
                    Log.Info("DISPLAY", $"current dpi: {_currentDpi}");
                    if (oldDpi.X != _currentDpi.X || oldDpi.Y != _currentDpi.Y)
                    {
                        Debug.WriteLine("DPI Changed!!");

                        DpiChanged();

                    }
                    break;
            }
            return IntPtr.Zero;
        }

        Point GetDpiForHwnd(IntPtr hwnd)
        {
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            uint newDpiX = 96;
            uint newDpiY = 96;
            if (GetDpiForMonitor(monitor, (int)MonitorDpiType.MDT_Effective_DPI, ref newDpiX, ref newDpiY) != 0)
            {
                return new Point(96.0, 96.0);
            }
            return new Point((double)newDpiX, (double)newDpiY);
        }

        ProcessDpiAwareness GetCurrentProcessDpiAwareness()
        {
            ProcessDpiAwareness awareness = ProcessDpiAwareness.Process_DPI_Unaware;
            var res = GetProcessDpiAwareness(Process.GetCurrentProcess().Handle, ref awareness);
            return awareness;
        }

        void DpiChanged()
        {
            _scaleFactor = _currentDpi.X / _wpfDpi;
            Debug.WriteLine($"Scale factor is {_scaleFactor}");
            UpdateLayoutTransform(_scaleFactor);
            //UpdateLayoutTransform(1.0);
            Debug.WriteLine($"width: {Width}, height: {Height}");
        }
    }
}
