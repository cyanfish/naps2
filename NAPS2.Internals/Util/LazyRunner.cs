namespace NAPS2.Util;

/// <summary>
/// Encapsulates an action that should be run once lazily.
/// </summary>
public class LazyRunner
{
    private readonly Lazy<object> _lazy;

    public LazyRunner(Action action)
    {
        _lazy = new Lazy<object>(() =>
        {
            action();
            return new object();
        });
    }

    /// <summary>
    /// Runs the action if it hasn't already been run. If the action is already running, waits for completion.
    /// </summary>
    public void Run()
    {
        var _ = _lazy.Value;
    }
}