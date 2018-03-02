using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Worker
{
    public interface IWorkerServiceFactory
    {
        IWorkerService Create();
    }
}
