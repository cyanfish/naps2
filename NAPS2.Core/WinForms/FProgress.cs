using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.ImportExport;
using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public partial class FProgress : FormBase
    {
        private readonly IErrorOutput errorOutput;

        private bool finished;
        private IOperation operation;

        public FProgress(IErrorOutput errorOutput)
        {
            this.errorOutput = errorOutput;
            InitializeComponent();

            RestoreFormState = false;
        }

        public IOperation Operation
        {
            get { return operation; }
            set
            {
                operation = value;
                Text = operation.ProgressTitle;
                operation.StatusChanged += operation_StatusChanged;
                operation.Error += operation_Error;
                operation.Finished += operation_Finished;
                btnCancel.Visible = operation.AllowCancel;
            }
        }

        void operation_Error(object sender, OperationErrorEventArgs e)
        {
            Invoke(new Action(() => errorOutput.DisplayError(e.ErrorMessage)));
        }

        void operation_StatusChanged(object sender, EventArgs e)
        {
            Invoke(new Action(DisplayProgress));
        }

        void operation_Finished(object sender, EventArgs e)
        {
            finished = true;
            Invoke(new Action(Close));
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(progressBar, labelStatus)
                    .WidthToForm()
                .Bind(btnCancel)
                    .RightToForm()
                .Activate();

            DisplayProgress();
            if (finished)
            {
                Close();
            }
        }

        private void DisplayProgress()
        {
            labelStatus.Text = Operation.Status.StatusText;
            if (Operation.Status.MaxProgress == 1 || Operation.Status.IndeterminateProgress)
            {
                labelNumber.Text = "";
                progressBar.Style = ProgressBarStyle.Marquee;
            }
            else if (Operation.Status.MaxProgress == 0)
            {
                labelNumber.Text = "";
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = 0;
                progressBar.Maximum = 1;
            }
            else
            {
                labelNumber.Text = string.Format(MiscResources.Progress, Operation.Status.CurrentProgress, Operation.Status.MaxProgress);
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = Operation.Status.CurrentProgress;
                progressBar.Maximum = Operation.Status.MaxProgress;
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
            if (!finished)
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
    }
}
