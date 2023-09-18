using System;
using System.Runtime.InteropServices;
using Utilizr.Win32.User32.Flags;
using Utilizr.Win32.User32.Structs;

namespace Utilizr.Win32.User32
{
    public static class User32
    {
        const string USER32_DLL = "user32.dll";

        [DllImport(USER32_DLL, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport(USER32_DLL, SetLastError = true)]
        public static extern short GetKeyState(int vKey);

        [DllImport(USER32_DLL, SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport(USER32_DLL, SetLastError = true)]
        public static extern IntPtr MonitorFromWindow(IntPtr handle, MonitorFromWindowFlags flags);


        [DllImport(USER32_DLL, SetLastError = true)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, NativeMonitorInfo lpmi);

        [DllImport(USER32_DLL)]
        public static extern IntPtr GetActiveWindow();

        [DllImport(USER32_DLL, SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport(USER32_DLL)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport(USER32_DLL)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport(USER32_DLL)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport(USER32_DLL)]
        public static extern bool GetWindowRect(IntPtr hwnd, out NativeRect rect);

        [DllImport(USER32_DLL, SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport(USER32_DLL, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ExitWindowsEx(ExitWindows uFlags, ShutdownReason dwReason);

        [DllImport(USER32_DLL, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int SetWindowsHookEx(WindowsHookType idHook, WindowsHookCallbackDelegate lpfn, int hInstance, int threadId);

        [DllImport(USER32_DLL, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport(USER32_DLL, SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}