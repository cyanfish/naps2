using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.ImportExport.Email.Oauth;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Ui;

public class EmailProviderForm : EtoDialogBase
{
    private readonly SystemEmailClients _systemEmailClients;
    private readonly GmailOauthProvider _gmailOauthProvider;
    private readonly OutlookWebOauthProvider _outlookWebOauthProvider;

    private readonly List<EmailProviderWidget> _providerWidgets;
    private readonly string[] _systemClientNames;
    private readonly string? _defaultSystemClientName;

    public EmailProviderForm(Naps2Config config, SystemEmailClients systemEmailClients,
        GmailOauthProvider gmailOauthProvider, OutlookWebOauthProvider outlookWebOauthProvider) : base(config)
    {
        _systemEmailClients = systemEmailClients;
        _gmailOauthProvider = gmailOauthProvider;
        _outlookWebOauthProvider = outlookWebOauthProvider;

        _providerWidgets = new List<EmailProviderWidget>();
#if NET6_0_OR_GREATER
        if (!OperatingSystem.IsWindowsVersionAtLeast(7))
        {
            _systemClientNames = Array.Empty<string>();
            _defaultSystemClientName = null;
        }
        else
        {
#endif
            _systemClientNames = _systemEmailClients.GetNames();
            _defaultSystemClientName = _systemEmailClients.GetDefaultName();

            foreach (var clientName in _systemClientNames.OrderBy(x => x == _defaultSystemClientName ? 0 : 1))
            {
                var exePath = _systemEmailClients.GetExePath(clientName);
                var icon = exePath == null ? null : EtoPlatform.Current.ExtractAssociatedIcon(exePath);
                _providerWidgets.Add(new EmailProviderWidget
                {
                    ProviderType = EmailProviderType.System,
                    ProviderIcon = icon ?? Icons.mail_yellow.ToEtoImage(),
                    ProviderName = clientName,
                    ClickAction = () => ChooseSystem(clientName)
                });
            }
#if NET6_0_OR_GREATER
        }
#endif

        if (_gmailOauthProvider.HasClientCreds)
        {
            _providerWidgets.Add(new EmailProviderWidget
            {
                ProviderType = EmailProviderType.Gmail,
                ProviderIcon = Icons.gmail.ToEtoImage(),
                ProviderName = EmailProviderType.Gmail.Description(),
                ClickAction = () => ChooseOauth(_gmailOauthProvider)
            });
        }

        if (_outlookWebOauthProvider.HasClientCreds)
        {
            _providerWidgets.Add(new EmailProviderWidget
            {
                ProviderType = EmailProviderType.OutlookWeb,
                ProviderIcon = Icons.outlookweb.ToEtoImage(),
                ProviderName = EmailProviderType.OutlookWeb.Description(),
                ClickAction = () => ChooseOauth(_outlookWebOauthProvider)
            });
        }

        //providerWidgets.Add(new EmailProviderWidget
        //{
        //    ProviderType = EmailProviderType.CustomSmtp,
        //    ProviderIcon = Icons.email_setting,
        //    ProviderName = EmailProviderType.CustomSmtp.Description(),
        //    ClickAction = ChooseCustomSmtp
        //});

        // Put the configured provider at the top
        var defaultWidget = GetDefaultWidget();
        if (defaultWidget != null)
        {
            _providerWidgets.Remove(defaultWidget);
            _providerWidgets.Insert(0, defaultWidget);
        }
    }

    protected override void BuildLayout()
    {
        Title = UiStrings.EmailProviderFormTitle;
        Icon = new Icon(1f, Icons.email_small.ToEtoImage());

        FormStateController.FixedHeightLayout = true;

        LayoutController.DefaultSpacing = 0;
        LayoutController.Content = L.Column(
            _providerWidgets.Select(x => C.Button(new ActionCommand(x.ClickAction)
            {
                Text = x.ProviderName,
                Image = x.ProviderIcon
            }, ButtonImagePosition.Left, big: true).NaturalWidth(500).Height(50)).Expand()
        );
    }

    public bool Result { get; private set; }

    private void ChooseSystem(string clientName)
    {
        var transact = Config.User.BeginTransaction();
        transact.Remove(c => c.EmailSetup);
        transact.Set(c => c.EmailSetup.SystemProviderName, clientName);
        transact.Set(c => c.EmailSetup.ProviderType, EmailProviderType.System);
        transact.Commit();
        Result = true;
        Close();
    }

    private void ChooseOauth(OauthProvider provider)
    {
        var authForm = FormFactory.Create<AuthorizeForm>();
        authForm.OauthProvider = provider;
        authForm.ShowModal();
        if (authForm.Result)
        {
            Result = true;
            Close();
        }
    }

    private EmailProviderWidget? GetDefaultWidget()
    {
        var emailSetup = Config.Get(c => c.EmailSetup);
        foreach (var widget in _providerWidgets)
        {
            if (widget.ProviderType == emailSetup.ProviderType)
            {
                if (widget.ProviderType == EmailProviderType.System)
                {
                    // System providers need additional logic since there may be more than one
                    if (widget.ProviderName == emailSetup.SystemProviderName
                        || string.IsNullOrEmpty(emailSetup.SystemProviderName) &&
                        widget.ProviderName == _defaultSystemClientName)
                    {
                        return widget;
                    }
                }
                else
                {
                    return widget;
                }
            }
        }
        return null;
    }

    public class EmailProviderWidget
    {
        public required EmailProviderType ProviderType { get; init; }
        public required Bitmap ProviderIcon { get; init; }
        public required string ProviderName { get; init; }
        public required Action ClickAction { get; init; }
    }
}