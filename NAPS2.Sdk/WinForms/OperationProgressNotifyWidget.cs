using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class OperationProgressNotifyWidget : NotifyWidgetBase
    {
        private readonly OperationProgress _operationProgress;
        private readonly IOperation _op;

        public OperationProgressNotifyWidget(OperationProgress operationProgress, IOperation op)
        {
            InitializeComponent();

            _operationProgress = operationProgress;
            _op = op;

            cancelToolStripMenuItem.Visible = op.AllowCancel;
            op.StatusChanged += Op_StatusChanged;
            op.Finished += Op_Finished;
        }

        public override void ShowNotify() => DisplayProgress();

        public override NotifyWidgetBase Clone() => new OperationProgressNotifyWidget(_operationProgress, _op);

        private void DisplayProgress()
        {
            var lblNumberRight = lblNumber.Right;
            WinFormsOperationProgress.RenderStatus(_op, lblTitle, lblNumber, progressBar);
            if (_op.Status?.IndeterminateProgress != true)
            {
                // Don't display the number if the progress bar is precise
                // Otherwise, the widget will be too cluttered
                // The number is only shown for OcrOperation at the moment
                lblNumber.Text = "";
            }
            lblNumber.Left = lblNumberRight - lblNumber.Width;
            Width = Math.Max(Width, lblTitle.Width + lblNumber.Width + 22);
            Height = Math.Max(Height, lblTitle.Height + 35);
        }

        private void DoHideNotify()
        {
            _op.StatusChanged -= Op_StatusChanged;
            _op.Finished -= Op_Finished;
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
            _op.Cancel();
            cancelToolStripMenuItem.Enabled = false;
        }

        private void OperationProgressNotifyWidget_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DoHideNotify();
                _operationProgress.ShowModalProgress(_op);
            }
        }
    }
}
