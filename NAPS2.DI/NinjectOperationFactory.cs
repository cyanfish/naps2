using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Operation;
using Ninject;

namespace NAPS2.DI
{
    public class NinjectOperationFactory : IOperationFactory
    {
        private readonly IKernel kernel;

        public NinjectOperationFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public T Create<T>() where T : IOperation
        {
            var form = kernel.Get<T>();
            return form;
        }
    }
}
