using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NAPS2.Remoting.Worker
{
    public class WorkerPool : IDisposable
    {
        private const int TICK_INTERVAL = 5000;
        
        private readonly IWorkerFactory workerFactory;
        private readonly Timer timer;
        private List<PoolEntry> entries = new List<PoolEntry>();

        public WorkerPool(IWorkerFactory workerFactory)
        {
            this.workerFactory = workerFactory;
            timer = new Timer(Tick, null, 0, TICK_INTERVAL);
        }

        private void Tick(object state)
        {
            lock (this)
            {
                var stillUsable = entries
                    .Where(x => x.LastUsed > DateTime.Now - TimeSpan.FromMilliseconds(TICK_INTERVAL)).ToList();
                var expired = entries.Except(stillUsable).ToList();
                entries = stillUsable;
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
                if (entries.Count > 0)
                {
                    var entry = entries[entries.Count - 1];
                    entries.RemoveAt(entries.Count - 1);
                    return entry.Worker;
                }
                return workerFactory.Create();
            }
        }

        private void Return(WorkerContext workerContext)
        {
            lock (this)
            {
                entries.Add(new PoolEntry { Worker = workerContext, LastUsed = DateTime.Now });
            }
        }

        private class PoolEntry
        {
            public WorkerContext Worker { get; set; }
            
            public DateTime LastUsed { get; set; }
        }

        public void Dispose()
        {
            timer.Dispose();
            lock (this)
            {
                foreach (var entry in entries)
                {
                    entry.Worker.Dispose();
                }

                entries.Clear();
            }
        }
    }
}