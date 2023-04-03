using System;
using System.Runtime.InteropServices;
using Utilizr.Logging;
using Utilizr.Win32.User32;
using Utilizr.Win32.User32.Flags;

namespace Utilizr.WPF.Util
{
    public class GlobalKeyboardHook : IDisposable
    {
        public delegate void GlobalKeyEventHandler(int vkCode);

        public event GlobalKeyEventHandler? GlobalKeyDown;
        public event GlobalKeyEventHandler? GlobalKeyUp;

        public enum KeyEvents
        {
            KeyDown = 0x0100,
            KeyUp = 0x0101,
            SKeyDown = 0x0104,
            SKeyUp = 0x0105
        }

        readonly int _key;
        int _hookID = 0;
        readonly WindowsHookCallbackDelegate _hookCallback;

        // https://stackoverflow.com/a/34290332
        /// <summary>
        /// </summary>
        /// <param name="key">Enum int value from System.Windows.Forms.Key</param>
        public GlobalKeyboardHook(int key)
        {
            _key = key;
            _hookCallback = new WindowsHookCallbackDelegate(KeybHookProc);
        }

        public void Register()
        {
            _hookID = User32.SetWindowsHookEx(WindowsHookType.WH_KEYBOARD_LL, _hookCallback, 0, 0);
        }

        public void Unregister()
        {
            if (_hookID == 0)
                return;

            User32.UnhookWindowsHookEx(_hookID);
        }

        public void Dispose()
        {
            Unregister();
        }

        int KeybHookProc(int code, IntPtr w, IntPtr l)
        {
            if (code < 0)
                return User32.CallNextHookEx(_hookID, code, w, l);

            try
            {
                var kEvent = (KeyEvents)w;
                var vkCode = Marshal.ReadInt32((IntPtr)l);

                if (vkCode == _key)
                {
                    if (kEvent == KeyEvents.KeyDown || kEvent == KeyEvents.SKeyDown)
                    {
#if DEBUG
                        Log.Info(nameof(GlobalKeyboardHook), $"KeyDown {vkCode}");
#endif
                        GlobalKeyDown?.Invoke(vkCode);
                    }

                    if (kEvent == KeyEvents.KeyUp || kEvent == KeyEvents.SKeyUp)
                    {
#if DEBUG
                        Log.Info(nameof(GlobalKeyboardHook), $"KeyUp {vkCode}");
#endif
                        GlobalKeyUp?.Invoke(vkCode);
                    }
                }
            }
            catch (Exception)
            {
                //Ignore all errors...
            }

            return User32.CallNextHookEx(_hookID, code, w, l);
        }
    }
}