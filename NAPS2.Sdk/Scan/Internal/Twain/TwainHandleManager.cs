namespace NAPS2.Scan.Internal.Twain;

public class TwainHandleManager : IDisposable
{
    public static Func<TwainHandleManager> Factory { get; set; } = () => new TwainHandleManager();

    protected TwainHandleManager()
    {
    }

    public virtual IntPtr GetDsmHandle(IntPtr dialogParent, bool useNativeUi)
    {
        return dialogParent;
    }

    public virtual IntPtr GetEnableHandle(IntPtr dialogParent, bool useNativeUi)
    {
        return dialogParent;
    }

    public virtual void Dispose()
    {
    }
}