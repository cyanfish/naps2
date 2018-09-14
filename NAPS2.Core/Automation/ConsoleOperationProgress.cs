using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Operation;

namespace NAPS2.Automation
{
    public class ConsoleOperationProgress : IOperationProgress
    {
        public void Attach(IOperation op)
        {
        }

        public void ShowProgress(IOperation op)
        {
            op.Wait();
        }

        public void ShowModalProgress(IOperation op) => throw new InvalidOperationException();

        public void ShowBackgroundProgress(IOperation op) {
        }

        public List<IOperation> ActiveOperations => throw new InvalidOperationException();
    }
}
