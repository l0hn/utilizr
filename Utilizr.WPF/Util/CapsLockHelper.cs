using Utilizr.Logging;
using Utilizr.Win.Info;

namespace Utilizr.WPF.Util
{
    /// <summary>
    /// Work around for issue in WPF. GlobalKeyboardHook does always detect key
    /// up and down but not possible to get correct value while the app's window
    /// is active. Instead remember initial state on app launch and use the toggle
    /// count to determine caps lock state. Nasty workaround for nasty issue...
    /// </summary>
    public static class CapsLockHelper
    {
        public delegate void CapsLockHelperHandler(bool capsLock);
        public static event CapsLockHelperHandler? CapsLockChanged;

        public static bool CapsLock
        {
            get
            {
                if (!_isInitialized)
                {
                    Log.Warning($"{nameof(CapsLockHelper)} has not been initialised yet. {nameof(Initialise)} must be invoked to avoid broken default behaviour.");

                    var capsLock = KeyboardInfo.GetState(KeyboardInfo.CapsLock);
                    return capsLock.IsPressed || capsLock.IsToggled;
                }
                // If caps lock on app start, even presses is caps, odd is no caps.
                // Other way around is no caps lock on startup
                bool evenPresses = _pressCount % 2 == 0;

                var hasCaps = _initialCapsLockState
                    ? evenPresses
                    : !evenPresses;

                return hasCaps;
            }
        }

        static bool _isInitialized;
        static bool _initialCapsLockState;
        static GlobalKeyboardHook _globalKeyboardHook;
        static uint _pressCount = 0;
        static readonly object SETUP_LOCK = new();

        public static void Initialise()
        {
            lock (SETUP_LOCK)
            {
                if (_isInitialized)
                    return;

                _globalKeyboardHook = new GlobalKeyboardHook(KeyboardInfo.CapsLock);
                _globalKeyboardHook.GlobalKeyDown += _globalKeyboardHook_GlobalKeyDown;
                _globalKeyboardHook.Register();

                var capsLock = KeyboardInfo.GetState(KeyboardInfo.CapsLock);
                _initialCapsLockState = capsLock.IsPressed || capsLock.IsToggled;
                _isInitialized = true;
            }
        }

        static void _globalKeyboardHook_GlobalKeyDown(int vkCode)
        {
            _pressCount = _pressCount + 1 > uint.MaxValue // odd
                        ? 0 // ..so this can be even
                        : _pressCount + 1U;

#if DEBUG
            Log.Info(nameof(CapsLockHelper), $"hasCaps = {CapsLock}");
#endif
            CapsLockChanged?.Invoke(CapsLock);
        }
    }
}
