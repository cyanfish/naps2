using System.Threading;

namespace NAPS2.Threading;

public class SyncContextInvoker : IInvoker
{
    private readonly SynchronizationContext _current;

    public SyncContextInvoker(SynchronizationContext current)
    {
        _current = current;
    }

    public void Invoke(Action action)
    {
        _current.Send(_ => action(), null);
    }

    public void InvokeAsync(Action action)
    {
        _current.Post(_ => action(), null);
    }

    public T InvokeGet<T>(Func<T> func)
    {
        T value = default!;
        _current.Send(_ => value = func(), null);
        return value;
    }
}