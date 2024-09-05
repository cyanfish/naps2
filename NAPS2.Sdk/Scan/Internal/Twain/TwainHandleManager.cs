#if !MAC
using NTwain;

namespace NAPS2.Scan.Internal.Twain;

/// <summary>
/// Abstracts how HWND handles are obtained for use with TWAIN.
/// </summary>
internal abstract class TwainHandleManager : IDisposable
{
    public static Func<TwainHandleManager> Factory { get; set; } = () =>
    {
#if NET6_0_OR_GREATER
        if (!OperatingSystem.IsWindows()) throw new NotSupportedException();
#endif
        return new DefaultTwainHandleManager();
    };

    protected TwainHandleManager()
    {
    }

    public abstract IntPtr GetDsmHandle(IntPtr dialogParent, bool useNativeUi);

    public abstract IntPtr GetEnableHandle(IntPtr dialogParent, bool useNativeUi);

    public abstract MessageLoopHook CreateMessageLoopHook(IntPtr dialogParent = default, bool useNativeUi = false);

    public abstract IInvoker Invoker { get; }

    public abstract void Dispose();
}
#endif