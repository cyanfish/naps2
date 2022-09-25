// ReSharper disable once CheckNamespace
namespace NAPS2.Util;

public class AsyncSink<T> where T : class
{
    private static TaskCompletionSource<T?> CreateTcs() => new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly List<TaskCompletionSource<T?>> _items = new()
    {
        CreateTcs()
    };

    public bool Completed { get; private set; }

    public int ItemCount
    {
        get
        {
            lock (this)
            {
                return _items.Count - 1;
            }
        }
    }

    public AsyncSource<T> AsSource() => new SinkSource(this);

    public void SetCompleted()
    {
        lock (this)
        {
            if (Completed)
            {
                return;
            }
            Completed = true;
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
            if (Completed)
            {
                throw new InvalidOperationException("Sink is already in the completed state");
            }
            Completed = true;
            ex.PreserveStackTrace();
            _items.Last().SetException(ex);
        }
    }

    public virtual void PutItem(T item)
    {
        lock (this)
        {
            _items.Last().SetResult(item);
            _items.Add(CreateTcs());
        }
    }

    private class SinkSource : AsyncSource<T>
    {
        private readonly AsyncSink<T> _sink;
        private int _itemsRead;

        public SinkSource(AsyncSink<T> sink)
        {
            _sink = sink;
        }

        public override async Task<T?> Next()
        {
            TaskCompletionSource<T?> tcs;
            lock (_sink)
            {
                if (_itemsRead >= _sink._items.Count)
                {
                    _itemsRead--;
                }
                tcs = _sink._items[_itemsRead];
            }
            var result = await tcs.Task;
            _itemsRead++;
            return result;
        }
    }
}