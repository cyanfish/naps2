using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.Lang.Resources;
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
                notificationManager.ParentForm.SafeInvoke(() => notificationManager.OperationProgress(this, op));
            }
        }

        public static void RenderStatus(IOperation op, Label textLabel, Label numberLabel, ProgressBar progressBar)
        {
            var status = op.Status ?? new OperationStatus();
            textLabel.Text = status.StatusText;
            progressBar.Style = status.MaxProgress == 1 || status.IndeterminateProgress
                ? ProgressBarStyle.Marquee
                : ProgressBarStyle.Continuous;
            if (status.MaxProgress == 1 || status.ProgressType == OperationProgressType.None)
            {
                numberLabel.Text = "";
            }
            else if (status.MaxProgress == 0)
            {
                numberLabel.Text = "";
                progressBar.Maximum = 1;
                progressBar.Value = 0;
            }
            else if (status.ProgressType == OperationProgressType.BarOnly)
            {
                numberLabel.Text = "";
                progressBar.Maximum = status.MaxProgress;
                progressBar.Value = status.CurrentProgress;
            }
            else
            {
                numberLabel.Text = status.ProgressType == OperationProgressType.MB
                    ? string.Format(MiscResources.SizeProgress, (status.CurrentProgress / 1000000.0).ToString("f1"), (status.MaxProgress / 1000000.0).ToString("f1"))
                    : string.Format(MiscResources.ProgressFormat, status.CurrentProgress, status.MaxProgress);
                progressBar.Maximum = status.MaxProgress;
                progressBar.Value = status.CurrentProgress;
            }
            // Force the progress bar to render immediately
            if (progressBar.Value < progressBar.Maximum)
            {
                progressBar.Value += 1;
                progressBar.Value -= 1;
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
