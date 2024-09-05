#if !MAC
using NAPS2.Platform.Windows;
using NTwain;

namespace NAPS2.Scan.Internal.Twain;

/// <summary>
/// A MessageLoopHook implementation that uses Win32 methods directly, with no dependencies on WinForms or WPF.
/// </summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
internal class Win32MessageLoopHook : MessageLoopHook
{
    private readonly Win32MessagePump _messagePump;

    public Win32MessageLoopHook(Win32MessagePump messagePump, IntPtr dsmHandle)
    {
        _messagePump = messagePump;
        Handle = dsmHandle;
    }

    public override void Invoke(Action action) => _messagePump.Invoke(action);

    public override void BeginInvoke(Action action) => _messagePump.InvokeDispatch(action);

    protected override void Start(IWinMessageFilter filter) => _messagePump.Filter = filter.IsTwainMessage;

    protected override void Stop() => _messagePump.Filter = null;
}
#endif