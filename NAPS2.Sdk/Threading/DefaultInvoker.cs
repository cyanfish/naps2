namespace NAPS2.Threading;

/// <summary>
/// A default implementation for synchronized access to the UI thread that assumes there is no privileged thread.
/// </summary>
internal class DefaultInvoker : IInvoker
{
    public void Invoke(Action action) => action();

    public void InvokeDispatch(Action action) => Task.Run(action);

    public T InvokeGet<T>(Func<T> func) => func();
}