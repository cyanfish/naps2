using System.Threading;

namespace NAPS2.Remoting.Worker;

public class WorkerPool : IDisposable
{
    private const int TICK_INTERVAL = 5000;
        
    private readonly IWorkerFactory _workerFactory;
    private readonly Timer _timer;
    private List<PoolEntry> _entries = new List<PoolEntry>();

    public WorkerPool(IWorkerFactory workerFactory)
    {
        _workerFactory = workerFactory;
        _timer = new Timer(Tick, null, 0, TICK_INTERVAL);
    }

    private void Tick(object state)
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

    public T Use<T>(Func<WorkerContext, T> func)
    {
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
            return _workerFactory.Create();
        }
    }

    private void Return(WorkerContext workerContext)
    {
        lock (this)
        {
            _entries.Add(new PoolEntry { Worker = workerContext, LastUsed = DateTime.Now });
        }
    }

    private class PoolEntry
    {
        public WorkerContext Worker { get; set; }
            
        public DateTime LastUsed { get; set; }
    }

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