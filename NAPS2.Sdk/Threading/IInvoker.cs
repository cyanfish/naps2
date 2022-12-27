namespace NAPS2.Threading;

/// <summary>
/// An interface for synchronized access to the UI thread.
/// </summary>
public interface IInvoker
{
    /// <summary>
    /// Run an action on the UI thread, waiting for completion before returning.
    /// </summary>
    /// <param name="action"></param>
    void Invoke(Action action);

    /// <summary>
    /// Start running an action on the UI thread and immediately return.
    /// </summary>
    /// <param name="action"></param>
    void InvokeAsync(Action action);

    /// <summary>
    /// Run a function on the UI thread, wait for its result, then return that result.
    /// </summary>
    /// <param name="func"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T InvokeGet<T>(Func<T> func);
}