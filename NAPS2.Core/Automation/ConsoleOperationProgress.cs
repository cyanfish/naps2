using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Operation;

namespace NAPS2.Automation
{
    public class ConsoleOperationProgress : IOperationProgress
    {
        public void ShowProgress(IOperation op)
        {
            op.Success?.Wait();
        }
    }
}
