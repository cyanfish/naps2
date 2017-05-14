using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAPS2.Operation;

namespace NAPS2.Automation
{
    public class ConsoleOperationProgress : IOperationProgress
    {
        public void ShowProgress(IOperation op)
        {
            op.WaitUntilFinished();
        }
    }
}
