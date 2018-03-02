using System;
using System.Collections.Generic;
using System.Linq;
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

        public IWorkerService Create()
        {
            var service = kernel.Get<IWorkerService>();
            service.Init();
            return service;
        }
    }
}
