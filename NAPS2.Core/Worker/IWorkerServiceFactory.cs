using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Worker
{
    /// <summary>
    /// A factory interface to spawn NAPS2.Worker.exe instances as needed.
    /// </summary>
    public interface IWorkerServiceFactory
    {
        WorkerContext Create();
    }
}
