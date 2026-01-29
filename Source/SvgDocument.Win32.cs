using System;
using System.Runtime.InteropServices;

namespace Svg
{
    public partial class SvgDocument
    {
        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        private static int GetWin32SystemDpi()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return 96;
            }

            try
            {
                // NOTE: starting with Windows 8.1, the DPI is no longer system-wide but screen-specific
                IntPtr hDC = GetDC(IntPtr.Zero);
                if (hDC == IntPtr.Zero) return 96;

                const int LOGPIXELSY = 90;
                int result = GetDeviceCaps(hDC, LOGPIXELSY);
                ReleaseDC(IntPtr.Zero, hDC);
                return result;
            }
            catch
            {
                return 96;
            }
        }
    }
}
