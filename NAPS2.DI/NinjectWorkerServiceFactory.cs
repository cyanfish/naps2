using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NAPS2.Recovery;
using NAPS2.Worker;
using Ninject;

namespace NAPS2.DI
{
    public class NinjectWorkerServiceFactory : IWorkerServiceFactory
    {
        private readonly IKernel kernel;

        public NinjectWorkerServiceFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public WorkerContext Create()
        {
            var worker = kernel.Get<WorkerContext>();
            try
            {
                worker.Service.Init(RecoveryImage.RecoveryFolder.FullName);
            }
            catch (EndpointNotFoundException)
            {
                // Retry once
                worker = kernel.Get<WorkerContext>();
                worker.Service.Init(RecoveryImage.RecoveryFolder.FullName);
            }
            return worker;
        }
    }
}
