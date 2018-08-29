using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Worker
{
    /// <summary>
    /// A class storing the objects the client needs to use a NAPS2.Worker.exe instance.
    /// </summary>
    public class WorkerContext : IDisposable
    {
        public IWorkerService Service { get; set; }

        public void Dispose()
        {
            ((IDisposable)Service)?.Dispose();
        }
    }
}
