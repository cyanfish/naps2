using System.Threading;
using NAPS2.Scan;

namespace NAPS2.Remoting.Worker;

internal class WorkerPool : IDisposable
{
    private const int TICK_INTERVAL = 5000;

    private readonly ScanningContext _scanningContext;
    private readonly Timer _timer;
    private List<PoolEntry> _entries = new();

    public WorkerPool(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
        _timer = new Timer(Tick, null, 0, TICK_INTERVAL);
    }

    private void Tick(object? state)
    {
        lock (this)
        {
            var stillUsable = _entries
                .Where(x => x.LastUsed > DateTime.Now - TimeSpan.FromMilliseconds(TICK_INTERVAL)).ToList();
            var expired = _entries.Except(stillUsable).ToList();
            _entries = stillUsable;
            foreach (var entry in expired)
            {
                entry.Worker.Dispose();
            }
        }
    }

    public T Use<T>(WorkerType workerType, Func<WorkerContext, T> func)
    {
        if (workerType != WorkerType.Native)
        {
            throw new NotSupportedException("WorkerPool only supports native workers");
        }
        var worker = Take();
        T result;
        try
        {
            result = func(worker);
        }
        catch (Exception)
        {
            worker.Dispose();
            throw;
        }
        Return(worker);
        return result;
    }

    private WorkerContext Take()
    {
        lock (this)
        {
            if (_entries.Count > 0)
            {
                var entry = _entries[_entries.Count - 1];
                _entries.RemoveAt(_entries.Count - 1);
                return entry.Worker;
            }
            return _scanningContext.CreateWorker(WorkerType.Native)!;
        }
    }

    private void Return(WorkerContext workerContext)
    {
        lock (this)
        {
            _entries.Add(new PoolEntry(workerContext, DateTime.Now));
        }
    }

    private record PoolEntry(WorkerContext Worker, DateTime LastUsed);

    public void Dispose()
    {
        _timer.Dispose();
        lock (this)
        {
            foreach (var entry in _entries)
            {
                entry.Worker.Dispose();
            }

            _entries.Clear();
        }
    }
}