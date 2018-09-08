using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Operation;

namespace NAPS2.WinForms
{
    public class WinFormsOperationProgress : IOperationProgress
    {
        private readonly IFormFactory formFactory;
        private readonly NotificationManager notificationManager;

        public WinFormsOperationProgress(IFormFactory formFactory, NotificationManager notificationManager)
        {
            this.formFactory = formFactory;
            this.notificationManager = notificationManager;
        }

        public void ShowProgress(IOperation op)
        {
            var form = formFactory.Create<FProgress>();
            form.Operation = op;
            form.ShowDialog();

            if (!form.Operation.IsFinished)
            {
                SynchronizationContext.Current.Send(s => notificationManager.OperationProgress(this, op), null);
            }
        }
    }
}
