using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Operation;

namespace NAPS2.Automation
{
    public class ConsoleOperationProgress : OperationProgress
    {
        public override void Attach(IOperation op)
        {
        }

        public override void ShowProgress(IOperation op)
        {
            op.Wait();
        }

        public override void ShowModalProgress(IOperation op)
        {
        }

        public override void ShowBackgroundProgress(IOperation op) {
        }

        public override void RenderStatus(IOperation op, Label textLabel, Label numberLabel, ProgressBar progressBar) => throw new NotSupportedException();

        public override List<IOperation> ActiveOperations => throw new NotSupportedException();
    }
}
