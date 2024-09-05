using NAPS2.Scan.Internal.Twain;
using NTwain;

namespace NAPS2.Platform.Windows;

internal class Win32TwainHandleManager : TwainHandleManager
{
    private readonly Win32MessagePump _messagePump;
    private IntPtr _parentWindow;
    private IntPtr _disabledWindow;
    private bool _disposed;
    private IntPtr? _handle;

    public Win32TwainHandleManager(Win32MessagePump messagePump)
    {
        _messagePump = messagePump;
    }

    public override IntPtr GetDsmHandle(IntPtr dialogParent, bool useNativeUi)
    {
        // This handle is used for the TWAIN event loop. However, in some cases (e.g. an early error) it can still
        // be used for UI.
        return _handle ??= GetHandle(dialogParent, useNativeUi);
    }

    public override IntPtr GetEnableHandle(IntPtr dialogParent, bool useNativeUi)
    {
        // This handle is used as the parent window for TWAIN UI
        return _handle ??= GetHandle(dialogParent, useNativeUi);
    }

    private IntPtr GetHandle(IntPtr dialogParent, bool useNativeUi)
    {
        if (dialogParent == IntPtr.Zero)
        {
            // If we have no real parent, we just give it an arbitrary form handle in this process as a parent.
            return _messagePump.Handle;
        }

        // If we are expected to show UI, ideally we'd just return dialogParent. But I've found some issues with that
        // where the window can become non-interactable (e.g. unable to cancel a native UI scan). The cause might be
        // related to the window being in another process.
        _parentWindow = _messagePump.CreateBackgroundWindow(dialogParent);

        // At the Windows API level, a modal window is implemented by doing two things:
        // 1. Setting the parent on the child window
        // 2. Disabling the parent window
        // We do this rather than calling ShowDialog to avoid blocking the thread.
        if (useNativeUi)
        {
            // We only want to disable the parent window if we're showing the native UI. Otherwise, we expect that
            // the NAPS2 UI should be interactable, and the only UI shown should be error messages.
            Win32.EnableWindow(dialogParent, false);
        }
        _disabledWindow = dialogParent;

        return _parentWindow;
    }

    public override MessageLoopHook CreateMessageLoopHook(IntPtr dialogParent = default, bool useNativeUi = false)
    {
        return new Win32MessageLoopHook(_messagePump, GetDsmHandle(dialogParent, useNativeUi));
    }

    public override void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _messagePump.CloseWindow(_parentWindow);
        if (_disabledWindow != IntPtr.Zero)
        {
            Win32.EnableWindow(_disabledWindow, true);
        }
    }
}