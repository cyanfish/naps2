using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.Util
{
    public class Win32Window : IWin32Window
    {
        public Win32Window(IntPtr hwnd)
        {
            Handle = hwnd;
        }

        public IntPtr Handle { get; }
    }
}
