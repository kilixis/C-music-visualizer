using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MusicVisualizer
{
    public static class Win32
    {
        const int WS_EX_LAYERED = 0x80000;
        const int WS_EX_TRANSPARENT = 0x20;
        const int WS_EX_TOOLWINDOW = 0x80;
        const int GWL_EXSTYLE = -20;

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public static void MakeWindowClickThrough(Form form)
        {
            int exStyle = GetWindowLong(form.Handle, GWL_EXSTYLE);
            SetWindowLong(form.Handle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
        }
    }
}
