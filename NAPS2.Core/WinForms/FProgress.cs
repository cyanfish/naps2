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
        private readonly IOperationProgress operationProgress;

        private volatile bool loaded;
        private volatile bool background;
        private IOperation operation;

        public FProgress(IErrorOutput errorOutput, IOperationProgress operationProgress)
        {
            this.errorOutput = errorOutput;
            this.operationProgress = operationProgress;
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
                operation.Finished += operation_Finished;
                btnCancel.Visible = operation.AllowCancel;
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
            WinFormsOperationProgress.RenderStatus(Operation, labelStatus, labelNumber, progressBar);
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
            Close();
        }
    }
}
