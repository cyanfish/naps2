using System.Threading;
using Eto.Drawing;
using NAPS2.EtoForms.Layout;
using NAPS2.ImportExport.Email.Oauth;

namespace NAPS2.EtoForms.Ui;

public class AuthorizeForm : EtoDialogBase
{
    private readonly ErrorOutput _errorOutput;
    private CancellationTokenSource? _cancelTokenSource;

    public AuthorizeForm(Naps2Config config, IIconProvider iconProvider, ErrorOutput errorOutput) : base(config)
    {
        Title = UiStrings.AuthorizeFormTitle;
        Icon = iconProvider.GetFormIcon("key_small");

        _errorOutput = errorOutput;
    }

    protected override void BuildLayout()
    {
        FormStateController.FixedHeightLayout = true;
        FormStateController.RestoreFormState = false;
        FormStateController.Resizable = false;

        LayoutController.Content = L.Row(
            C.Label(UiStrings.WaitingForAuthorization).Padding(right: 30),
            C.CancelButton(this)
        );
    }

    public OauthProvider? OauthProvider { get; set; }

    public bool Result { get; private set; }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (OauthProvider == null) throw new InvalidOperationException("OauthProvider must be specified");

        _cancelTokenSource = new CancellationTokenSource();
        Task.Run(() =>
        {
            try
            {
                OauthProvider.AcquireToken(_cancelTokenSource.Token);
                Invoker.Current.Invoke(() =>
                {
                    Result = true;
                    Close();
                });
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (!_cancelTokenSource.IsCancellationRequested)
                {
                    _errorOutput.DisplayError(MiscResources.AuthError, ex);
                    Log.ErrorException("Error acquiring Oauth token", ex);
                }
                Invoker.Current.Invoke(Close);
            }
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _cancelTokenSource?.Cancel();
    }
}