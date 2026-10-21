using System.Linq.Expressions;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.ImportExport.Email;

namespace NAPS2.EtoForms.Ui;

internal class EmailSettingsPanel : SettingsPanelWithRememberCheckbox<EmailSettings>
{
    private readonly Label _provider = new() { Text = " \n " };
    private readonly FilePathWithPlaceholders _attachmentName;

    public EmailSettingsPanel(IFormBase form) : base(form)
    {
        _attachmentName = new(form);

        LoadValues(Config);
        UpdateProvider(Config);
    }

    public override string Title => UiStrings.EmailSettings;

    public override string IconName => "email";

    protected override Expression<Func<CommonConfig, EmailSettings>> SettingsAccessor => c => c.EmailSettings;

    protected override Expression<Func<CommonConfig, bool>> RememberAccessor => c => c.RememberEmailSettings;

    public override LayoutElement BuildScrollableLayout()
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
            _attachmentName
        );
    }

    protected override EmailSettings SettingsValues
    {
        get =>
            new()
            {
                AttachmentName = _attachmentName.Text ?? ""
            };
        set => _attachmentName.Text = value.AttachmentName;
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

    private void ChangeProvider()
    {
        var form = Form.FormFactory.Create<EmailProviderForm>();
        form.ShowModal();
        UpdateProvider(Config);
        Form.LayoutController.Invalidate();
    }
}