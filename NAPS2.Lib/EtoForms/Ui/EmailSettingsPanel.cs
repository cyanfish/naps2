using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.ImportExport.Email;

namespace NAPS2.EtoForms.Ui;

internal class EmailSettingsPanel : SettingsPanel
{
    private readonly Label _provider = new() { Text = " \n " };
    private readonly FilePathWithPlaceholders _attachmentName;
    private readonly CheckBox _rememberSettings = new() { Text = UiStrings.RememberTheseSettings };

    public EmailSettingsPanel(IFormBase form) : base(form)
    {
        _attachmentName = new(form);

        UpdateValues(Config);
        UpdateProvider(Config);
    }

    public override LayoutColumn BuildLayout()
    {
        return L.Column(
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
            _rememberSettings
        );
    }

    private void UpdateValues(Naps2Config config)
    {
        _attachmentName.Text = config.Get(c => c.EmailSettings.AttachmentName);
        _rememberSettings.Checked = config.Get(c => c.RememberEmailSettings);
    }

    private void UpdateProvider(Naps2Config config)
    {
        if (!config.User.Has(c => c.EmailSetup.ProviderType))
        {
            _provider.Text = SettingsResources.EmailProvider_NotSelected;
            return;
        }
        switch (config.Get(c => c.EmailSetup.ProviderType))
        {
            case EmailProviderType.Gmail:
                _provider.Text = SettingsResources.EmailProviderType_Gmail + '\n' +
                                 config.Get(c => c.EmailSetup.GmailUser);
                break;
            case EmailProviderType.OutlookNew:
                _provider.Text = SettingsResources.EmailProviderType_OutlookNew;
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
                _provider.Text = config.Get(c => c.EmailSetup.SystemProviderName);
                break;
        }
    }

    public override void Save()
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

    public override void RestoreDefaults()
    {
        UpdateValues(Config.DefaultsOnly);
    }

    private void ChangeProvider()
    {
        var form = Form.FormFactory.Create<EmailProviderForm>();
        form.ShowModal();
        UpdateProvider(Config);
        Form.LayoutController.Invalidate();
    }
}