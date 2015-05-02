using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace GlobalKeyboardHook
{
    public class KeyboardHandler : IDisposable
    {
        public event Action keyboardEventHandler;
        public const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly Window _mainWindow;
        WindowInteropHelper _host;
        private int _keyCode;

        public KeyboardHandler(Window mainWindow, Key key)
        {
            _mainWindow = mainWindow;
            _host = new WindowInteropHelper(_mainWindow);
            _keyCode = (int)KeyInterop.VirtualKeyFromKey(key);

            SetupHotKey(_host.Handle);
            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
        }

        void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message == WM_HOTKEY)
            {
                //Handle hot key kere
                if (keyboardEventHandler != null)
                    keyboardEventHandler();
            }
        }

        private void SetupHotKey(IntPtr handle)
        {
            RegisterHotKey(handle, GetType().GetHashCode(), 0, _keyCode);
        }

        public void Dispose()
        {
            UnregisterHotKey(_host.Handle, GetType().GetHashCode());
        }
    }
}