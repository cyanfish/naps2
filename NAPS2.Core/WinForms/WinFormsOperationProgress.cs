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

        private readonly List<IOperation> activeOperations = new List<IOperation>();

        public WinFormsOperationProgress(IFormFactory formFactory, NotificationManager notificationManager)
        {
            this.formFactory = formFactory;
            this.notificationManager = notificationManager;
        }

        public void ShowProgress(IOperation op)
        {
            lock (this)
            {
                activeOperations.Add(op);
                op.Finished += (sender, args) => activeOperations.Remove(op);
                if (op.IsFinished) activeOperations.Remove(op);
            }

            var form = formFactory.Create<FProgress>();
            form.Operation = op;
            form.ShowDialog();

            if (!op.IsFinished)
            {
                SynchronizationContext.Current.Send(s => notificationManager.OperationProgress(this, op), null);
            }
        }

        public List<IOperation> ActiveOperations
        {
            get
            {
                lock (activeOperations)
                {
                    return activeOperations.ToList();
                }
            }
        }
    }
}
