using NTwain;

namespace NAPS2.Scan.Internal.Twain;

internal class TwainHandleManager : IDisposable
{
    public static Func<TwainHandleManager> Factory { get; set; } = () => new TwainHandleManager();

    protected TwainHandleManager()
    {
    }

    public virtual IntPtr GetDsmHandle(IntPtr dialogParent, bool useNativeUi) => dialogParent;

    public virtual IntPtr GetEnableHandle(IntPtr dialogParent, bool useNativeUi) => dialogParent;

    public virtual MessageLoopHook CreateMessageLoopHook(IntPtr dialogParent = default, bool useNativeUi = false) =>
        throw new NotSupportedException();

    public virtual void Dispose()
    {
    }
}