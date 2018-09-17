using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public partial class FProgress : FormBase
    {
        private readonly IErrorOutput errorOutput;

        private volatile bool loaded;
        private volatile bool background;
        private IOperation operation;

        public FProgress(IErrorOutput errorOutput)
        {
            this.errorOutput = errorOutput;
            InitializeComponent();

            RestoreFormState = false;
        }

        public IOperation Operation
        {
            get => operation;
            set
            {
                operation = value;
                operation.StatusChanged += operation_StatusChanged;
                operation.Error += operation_Error;
                operation.Finished += operation_Finished;
                btnCancel.Visible = operation.AllowCancel;
            }
        }

        void operation_Error(object sender, OperationErrorEventArgs e)
        {
            // TODO: Operation error for background
            if (!background)
            {
                SafeInvoke(() => errorOutput.DisplayError(e.ErrorMessage, e.Exception));
            }
        }

        void operation_StatusChanged(object sender, EventArgs e)
        {
            if (loaded && !background)
            {
                SafeInvoke(DisplayProgress);
            }
        }

        void operation_Finished(object sender, EventArgs e)
        {
            if (loaded && !background)
            {
                SafeInvoke(Close);
            }
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            loaded = true;
            Text = operation.ProgressTitle;
            btnRunInBG.Visible = operation.AllowBackground;

            DisplayProgress();
            if (operation.IsFinished)
            {
                Close();
            }
        }

        private void DisplayProgress()
        {
            var status = Operation.Status ?? new OperationStatus();
            labelStatus.Text = status.StatusText;
            if (status.MaxProgress == 1 || status.IndeterminateProgress)
            {
                labelNumber.Text = "";
                progressBar.Style = ProgressBarStyle.Marquee;
            }
            else if (status.MaxProgress == 0)
            {
                labelNumber.Text = "";
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Maximum = 1;
                progressBar.Value = 0;
            }
            else
            {
                labelNumber.Text = status.ProgressType == OperationProgressType.MB
                    ? string.Format(MiscResources.SizeProgress, (status.CurrentProgress / 1000000.0).ToString("f1"), (status.MaxProgress / 1000000.0).ToString("f1"))
                    : string.Format(MiscResources.ProgressFormat, status.CurrentProgress, status.MaxProgress);
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
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            TryCancelOp();
        }

        private void FDownloadProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!operation.IsFinished && !background)
            {
                TryCancelOp();
                e.Cancel = true;
            }
        }

        private void TryCancelOp()
        {
            if (Operation.AllowCancel)
            {
                Operation.Cancel();
                btnCancel.Enabled = false;
            }
        }

        private void btnRunInBG_Click(object sender, EventArgs e)
        {
            background = true;
            Hide();
        }
    }
}
