// ReSharper disable once CheckNamespace
namespace NAPS2.Util;

internal class AsyncSink<T> where T : class
{
    private static TaskCompletionSource<T?> CreateTcs() => new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly List<TaskCompletionSource<T?>> _items = new()
    {
        CreateTcs()
    };
    private bool _completed;

    public async IAsyncEnumerable<T> AsAsyncEnumerable()
    {
        int i = 0;
        while (true)
        {
            TaskCompletionSource<T?> tcs;
            lock (this)
            {
                tcs = _items[i++];
            }
            var item = await tcs.Task;
            if (item == null)
            {
                yield break;
            }
            yield return item;
        }
    }

    public void SetCompleted()
    {
        lock (this)
        {
            if (_completed)
            {
                return;
            }
            _completed = true;
            _items.Last().SetResult(null);
        }
    }

    public void SetError(Exception ex)
    {
        if (ex == null)
        {
            throw new ArgumentNullException(nameof(ex));
        }
        lock (this)
        {
            if (_completed)
            {
                throw new InvalidOperationException("Sink is already in the completed state");
            }
            _completed = true;
            ex.PreserveStackTrace();
            _items.Last().SetException(ex);
        }
    }

    public void PutItem(T item)
    {
        lock (this)
        {
            _items.Last().SetResult(item);
            _items.Add(CreateTcs());
        }
    }
}