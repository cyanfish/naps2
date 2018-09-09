using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Operation;

namespace NAPS2.WinForms
{
    public partial class OperationProgressNotifyWidget : NotifyWidgetBase
    {
        private readonly IOperationProgress operationProgress;
        private readonly IOperation op;

        public OperationProgressNotifyWidget(IOperationProgress operationProgress, IOperation op)
        {
            InitializeComponent();

            this.operationProgress = operationProgress;
            this.op = op;

            cancelToolStripMenuItem.Visible = op.AllowCancel;
            op.StatusChanged += Op_StatusChanged;
            op.Finished += Op_Finished;
            DisplayProgress();
        }

        private void DisplayProgress()
        {
            var status = op.Status ?? new OperationStatus();
            lblTitle.Text = status.StatusText;
            if (status.MaxProgress == 1 || status.IndeterminateProgress)
            {
                progressBar.Style = ProgressBarStyle.Marquee;
            }
            else if (status.MaxProgress == 0)
            {
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Maximum = 1;
                progressBar.Value = 0;
            }
            else
            {
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Maximum = status.MaxProgress;
                progressBar.Value = status.CurrentProgress;
            }
            // Force the progress bar to render immediately
            if (progressBar.Value < progressBar.Maximum)
            {
                progressBar.Value += 1;
                progressBar.Value -= 1;
            }
            Width = Math.Max(Width, lblTitle.Width + 22);
            Height = Math.Max(Height, lblTitle.Height + 35);
        }

        private void DoHideNotify()
        {
            op.StatusChanged -= Op_StatusChanged;
            op.Finished -= Op_Finished;
            InvokeHideNotify();
        }

        private void Op_StatusChanged(object sender, EventArgs e)
        {
            SafeInvoke(DisplayProgress);
        }

        private void Op_Finished(object sender, EventArgs e)
        {
            DoHideNotify();
        }

        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            op.Cancel();
            cancelToolStripMenuItem.Enabled = false;
        }

        private void OperationProgressNotifyWidget_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DoHideNotify();
                operationProgress.ShowModalProgress(op);
            }
        }
    }
}
