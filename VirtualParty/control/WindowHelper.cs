using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace VirtualParty
{
    class WindowHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        public static void Show(Window win)
        {
            var fwnd = GetForegroundWindow();
            if (fwnd == null) win.Owner = null;
            else win.Owner = (Window)HwndSource.FromHwnd(fwnd).RootVisual;
            win.ShowInTaskbar = false;
            win.Show();
        }

    }
}
