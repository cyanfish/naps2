using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.ImportExport.Email.Oauth;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public partial class FAuthorize : FormBase
    {
        private readonly IErrorOutput errorOutput;
        private CancellationTokenSource cancelTokenSource;

        public FAuthorize(IErrorOutput errorOutput)
        {
            this.errorOutput = errorOutput;
            RestoreFormState = false;
            InitializeComponent();
        }

        public OauthProvider OauthProvider { get; set; }

        private void FAuthorize_Load(object sender, EventArgs e)
        {
            MaximumSize = new Size(Math.Max(lblWaiting.Width + 142, 272), Height);
            MinimumSize = new Size(Math.Max(lblWaiting.Width + 142, 272), Height);

            cancelTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
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
                catch (Exception ex)
                {
                    errorOutput.DisplayError(MiscResources.AuthError, ex);
                    Log.ErrorException("Error acquiring Oauth token", ex);
                    Invoke(() =>
                    {
                        DialogResult = DialogResult.Cancel;
                        Close();
                    });
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void FAuthorize_FormClosed(object sender, FormClosedEventArgs e)
        {
            cancelTokenSource?.Cancel();
        }
    }
}
