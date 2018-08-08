using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NAPS2.ImportExport.Email.Oauth;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public partial class FAuthorize : FormBase
    {
        private readonly ThreadFactory threadFactory;

        private CancellationTokenSource cancelTokenSource;

        public FAuthorize(ThreadFactory threadFactory)
        {
            this.threadFactory = threadFactory;

            RestoreFormState = false;
            InitializeComponent();
        }

        public OauthProvider OauthProvider { get; set; }

        private void FAuthorize_Load(object sender, EventArgs e)
        {
            MaximumSize = new Size(Math.Max(lblWaiting.Width + 142, 272), Height);
            MinimumSize = new Size(Math.Max(lblWaiting.Width + 142, 272), Height);

            cancelTokenSource = new CancellationTokenSource();
            threadFactory.StartThread(() =>
            {
                try
                {
                    OauthProvider.AcquireToken(cancelTokenSource.Token);
                    Invoke(() =>
                    {
                        DialogResult = DialogResult.OK;
                        Close();
                    });
                }
                catch (OperationCanceledException)
                {
                }
            });
        }

        private void FAuthorize_FormClosed(object sender, FormClosedEventArgs e)
        {
            cancelTokenSource?.Cancel();
        }
    }
}
