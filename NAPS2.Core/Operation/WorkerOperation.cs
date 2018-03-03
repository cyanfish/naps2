using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Worker;

namespace NAPS2.Operation
{
    public abstract class WorkerOperation : OperationBase
    {
        protected WorkerOperation(IWorkerServiceFactory workerServiceFactory)
        {
            WorkerServiceFactory = workerServiceFactory;
        }

        protected bool UseWorker => !Environment.Is64BitProcess;

        protected IWorkerServiceFactory WorkerServiceFactory { get; }
    }
}