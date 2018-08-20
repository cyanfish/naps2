using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Worker
{
    public class WorkerContext : IDisposable
    {
        public IWorkerService Service { get; set; }

        public IWorkerCallback Callback { get; set; }

        public void Dispose()
        {
            ((IDisposable)Service)?.Dispose();
        }
    }
}
