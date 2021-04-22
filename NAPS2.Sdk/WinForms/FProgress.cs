using System;
using System.Windows.Forms;
using NAPS2.Logging;
using NAPS2.Operation;

namespace NAPS2.WinForms
{
    public partial class FProgress : FormBase
    {
        private readonly ErrorOutput _errorOutput;
        private readonly OperationProgress _operationProgress;

        private volatile bool _loaded;
        private volatile bool _background;
        private IOperation _operation;

        public FProgress(ErrorOutput errorOutput, OperationProgress operationProgress)
        {
            _errorOutput = errorOutput;
            _operationProgress = operationProgress;
            InitializeComponent();

            RestoreFormState = false;
        }

        public IOperation Operation
        {
            get => _operation;
            set
            {
                _operation = value;
                _operation.StatusChanged += operation_StatusChanged;
                _operation.Finished += operation_Finished;
                btnCancel.Visible = _operation.AllowCancel;
            }
        }

        void operation_StatusChanged(object sender, EventArgs e)
        {
            if (_loaded && !_background)
            {
                SafeInvoke(DisplayProgress);
            }
        }

        void operation_Finished(object sender, EventArgs e)
        {
            if (_loaded && !_background)
            {
                SafeInvoke(Close);
            }
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            _loaded = true;
            Text = _operation.ProgressTitle;
            btnRunInBG.Visible = _operation.AllowBackground;

            DisplayProgress();
            if (_operation.IsFinished)
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
            if (!_operation.IsFinished && !_background)
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
            _background = true;
            Close();
        }
    }
}
