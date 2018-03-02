using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NAPS2.Recovery;
using NAPS2.Scan;

namespace NAPS2.Worker
{
    public static class WorkerServiceExtensions
    {
        public static void Close(this IWorkerService service)
        {
            (service as IDisposable)?.Dispose();
        }
    }
}
