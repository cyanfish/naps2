using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public partial class FScanProgress : FormBase
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _isComplete;

        public FScanProgress()
        {
            CancelToken = _cts.Token;

            SaveFormState = false;
            RestoreFormState = false;
            InitializeComponent();
        }

        public int PageNumber { get; set; }

        public Func<Stream> Transfer { get; set; }

        public Func<Task> AsyncTransfer { get; set; }

        public Stream ImageStream { get; private set; }

        public Exception Exception { get; private set; }

        public CancellationToken CancelToken { get; private set; }

        public void OnProgress(int current, int max)
        {
            if (current > 0)
            {
                SafeInvoke(() =>
                {
                    progressBar.Style = ProgressBarStyle.Continuous;
                    progressBar.Maximum = max;
                    progressBar.Value = current;
                });
            }
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(progressBar)
                    .WidthToForm()
                .Bind(btnCancel)
                    .RightToForm()
                .Activate();

            RefreshStatus();
        }

        public void RefreshStatus()
        {
            labelPage.Text = string.Format(MiscResources.ScanPageProgress, PageNumber);
        }

        protected override bool ShowWithoutActivation => true;

        private void FScanProgress_Shown(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (Transfer != null)
                    {
                        ImageStream = Transfer();
                    }
                    if (AsyncTransfer != null)
                    {
                        await AsyncTransfer();
                    }
                }
                catch (Exception ex)
                {
                    Exception = ex;
                }
                SafeInvoke(() =>
                {
                    _isComplete = true;
                    DialogResult = CancelToken.IsCancellationRequested ? DialogResult.Cancel : DialogResult.OK;
                    Close();
                });
            });
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cts.Cancel();
            btnCancel.Enabled = false;
        }

        private void FScanProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_isComplete)
            {
                // Prevent simultaneous transfers by refusing to close until the transfer is complete
                e.Cancel = true;
            }
        }
    }
}
