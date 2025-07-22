using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using Vortex.AutoClick;

namespace Auto_Click
{
    public partial class MainWindow : Window
    {
        #region Private Variables
        private CAutoClick m_auto;
        private Thread m_thread;
        private bool m_bIsRunning = false;
        private IntPtr m_windowHwnd;
        private HwndSource m_source;

        private const int HOTKEY_ID_ON = 1;
        private const int HOTKEY_ID_OFF = 2;
        private const int WM_HOTKEY = 0x0312;
        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region Private Methods

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();

                if (id == HOTKEY_ID_ON)
                {
                    OnHotKeyPressed(true);
                    handled = true;
                }
                else if (id == HOTKEY_ID_OFF)
                {
                    OnHotKeyPressed(false);
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DragProgram(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void OnHotKeyPressed(bool bActive)
        {
            UnregisterHotkeys();
            RegisterHotkeys();

            if (bActive)
            {
                Key keyOn = GetSelectedKey(cbKeyOn);
                Key keyOff = GetSelectedKey(cbKeyOff);

                if(keyOn == keyOff)
                {
                    MessageBox.Show("You can't use the same key for activation and deactivation.", "Oh no!");
                    return;
                }

                int iSpeed = (int)(slSpeed.Value);
                int iMouseButton = cbMouseButton.SelectedIndex == 0 ? 0x2 : 0x8;

                m_auto = new CAutoClick(keyOn, keyOff, iSpeed, iMouseButton);

                if (!m_bIsRunning)
                {
                    lbStatus.Text = "Online";
                    lbStatus.Foreground = System.Windows.Media.Brushes.LimeGreen;
                    m_bIsRunning = true;
                    m_thread = new Thread(() =>
                    {
                        while (m_bIsRunning)
                        {
                            m_auto.Click();
                            Thread.Sleep(m_auto.Speed);
                        }
                    });
                    m_thread.IsBackground = true;
                    m_thread.Start();
                }
            }
            else
            {
                if (m_bIsRunning)
                {
                    m_bIsRunning = false;
                    lbStatus.Text = "Offline";
                    lbStatus.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
        }

        private void ClickCounter(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(tbClick.Text, out int value))
            {
                value = 0;
            }
            tbClick.Text = (++value).ToString();
        }

        private Key GetSelectedKey(ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ComboBoxItem item)
            {
                string keyName = item.Content.ToString();
                if (Enum.TryParse(keyName, out Key result))
                    return result;
            }
            return Key.None;
        }

        private uint GetVirtualKeyFromComboBox(ComboBox comboBox)
        {
            Key key = GetSelectedKey(comboBox);
            return (uint)KeyInterop.VirtualKeyFromKey(key);
        }

        private void RegisterHotkeys()
        {
            uint vkOn = GetVirtualKeyFromComboBox(cbKeyOn);
            uint vkOff = GetVirtualKeyFromComboBox(cbKeyOff);

            if (vkOn != 0)
            {
                RegisterHotKey(m_windowHwnd, HOTKEY_ID_ON, 0, vkOn);
            }

            if (vkOff != 0)
            {
                RegisterHotKey(m_windowHwnd, HOTKEY_ID_OFF, 0, vkOff);
            }
        }

        private void UnregisterHotkeys()
        {
            UnregisterHotKey(m_windowHwnd, HOTKEY_ID_ON);
            UnregisterHotKey(m_windowHwnd, HOTKEY_ID_OFF);
        }

        #endregion

        #region Protected Methods

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            m_windowHwnd = new WindowInteropHelper(this).Handle;
            m_source = HwndSource.FromHwnd(m_windowHwnd);
            m_source.AddHook(HwndHook);

            RegisterHotkeys();
        }

        protected override void OnClosed(EventArgs e)
        {
            m_bIsRunning = false;
            UnregisterHotkeys();
            m_source.RemoveHook(HwndHook);
            base.OnClosed(e);
        }

        #endregion
    }
}
