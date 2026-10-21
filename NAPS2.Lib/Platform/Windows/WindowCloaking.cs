namespace NAPS2.Platform.Windows;

/// <summary>
/// Hides windows (without changing their visibility state) so they're only shown once fully rendered.
/// </summary>
internal static class WindowCloaking
{
    private const int DWMWA_CLOAK = 13;
    private const uint RDW_INVALIDATE = 0x1;
    private const uint RDW_UPDATENOW = 0x100;
    private const uint RDW_ALLCHILDREN = 0x80;

    public static void SetCloaked(IntPtr hwnd, bool cloaked)
    {
        int value = cloaked ? 1 : 0;
        Win32.DwmSetWindowAttribute(hwnd, DWMWA_CLOAK, ref value, sizeof(int));
    }

    /// <summary>
    /// Synchronously paints the window and its children (e.g. for WinForms, which otherwise paints when idle).
    /// </summary>
    public static void PaintNow(IntPtr hwnd)
    {
        Win32.RedrawWindow(hwnd, IntPtr.Zero, IntPtr.Zero, RDW_INVALIDATE | RDW_UPDATENOW | RDW_ALLCHILDREN);
    }
}
