using System.Drawing;
using System.Windows.Forms;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.ImportExport.Email.Oauth;
using NAPS2.Scan;

namespace NAPS2.WinForms;

public partial class FEmailProvider : FormBase
{
    private readonly GmailOauthProvider _gmailOauthProvider;
    private readonly OutlookWebOauthProvider _outlookWebOauthProvider;
    private readonly SystemEmailClients _systemEmailClients;

    private List<EmailProviderWidget> _providerWidgets;
    private string[] _systemClientNames;
    private string _defaultSystemClientName;

    public FEmailProvider(GmailOauthProvider gmailOauthProvider, OutlookWebOauthProvider outlookWebOauthProvider, SystemEmailClients systemEmailClients)
    {
        _gmailOauthProvider = gmailOauthProvider;
        _outlookWebOauthProvider = outlookWebOauthProvider;
        _systemEmailClients = systemEmailClients;

        InitializeComponent();
    }

    private void FEmailProvider_Load(object sender, EventArgs e)
    {
        _providerWidgets = new List<EmailProviderWidget>();
        _systemClientNames = _systemEmailClients.GetNames();
        _defaultSystemClientName = _systemEmailClients.GetDefaultName();

        foreach (var clientName in _systemClientNames.OrderBy(x => x == _defaultSystemClientName ? 0 : 1))
        {
            var exePath = _systemEmailClients.GetExePath(clientName);
            var icon = exePath == null ? null : Icon.ExtractAssociatedIcon(exePath);
            var bitmap = icon?.ToBitmap();
            _providerWidgets.Add(new EmailProviderWidget
            {
                ProviderType = EmailProviderType.System,
                ProviderIcon = bitmap ?? Icons.mail_yellow.ToBitmap(),
                ProviderName = clientName,
                ClickAction = () => ChooseSystem(clientName)
            });
        }

        if (_gmailOauthProvider.HasClientCreds)
        {
            _providerWidgets.Add(new EmailProviderWidget
            {
                ProviderType = EmailProviderType.Gmail,
                ProviderIcon = Icons.gmail.ToBitmap(),
                ProviderName = EmailProviderType.Gmail.Description(),
                ClickAction = () => ChooseOauth(_gmailOauthProvider)
            });
        }

        if (_outlookWebOauthProvider.HasClientCreds)
        {
            _providerWidgets.Add(new EmailProviderWidget
            {
                ProviderType = EmailProviderType.OutlookWeb,
                ProviderIcon = Icons.outlookweb.ToBitmap(),
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

        ShowWidgets();
    }

    private void ChooseSystem(string clientName)
    {
        var emailSetup = Config.Get(c => c.EmailSetup);
        emailSetup.SystemProviderName = clientName;
        emailSetup.ProviderType = EmailProviderType.System;
        Config.User.Set(c => c.EmailSetup, emailSetup);
        DialogResult = DialogResult.OK;
        Close();
    }

    private void ChooseOauth(OauthProvider provider)
    {
        var authForm = FormFactory.Create<FAuthorize>();
        authForm.OauthProvider = provider;
        if (authForm.ShowDialog() == DialogResult.OK)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    private void ShowWidgets()
    {
        int heightDiff = Height - panel1.Height;
        panel1.Height = 0;
        foreach (var widget in _providerWidgets)
        {
            panel1.Controls.Add(widget);
            widget.Top = panel1.Height;
            widget.Width = panel1.Width;
            widget.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Height += widget.Height - 1;
        }

        panel1.Height += 1;
        MaximumSize = new Size(MaximumSize.Width, panel1.Height + heightDiff);
        MinimumSize = new Size(MinimumSize.Width, panel1.Height + heightDiff);
    }

    private EmailProviderWidget GetDefaultWidget()
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
                        || string.IsNullOrEmpty(emailSetup.SystemProviderName) && widget.ProviderName == _defaultSystemClientName)
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
}