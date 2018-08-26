using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.WinForms;

namespace NAPS2.Operation
{
    public class OperationManager
    {
        private readonly IFormFactory formFactory;
        private readonly NotificationManager notificationManager;

        public OperationManager(IFormFactory formFactory, NotificationManager notificationManager)
        {
            this.formFactory = formFactory;
            this.notificationManager = notificationManager;
        }

        public void Attach(IOperation op)
        {
            var form = formFactory.Create<FProgress>();
            form.Operation = op;
            form.ShowDialog();

            if (!form.Operation.Status.Finished)
            {
                notificationManager.OperationProgress(this, op);
            }
        }
    }
}
