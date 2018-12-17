using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.Operation
{
    public class StubOperationProgress : OperationProgress
    {
        public override void Attach(IOperation op)
        {
        }

        public override void ShowProgress(IOperation op)
        {
        }

        public override void ShowModalProgress(IOperation op)
        {
        }

        public override void ShowBackgroundProgress(IOperation op)
        {
        }

        public override List<IOperation> ActiveOperations => throw new NotSupportedException();
    }
}