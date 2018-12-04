using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.Operation
{
    public class StubOperationProgress : IOperationProgress
    {
        public void Attach(IOperation op)
        {
        }

        public void ShowProgress(IOperation op)
        {
        }

        public void ShowModalProgress(IOperation op)
        {
        }

        public void ShowBackgroundProgress(IOperation op)
        {
        }

        public void RenderStatus(IOperation op, Label textLabel, Label numberLabel, ProgressBar progressBar) => throw new NotSupportedException();

        public List<IOperation> ActiveOperations => throw new NotSupportedException();
    }
}