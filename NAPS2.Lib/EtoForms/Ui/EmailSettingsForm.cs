using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;

namespace NAPS2.EtoForms.Ui;

internal class EmailSettingsForm : EtoDialogBase
{
    private readonly SystemEmailClients _systemEmailClients;

    private readonly Label _provider = new() { Text = " \n " };
    private readonly FilePathWithPlaceholders _attachmentName;
    private readonly CheckBox _rememberSettings = new() { Text = UiStrings.RememberTheseSettings };
    private readonly Button _restoreDefaults = new() { Text = UiStrings.RestoreDefaults };

    public EmailSettingsForm(Naps2Config config, SystemEmailClients systemEmailClients, IIconProvider iconProvider) :
        base(config)
    {
        Title = UiStrings.EmailSettingsFormTitle;
        IconName = "email_small";

        _systemEmailClients = systemEmailClients;
        _attachmentName = new(this);

        UpdateValues(Config);
        UpdateProvider(Config);

        _restoreDefaults.Click += RestoreDefaults_Click;
    }

    protected override void BuildLayout()
    {
        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);
        FormStateController.FixedHeightLayout = true;

        LayoutController.Content = L.Column(
            L.GroupBox(
                UiStrings.Provider,
                L.Row(
                    _provider.DynamicWrap(EtoPlatform.Current.IsGtk ? 150 : 0).AlignCenter(),
                    C.Filler(),
                    C.Button(UiStrings.Change, ChangeProvider).AlignCenter().Padding(top: 4, bottom: 4)
                )
            ),
            C.Label(UiStrings.AttachmentNameLabel),
            _attachmentName,
            C.Filler(),
            _rememberSettings,
            L.Row(
                _restoreDefaults.MinWidth(140),
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, Save),
                    C.CancelButton(this))
            )
        );
    }

    private void UpdateValues(Naps2Config config)
    {
        _attachmentName.Text = config.Get(c => c.EmailSettings.AttachmentName);
        _rememberSettings.Checked = config.Get(c => c.RememberEmailSettings);
    }

    private void UpdateProvider(Naps2Config config)
    {
        switch (config.Get(c => c.EmailSetup.ProviderType))
        {
            case EmailProviderType.Gmail:
                _provider.Text = SettingsResources.EmailProviderType_Gmail + '\n' +
                                 config.Get(c => c.EmailSetup.GmailUser);
                break;
            case EmailProviderType.OutlookWeb:
                _provider.Text = SettingsResources.EmailProviderType_OutlookWeb + '\n' +
                                 config.Get(c => c.EmailSetup.OutlookWebUser);
                break;
            case EmailProviderType.Thunderbird:
                _provider.Text = SettingsResources.EmailProviderType_Thunderbird;
                break;
            case EmailProviderType.AppleMail:
                _provider.Text = SettingsResources.EmailProviderType_AppleMail;
                break;
            case EmailProviderType.CustomSmtp:
                _provider.Text = config.Get(c => c.EmailSetup.SmtpHost) + '\n' + config.Get(c => c.EmailSetup.SmtpUser);
                break;
            case EmailProviderType.System:
#if NET6_0_OR_GREATER
                if (!OperatingSystem.IsWindowsVersionAtLeast(7))
                {
                    _provider.Text = SettingsResources.EmailProvider_NotSelected;
                    break;
                }
#endif
                _provider.Text = config.Get(c => c.EmailSetup.SystemProviderName) ??
                                 _systemEmailClients.GetDefaultName();
                break;
            default:
                _provider.Text = SettingsResources.EmailProvider_NotSelected;
                break;
        }
    }

    private void Save()
    {
        var emailSettings = new EmailSettings
        {
            AttachmentName = _attachmentName.Text ?? ""
        };

        var runTransact = Config.Run.BeginTransaction();
        var userTransact = Config.User.BeginTransaction();
        bool remember = _rememberSettings.IsChecked();
        var transactToWrite = remember ? userTransact : runTransact;

        runTransact.Remove(c => c.EmailSettings);
        userTransact.Remove(c => c.EmailSettings);
        transactToWrite.Set(c => c.EmailSettings, emailSettings);
        userTransact.Set(c => c.RememberEmailSettings, remember);

        runTransact.Commit();
        userTransact.Commit();
    }

    private void RestoreDefaults_Click(object? sender, EventArgs e)
    {
        UpdateValues(Config.DefaultsOnly);
    }

    private void ChangeProvider()
    {
        var form = FormFactory.Create<EmailProviderForm>();
        form.ShowModal();
        UpdateProvider(Config);
        LayoutController.Invalidate();
    }
}