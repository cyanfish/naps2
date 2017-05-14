using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAPS2.Operation;
using NAPS2.Util;

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
