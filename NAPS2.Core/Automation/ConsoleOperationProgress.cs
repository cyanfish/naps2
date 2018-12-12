using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Operation;
using NAPS2.WinForms;

namespace NAPS2.Automation
{
    public class ConsoleOperationProgress : IOperationProgress
    {
        private readonly IFormFactory formFactory;

        public ConsoleOperationProgress(IFormFactory formFactory)
        {
            this.formFactory = formFactory;
        }

        public void Attach(IOperation op)
        {
        }

        public void ShowProgress(IOperation op)
        {
            op.Wait();
        }

        public void ShowModalProgress(IOperation op)
        {
            if (!op.IsFinished)
            {
                var form = formFactory.Create<FProgress>();
                form.Operation = op;
                form.ShowDialog();
            }
            op.Wait();
        }

        public void ShowBackgroundProgress(IOperation op) {
        }

        public List<IOperation> ActiveOperations => throw new InvalidOperationException();
    }
}
