using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.Util
{
    /// <summary>
    /// A trivial implementation of IWin32Window for use when serializing window handles cross-process.
    /// </summary>
    public class Win32Window : IWin32Window
    {
        public Win32Window(IntPtr hwnd)
        {
            Handle = hwnd;
        }

        public IntPtr Handle { get; }
    }

    public static class Win32WindowExtensions
    {
        public static IntPtr SafeHandle(this IWin32Window window)
        {
            if (window is Form form)
            {
                IntPtr hwnd = IntPtr.Zero;
                form.Invoke(new Action(() => hwnd = window.Handle));
                return hwnd;
            }
            return window.Handle;
        }
    }
}
