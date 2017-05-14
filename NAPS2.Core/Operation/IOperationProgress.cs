using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Operation
{
    public interface IOperationProgress
    {
        void ShowProgress(IOperation op);
    }
}
