using System;
using System.Windows;
using System.Windows.Interop;
using Utilizr.Win32.User32;

// http://www.dreamincode.net/forums/topic/180436-global-hotkeys/
// https://stackoverflow.com/a/624424/1229237
// https://stackoverflow.com/a/3654821/1229237

namespace Utilizr.WPF.Util
{
    public class GlobalHotKey
    {
        public event EventHandler? KeyPressed;

        //modifiers
        public const int MOD_NONE           = 0x0000;
        public const int MOD_ALT            = 0x0001;
        public const int MOD_CTRL           = 0x0002;
        public const int MOD_SHIFT          = 0x0004;
        public const int MOD_WIN            = 0x0008;

        // Virtual Key List
        public const int VK_CAPS_LOCK = 0x14;

        //windows message id for hotkey
        const int WM_HOTKEY_MSG_ID = 0x0312;

        private readonly int _modifier;
        private readonly int _key;
        private readonly IntPtr _hWnd;
        private readonly int _id;
        private HwndSource? _source;
        private HwndSourceHook? _hook;

        /// <summary>
        /// Detect when the given key is pressed on the keyboard regardless whether the window has focus.
        /// </summary>
        public GlobalHotKey(int modifier, int key, Window window)
        {
            _modifier = modifier;
            _key = key;
            _hWnd = new WindowInteropHelper(window).Handle;
            if (_hWnd == IntPtr.Zero)
                throw new ArgumentException($"{nameof(window)} handle was 0. Has it loaded yet?");
            _id = GetHashCode();
        }

        public override int GetHashCode()
        {
            return _modifier ^ _key ^ _hWnd.ToInt32();
        }

        public bool Register()
        {
            if (_hook != null)
                return false; //already registered

            if (!User32.RegisterHotKey(_hWnd, _id, _modifier, _key))
                return false;

            _source = HwndSource.FromHwnd(_hWnd);
            _hook = new HwndSourceHook(WndProc);
            _source.AddHook(_hook);
            return true;
        }

        public bool Unregister()
        {
            if (_hook == null)
                return false; // was never registered

            bool result = User32.UnregisterHotKey(_hWnd, _id);
            _source!.RemoveHook(_hook);
            return result;
        }

        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_HOTKEY_MSG_ID:
                    OnKeyPressed();
                    handled = true;
                    break;
            }

            return IntPtr.Zero;
        }

        protected virtual void OnKeyPressed()
        {
            KeyPressed?.Invoke(this, EventArgs.Empty);
        }
    }
}