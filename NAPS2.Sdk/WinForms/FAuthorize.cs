using System;
using System.Drawing;
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
        private readonly ErrorOutput _errorOutput;
        private CancellationTokenSource? _cancelTokenSource;

        public FAuthorize(ErrorOutput errorOutput)
        {
            _errorOutput = errorOutput;
            RestoreFormState = false;
            InitializeComponent();
        }

        public OauthProvider? OauthProvider { get; set; }

        private void FAuthorize_Load(object sender, EventArgs e)
        {
            if (OauthProvider == null) throw new InvalidOperationException("OauthProvider must be specified");
            
            MaximumSize = new Size(Math.Max(lblWaiting.Width + 142, 272), Height);
            MinimumSize = new Size(Math.Max(lblWaiting.Width + 142, 272), Height);

            _cancelTokenSource = new CancellationTokenSource();
            Task.Run(() =>
            {
                try
                {
                    OauthProvider.AcquireToken(_cancelTokenSource.Token);
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
                    _errorOutput.DisplayError(MiscResources.AuthError, ex);
                    Log.ErrorException("Error acquiring Oauth token", ex);
                    Invoke(() =>
                    {
                        DialogResult = DialogResult.Cancel;
                        Close();
                    });
                }
            });
        }

        private void FAuthorize_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cancelTokenSource?.Cancel();
        }
    }
}
