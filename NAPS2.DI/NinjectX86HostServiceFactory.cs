using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Host;
using Ninject;

namespace NAPS2.DI
{
    public class NinjectX86HostServiceFactory : IX86HostServiceFactory
    {
        private readonly IKernel kernel;

        public NinjectX86HostServiceFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IX86HostService Create()
        {
            return kernel.Get<IX86HostService>();
        }
    }
}
