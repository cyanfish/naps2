using System.Windows.Forms;
using NAPS2.Platform.Windows;
using NAPS2.Scan.Internal.Twain;

namespace NAPS2.WinForms;

public class WinFormsTwainHandleManager : TwainHandleManager
{
    private readonly Form _baseForm;
    private Form? _parentForm;
    private IntPtr _disabledWindow;
    private bool _disposed;

    public WinFormsTwainHandleManager(Form baseForm)
    {
        _baseForm = baseForm;
    }

    public override IntPtr GetDsmHandle(IntPtr dialogParent, bool useNativeUi)
    {
        return _baseForm.Handle;
    }

    public override IntPtr GetEnableHandle(IntPtr dialogParent, bool useNativeUi)
    {
        if (!useNativeUi || dialogParent == IntPtr.Zero)
        {
            return _baseForm.Handle;
        }
        _parentForm = new BackgroundForm();
        // At the Windows API level, a modal window is implemented by doing two things:
        // 1. Setting the parent on the child window
        // 2. Disabling the parent window
        // We do this rather than calling ShowDialog to avoid blocking the thread.
        _parentForm.Show(new Win32Window(dialogParent));
        Win32.EnableWindow(dialogParent, false);
        _disabledWindow = dialogParent;
        return _parentForm.Handle;
    }

    public override void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _parentForm?.Close();
        if (_disabledWindow != IntPtr.Zero)
        {
            Win32.EnableWindow(_disabledWindow, true);
        }
    }
}