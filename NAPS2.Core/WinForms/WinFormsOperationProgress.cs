using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Config;
using NAPS2.Operation;

namespace NAPS2.WinForms
{
    public class WinFormsOperationProgress : IOperationProgress
    {
        private readonly IFormFactory formFactory;
        private readonly NotificationManager notificationManager;
        private readonly IUserConfigManager userConfigManager;

        private readonly HashSet<IOperation> activeOperations = new HashSet<IOperation>();

        public WinFormsOperationProgress(IFormFactory formFactory, NotificationManager notificationManager, IUserConfigManager userConfigManager)
        {
            this.formFactory = formFactory;
            this.notificationManager = notificationManager;
            this.userConfigManager = userConfigManager;
        }

        public void Attach(IOperation op)
        {
            lock (this)
            {
                if (!activeOperations.Contains(op))
                {
                    activeOperations.Add(op);
                    op.Finished += (sender, args) => activeOperations.Remove(op);
                    if (op.IsFinished) activeOperations.Remove(op);
                }
            }
        }

        public void ShowProgress(IOperation op)
        {
            if (userConfigManager.Config.BackgroundOperations.Contains(op.GetType().Name))
            {
                ShowBackgroundProgress(op);
            }
            else
            {
                ShowModalProgress(op);
            }
        }

        public void ShowModalProgress(IOperation op)
        {
            Attach(op);

            userConfigManager.Config.BackgroundOperations.Remove(op.GetType().Name);
            userConfigManager.Save();

            if (!op.IsFinished)
            {
                var form = formFactory.Create<FProgress>();
                form.Operation = op;
                form.ShowDialog();
            }

            if (!op.IsFinished)
            {
                ShowBackgroundProgress(op);
            }
        }

        public void ShowBackgroundProgress(IOperation op)
        {
            Attach(op);

            userConfigManager.Config.BackgroundOperations.Add(op.GetType().Name);
            userConfigManager.Save();

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
