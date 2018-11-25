using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public partial class FEmailSettings : FormBase
    {
        private readonly EmailSettingsContainer emailSettingsContainer;
        private readonly SystemEmailClients systemEmailClients;

        public FEmailSettings(EmailSettingsContainer emailSettingsContainer, SystemEmailClients systemEmailClients)
        {
            this.emailSettingsContainer = emailSettingsContainer;
            this.systemEmailClients = systemEmailClients;
            InitializeComponent();
        }

        protected override void OnLoad(object sender, EventArgs e)
        {
            new LayoutManager(this)
                .Bind(btnRestoreDefaults, btnOK, btnCancel)
                    .BottomToForm()
                .Bind(btnOK, btnCancel, btnChangeProvider)
                    .RightToForm()
                .Bind(txtAttachmentName, groupBox1)
                    .WidthToForm()
                .Activate();

            UpdateProvider();
            UpdateValues(emailSettingsContainer.EmailSettings);
            cbRememberSettings.Checked = UserConfigManager.Config.EmailSettings != null;
        }

        private void UpdateValues(EmailSettings emailSettings)
        {
            txtAttachmentName.Text = emailSettings.AttachmentName;
        }

        private void UpdateProvider()
        {
            var setup = UserConfigManager.Config.EmailSetup;
            switch (setup?.ProviderType)
            {
                case EmailProviderType.Gmail:
                    lblProvider.Text = SettingsResources.EmailProviderType_Gmail + '\n' + setup.GmailUser;
                    break;
                case EmailProviderType.OutlookWeb:
                    lblProvider.Text = SettingsResources.EmailProviderType_OutlookWeb + '\n' + setup.OutlookWebUser;
                    break;
                case EmailProviderType.CustomSmtp:
                    lblProvider.Text = setup.SmtpHost + '\n' + setup.SmtpUser;
                    break;
                case EmailProviderType.System:
                    lblProvider.Text = setup.SystemProviderName ?? systemEmailClients.GetDefaultName();
                    break;
                default:
                    lblProvider.Text = SettingsResources.EmailProvider_NotSelected;
                    break;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var emailSettings = new EmailSettings
            {
                AttachmentName = txtAttachmentName.Text
            };

            emailSettingsContainer.EmailSettings = emailSettings;
            UserConfigManager.Config.EmailSettings = cbRememberSettings.Checked ? emailSettings : null;
            UserConfigManager.Save();

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnRestoreDefaults_Click(object sender, EventArgs e)
        {
            UpdateValues(new EmailSettings());
            cbRememberSettings.Checked = false;
        }

        private void linkPlaceholders_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var form = FormFactory.Create<FPlaceholders>();
            form.FileName = txtAttachmentName.Text;
            if (form.ShowDialog() == DialogResult.OK)
            {
                txtAttachmentName.Text = form.FileName;
            }
        }

        private void btnChangeProvider_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FEmailProvider>();
            if (form.ShowDialog() == DialogResult.OK)
            {
                UpdateProvider();
            }
        }
    }
}
