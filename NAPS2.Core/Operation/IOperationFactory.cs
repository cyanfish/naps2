using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Operation
{
    public interface IOperationFactory
    {
        T Create<T>() where T : IOperation;
    }
}
