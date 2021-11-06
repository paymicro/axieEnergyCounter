using System;
using System.Windows;
using System.Windows.Input;

namespace axieEnergyCounter
{
    public partial class MainWindow : Window
    {
        CoreLogic logic;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            logic = new CoreLogic(this);
            logic.RoundChanged += Logic_RoundChanged;
            logic.CounterChanged += Logic_CounterChanged;
            logic.Reset();
            UseGlobalHook.IsChecked = true;
        }

        void Logic_CounterChanged(object sender, CounterEventArgs e)
        {
            display.Content = e.Current.ToString();
            // logging
            var sign = e.Change < 0 ? "" : "+";
            Log.Text += $"{sign}{e.Change}{Environment.NewLine}";
        }

        void Logic_RoundChanged(object sender, CounterEventArgs e)
        {
            var roundText = $"=[ Round {e.Current} ]=";
            RoundLabel.Content = roundText;
            Log.Text += $"{roundText}{Environment.NewLine}";
        }

        protected override void OnClosed(EventArgs e)
        {
            logic.Close();
            base.OnClosed(e);
        }

        void Button_Minus(object sender, RoutedEventArgs e)
        {
            logic.ChangeCounter(-1);
        }

        void Button_Plus(object sender, RoutedEventArgs e)
        {
            logic.ChangeCounter(1);
        }

        void Button_Reset(object sender, RoutedEventArgs e)
        {
            Log.Text = "";
            logic.Reset();
        }

        void Log_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Log.ScrollToEnd();
        }

        void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.IsUp && !logic.GlobalHooksRegistred)
            {
                switch (e.Key.GetHashCode())
                {
                    case 0x55: 
                        logic.ChangeCounter(1);
                        break;
                    case 0x57: 
                        logic.ChangeCounter(-1);
                        break;
                    case 0x06: 
                        logic.NextRound();
                        break;
                    case 0x13: 
                        logic.Reset();
                        break;
                    default:
                        break;
                }
            }
        }

        void UseGlobalHook_Checked(object sender, RoutedEventArgs e)
        {
            logic.RegisterHotKeys();
            if (!logic.GlobalHooksRegistred)
            {
                e.Handled = true;
            }
        }

        void UseGlobalHook_Unchecked(object sender, RoutedEventArgs e)
        {
            logic.UnregisterHotKeys();
        }
    }
}
