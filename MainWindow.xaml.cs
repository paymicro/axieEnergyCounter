using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace axieEnergyCounter
{
    public partial class MainWindow : Window
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
        bool globalHooksIsRegistred = false;

        int Counter { get; set; } = 3;
        int NumRound { get; set; } = 1;

        public MainWindow()
        {
            InitializeComponent();
            Reset();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);
            UseGlobalHook.IsChecked = true;
        }

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

        string GetRoundText()
        {
            return $"=[ Round {NumRound} ]=";
        }

        void NextRound()
        {
            NumRound++;
            RoundLabel.Content = GetRoundText();
            Log.Text += $"{GetRoundText()}{Environment.NewLine}";
            ChangeCounter(2);
        }

        void Reset()
        {
            NumRound = 1;
            Counter = 0;
            RoundLabel.Content = GetRoundText();
            Log.Text = $"{GetRoundText()}{Environment.NewLine}";
            ChangeCounter(3);
        }

        void ChangeCounter(int value = 0)
        {
            // limits in range from 0 and 10 
            var newValue = Math.Max(0, Math.Min(Counter + value, 10));
            if (newValue != Counter)
            {
                var change = newValue - Counter;
                Counter = newValue;
                display.Content = Counter.ToString();

                // logging
                var sign = change < 0 ? "" : "+";
                Log.Text += $"{sign}{change}{Environment.NewLine}";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            if (globalHooksIsRegistred)
            {
                // Unregister
                UseGlobalHook_Unchecked(null, null);
            }
            base.OnClosed(e);
        }

        private void Button_Minus(object sender, RoutedEventArgs e)
        {
            ChangeCounter(-1);
        }

        private void Button_Plus(object sender, RoutedEventArgs e)
        {
            ChangeCounter(1);
        }

        private void Button_Reset(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void Log_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Log.ScrollToEnd();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.IsUp && !globalHooksIsRegistred)
            {
                switch (e.Key.GetHashCode())
                {
                    case 0x55: ChangeCounter(1); break;
                    case 0x57: ChangeCounter(-1); break;
                    case 0x06: NextRound(); break;
                    case HOTKEY_ENTER: Reset(); break;
                }
            }
        }

        private void UseGlobalHook_Checked(object sender, RoutedEventArgs e)
        {
            if (_windowHandle == IntPtr.Zero)
            {
                return;
            }

            globalHooksIsRegistred = RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, HOTKEY_PLUS) // +1
                            && RegisterHotKey(_windowHandle, HOTKEY_ID + 1, MOD_NONE, HOTKEY_MINUS) // -1
                            && RegisterHotKey(_windowHandle, HOTKEY_ID + 2, MOD_NONE, HOTKEY_ENTER) // Enter - new round +2
                            && RegisterHotKey(_windowHandle, HOTKEY_ID + 3, MOD_NONE, HOTKEY_ESC); // ESC - reset to 3
            if (!globalHooksIsRegistred)
            {
                e.Handled = true;
            }
        }

        private void UseGlobalHook_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_windowHandle != IntPtr.Zero)
            {
                _ = UnregisterHotKey(_windowHandle, HOTKEY_ID);
                _ = UnregisterHotKey(_windowHandle, HOTKEY_ID + 1);
                _ = UnregisterHotKey(_windowHandle, HOTKEY_ID + 2);
                _ = UnregisterHotKey(_windowHandle, HOTKEY_ID + 3);
            }
            globalHooksIsRegistred = false;
        }
    }
}
