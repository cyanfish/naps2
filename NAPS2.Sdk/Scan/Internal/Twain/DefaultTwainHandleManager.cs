#if !MAC
using System.Threading;
using NAPS2.Platform.Windows;
using NTwain;

namespace NAPS2.Scan.Internal.Twain;

/// <summary>
/// TwainHandleManager implementation that lazily starts a Win32MessagePump and delegates to a Win32TwainHandleManager.
/// </summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
internal class DefaultTwainHandleManager : TwainHandleManager
{
    private Win32MessagePump? _messagePump;
    private Win32TwainHandleManager? _inner;

    private Win32TwainHandleManager CreateInner()
    {
        var mre = new ManualResetEvent(false);
        var messageThread = new Thread(() =>
        {
            _messagePump = Win32MessagePump.Create();
            mre.Set();
            _messagePump.RunMessageLoop();
        });
        messageThread.SetApartmentState(ApartmentState.STA);
        messageThread.Start();
        mre.WaitOne();
        return new Win32TwainHandleManager(_messagePump!);
    }

    public override IntPtr GetDsmHandle(IntPtr dialogParent, bool useNativeUi)
    {
        _inner ??= CreateInner();
        return _inner.GetDsmHandle(dialogParent, useNativeUi);
    }

    public override IntPtr GetEnableHandle(IntPtr dialogParent, bool useNativeUi)
    {
        _inner ??= CreateInner();
        return _inner.GetEnableHandle(dialogParent, useNativeUi);
    }

    public override MessageLoopHook CreateMessageLoopHook(IntPtr dialogParent = default, bool useNativeUi = false)
    {
        _inner ??= CreateInner();
        return _inner.CreateMessageLoopHook(dialogParent, useNativeUi);
    }

    public override IInvoker Invoker
    {
        get
        {
            _inner ??= CreateInner();
            return _inner.Invoker;
        }
    }

    public override void Dispose()
    {
        _inner?.Dispose();
        _messagePump?.Dispose();
    }
}
#endif