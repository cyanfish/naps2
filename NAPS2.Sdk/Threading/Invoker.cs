namespace NAPS2.Threading;

// TODO: Can we get rid of this static context?
/// <summary>
/// Synchronized access to the UI thread.
/// </summary>
internal static class Invoker
{
    private static IInvoker _current = new DefaultInvoker();

    /// <summary>
    /// Gets or sets the current implementation of synchronized access to the UI thread.
    /// </summary>
    public static IInvoker Current
    {
        get => _current;
        set => _current = value ?? throw new ArgumentNullException(nameof(value));
    }
}