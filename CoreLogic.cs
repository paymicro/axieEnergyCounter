using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace axieEnergyCounter
{
    public class CoreLogic
    {
        [DllImport("user32.dll")]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        const int HOTKEY_ID = 19666;
        const int HOTKEY_PLUS = 0x6B;
        const int HOTKEY_MINUS = 0x6D;
        const int HOTKEY_ENTER = 0x0D;
        const int HOTKEY_ESC = 0x1B;
        const int WM_HOTKEY = 0x0312;
        const uint MOD_NONE = 0x0000; //(none)

        IntPtr _windowHandle;
        HwndSource _source;
        int _counter = 3;
        int _numRound = 1;

        public CoreLogic(Window mainWindow)
        {
            _windowHandle = new WindowInteropHelper(mainWindow).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);
        }

        public bool GlobalHooksRegistred { get; private set; } = false;

        IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != WM_HOTKEY)
            {
                return IntPtr.Zero;
            }

            int vkey = ((int)lParam >> 16) & 0xFFFF;
            switch (vkey)
            {
                case HOTKEY_PLUS:
                    ChangeCounter(1);
                    handled = true;
                    break;
                case HOTKEY_MINUS:
                    ChangeCounter(-1);
                    handled = true;
                    break;
                case HOTKEY_ENTER:
                    NextRound();
                    handled = true;
                    break;
                case HOTKEY_ESC:
                    Reset();
                    handled = true;
                    break;
                default:
                    handled = false;
                    break;
            }

            return IntPtr.Zero;
        }

        internal void Close()
        {
            _source.RemoveHook(HwndHook);
            if (GlobalHooksRegistred)
            {
                UnregisterHotKeys();
            }
        }

        internal void UnregisterHotKeys()
        {
            if (_windowHandle != IntPtr.Zero)
            {
                _ = UnregisterHotKey(_windowHandle, HOTKEY_ID);
                _ = UnregisterHotKey(_windowHandle, HOTKEY_ID + 1);
                _ = UnregisterHotKey(_windowHandle, HOTKEY_ID + 2);
                _ = UnregisterHotKey(_windowHandle, HOTKEY_ID + 3);
            }
            GlobalHooksRegistred = false;
        }

        internal void RegisterHotKeys()
        {
            GlobalHooksRegistred = _windowHandle != IntPtr.Zero
                && RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, HOTKEY_PLUS) // +1
                && RegisterHotKey(_windowHandle, HOTKEY_ID + 1, MOD_NONE, HOTKEY_MINUS) // -1
                && RegisterHotKey(_windowHandle, HOTKEY_ID + 2, MOD_NONE, HOTKEY_ENTER) // Enter - new round +2
                && RegisterHotKey(_windowHandle, HOTKEY_ID + 3, MOD_NONE, HOTKEY_ESC); // ESC - reset to 3
        }

        public event EventHandler<CounterEventArgs> RoundChanged;
        public event EventHandler<CounterEventArgs> CounterChanged;

        public void NextRound()
        {
            _numRound++;
            RoundChanged?.Invoke(this, new CounterEventArgs()
            {
                Change = 1,
                Current = _numRound
            });
            ChangeCounter(2);
        }

        public void Reset()
        {
            var change = _numRound;
            _numRound = 1;
            _counter = 0;
            RoundChanged?.Invoke(this, new CounterEventArgs()
            {
                Change = change,
                Current = _numRound
            });
            ChangeCounter(3);
        }

        public void ChangeCounter(int value = 0)
        {
            // limits in range from 0 and 10 
            var newValue = Math.Max(0, Math.Min(_counter + value, 10));
            if (newValue != _counter)
            {
                var change = newValue - _counter;
                _counter = newValue;
                CounterChanged?.Invoke(this, new CounterEventArgs()
                {
                    Change = change,
                    Current = _counter
                });
            }
        }
    }
}
