using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Operation
{
    public interface IOperationFactory
    {
        T Create<T>() where T : IOperation;
    }
}
