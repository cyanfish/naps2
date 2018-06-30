﻿using NAPS2.Scan;
using Ninject;

namespace NAPS2.DI
{
    public class NinjectScanDriverFactory : IScanDriverFactory
    {
        private readonly IKernel kernel;

        public NinjectScanDriverFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IScanDriver Create(string driverName)
        {
            return kernel.Get<IScanDriver>(driverName);
        }
    }
}