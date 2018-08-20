using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Scan.Wia;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public partial class FScanProgress : FormBase
    {
        private readonly ThreadFactory threadFactory;

        private bool isComplete;
        private bool cancel;

        public FScanProgress(ThreadFactory threadFactory)
        {
            this.threadFactory = threadFactory;

            InitializeComponent();
        }

        public int PageNumber { get; set; }

        public Func<Stream> Transfer { get; set; }

        public Stream ImageStream { get; private set; }

        public Exception Exception { get; private set; }

        public bool OnProgress(int current, int max)
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
            return !cancel;
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(progressBar)
                    .WidthToForm()
                .Bind(btnCancel)
                    .RightToForm()
                .Activate();

            labelPage.Text = string.Format(MiscResources.ScanPageProgress, PageNumber);
        }

        protected override bool ShowWithoutActivation => true;

        private void FScanProgress_Shown(object sender, EventArgs e)
        {
            threadFactory.StartThread(() =>
            {
                try
                {
                    ImageStream = Transfer();
                }
                catch (Exception ex)
                {
                    Exception = ex;
                }
                SafeInvoke(() =>
                {
                    isComplete = true;
                    DialogResult = cancel ? DialogResult.Cancel : DialogResult.OK;
                    Close();
                });
            });
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            cancel = true;
            btnCancel.Enabled = false;
        }

        private void FScanProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isComplete)
            {
                // Prevent simultaneous transfers by refusing to close until the transfer is complete
                e.Cancel = true;
            }
        }
    }
}
