using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Util;
using System;
using System.Windows.Forms;

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
            get => operation;
            set
            {
                operation = value;
                operation.StatusChanged += Operation_StatusChanged;
                operation.Error += Operation_Error;
                operation.Finished += Operation_Finished;
                BtnCancel.Visible = Operation.AllowCancel;
            }
        }

        public Func<bool> Start { get; set; }

        private void Operation_Error(object sender, OperationErrorEventArgs e)
        {
            SafeInvoke(() => errorOutput.DisplayError(e.ErrorMessage, e.Exception));
        }

        private void Operation_StatusChanged(object sender, EventArgs e)
        {
            if (loaded)
            {
                SafeInvoke(DisplayProgress);
            }
        }

        private void Operation_Finished(object sender, EventArgs e)
        {
            finished = true;
            if (loaded)
            {
                SafeInvoke(Close);
            }
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(progressBar, LabelStatus)
                    .WidthToForm()
                .Bind(BtnCancel)
                    .RightToForm()
                .Activate();

            loaded = true;
            Text = Operation.ProgressTitle;

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
            LabelStatus.Text = status.StatusText;
            if (status.MaxProgress == 1 || status.IndeterminateProgress)
            {
                LabelNumber.Text = "";
                progressBar.Style = ProgressBarStyle.Marquee;
            }
            else if (status.MaxProgress == 0)
            {
                LabelNumber.Text = "";
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = 0;
                progressBar.Maximum = 1;
            }
            else
            {
                LabelNumber.Text = string.Format(MiscResources.ProgressFormat, status.CurrentProgress, status.MaxProgress);
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = status.CurrentProgress;
                progressBar.Maximum = status.MaxProgress;
            }
            // Force the progress bar to render immediately
            if (progressBar.Value < progressBar.Maximum)
            {
                progressBar.Value++;
                progressBar.Value--;
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
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
                BtnCancel.Enabled = false;
            }
        }
    }
}