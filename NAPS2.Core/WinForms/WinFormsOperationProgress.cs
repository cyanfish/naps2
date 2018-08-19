using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Operation;

namespace NAPS2.WinForms
{
    public class WinFormsOperationProgress : IOperationProgress
    {
        private readonly IFormFactory formFactory;

        public WinFormsOperationProgress(IFormFactory formFactory)
        {
            this.formFactory = formFactory;
        }

        public void ShowProgress(IOperation op)
        {
            var form = formFactory.Create<FProgress>();
            form.Operation = op;
            form.ShowDialog();
        }
    }
}
