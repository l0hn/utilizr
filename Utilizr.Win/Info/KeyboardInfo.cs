using System;
using System.Diagnostics;
using Utilizr.Win32.User32;

namespace Utilizr.Win.Info
{
    public class KeyboardInfo
    {
        // Popular keys from System.Windows.Forms.Keys
        public static int None { get; } = 0;
        public static int CapsLock { get; } = 20;
        public static int LeftShift { get; } = 160;
        public static int LeftCtrl { get; } = 162;

        private KeyboardInfo() { }

        /// <summary>
        /// </summary>
        /// <param name="key">Enum int value from System.Windows.Forms.Key</param>
        /// <returns></returns>
        public static KeyStateInfo GetState(int key)
        {
            short keyState = User32.GetKeyState(key);
            byte[] bits = BitConverter.GetBytes(keyState);
            bool toggled = bits[0] > 0;
            bool pressed = bits[1] > 0;
            return new KeyStateInfo(key, pressed, toggled);
        }
    }

    [DebuggerDisplay("IsPressed={IsPressed}, IsToggled={IsToggled}")]
    public struct KeyStateInfo
    {
        public static KeyStateInfo Default { get; } = new KeyStateInfo(KeyboardInfo.None, false, false);
        public int Key { get; }
        public bool IsPressed { get; }
        public bool IsToggled { get; }

        public KeyStateInfo(int key, bool ispressed, bool istoggled)
        {
            Key = key;
            IsPressed = ispressed;
            IsToggled = istoggled;
        }
    }
}