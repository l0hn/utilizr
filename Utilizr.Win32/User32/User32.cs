using System;
using System.Runtime.InteropServices;
using Utilizr.Win32.User32.Flags;

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
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport(USER32_DLL, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int SetWindowsHookEx(WindowsHookType idHook, WindowsHookCallbackDelegate lpfn, int hInstance, int threadId);

        [DllImport(USER32_DLL, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport(USER32_DLL, SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}