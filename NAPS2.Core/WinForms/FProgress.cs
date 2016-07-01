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

        private volatile bool loaded;
        private volatile bool finished;
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
                operation.StatusChanged += operation_StatusChanged;
                operation.Error += operation_Error;
                operation.Finished += operation_Finished;
                btnCancel.Visible = operation.AllowCancel;
            }
        }

        public Func<bool> Start { get; set; }

        void operation_Error(object sender, OperationErrorEventArgs e)
        {
            Invoke(() => errorOutput.DisplayError(e.ErrorMessage, e.Exception));
        }

        void operation_StatusChanged(object sender, EventArgs e)
        {
            if (loaded)
            {
                Invoke(DisplayProgress);
            }
        }

        void operation_Finished(object sender, EventArgs e)
        {
            finished = true;
            if (loaded)
            {
                Invoke(Close);
            }
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(progressBar, labelStatus)
                    .WidthToForm()
                .Bind(btnCancel)
                    .RightToForm()
                .Activate();

            loaded = true;
            Text = operation.ProgressTitle;

            DisplayProgress();
            if (finished)
            {
                Close();
            }
        }

        private void FProgress_Shown(object sender, EventArgs e)
        {
            if (Start != null)
            {
                if (!Start())
                {
                    finished = true;
                    Close();
                }
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
                progressBar.Value = 0;
                progressBar.Maximum = 1;
            }
            else
            {
                labelNumber.Text = string.Format(MiscResources.ProgressFormat, status.CurrentProgress, status.MaxProgress);
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = status.CurrentProgress;
                progressBar.Maximum = status.MaxProgress;
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
