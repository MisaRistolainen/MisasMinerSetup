using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace MisasMinerSetup.Helpers
{
    public class HotkeyHelper
    {
        #region Singleton

        private static HotkeyHelper instance;

        private HotkeyHelper() { }

        public static HotkeyHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new HotkeyHelper();
                }

                return instance;
            }
        }

        #endregion

        #region DllImports

        [DllImport("User32.dll")] //Creating hotkeys for notifications of current hashrate
        private static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        #endregion

        #region Event Handler

        public delegate void HotkeyPressedHandler(object sender, EventArgs e);
        public event HotkeyPressedHandler OnHotkeyPressed;

        #endregion

        private HwndSource _source;
        private const int HOTKEY_ID = 9000;
        
        public void RegisterHotKey(System.Windows.Window window)
        {
            var helper = new WindowInteropHelper(window);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);

            helper = new WindowInteropHelper(window);
            const uint VK_F10 = 0x79;
            const uint MOD_CTRL = 0x0002;

            if (!RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CTRL, VK_F10))
            {
                // handle error
            }
        }

        public void UnregisterHotKey(System.Windows.Window window)
        {
            _source.RemoveHook(HwndHook);
            _source = null;

            var helper = new WindowInteropHelper(window);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;

            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            HotkeyPressed();
                            handled = true;
                            break;
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        private void HotkeyPressed()
        {
            // Make sure someone is listening to event
            if (OnHotkeyPressed == null) return;
            
            OnHotkeyPressed(this, new EventArgs());
        }
    }
}
