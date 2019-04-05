using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Config.Experimental;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public partial class FEmailSettings : FormBase
    {
        private readonly SystemEmailClients systemEmailClients;
        private TransactionConfigScope<CommonConfig> userTransact;
        private TransactionConfigScope<CommonConfig> runTransact;
        private ConfigProvider<CommonConfig> transactProvider;

        public FEmailSettings(SystemEmailClients systemEmailClients)
        {
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

            userTransact = ConfigScopes.User.BeginTransaction();
            runTransact = ConfigScopes.Run.BeginTransaction();
            transactProvider = ConfigScopes.WithTransactions(userTransact, runTransact);
            UpdateProvider();
            UpdateValues();
        }

        private void UpdateValues()
        {
            txtAttachmentName.Text = transactProvider.Get(c => c.EmailSettings.AttachmentName);
            cbRememberSettings.Checked = transactProvider.Get(c => c.RememberEmailSettings);
        }

        private void UpdateProvider()
        {
            switch (transactProvider.Get(c => c.EmailSetup.ProviderType))
            {
                case EmailProviderType.Gmail:
                    lblProvider.Text = SettingsResources.EmailProviderType_Gmail + '\n' + transactProvider.Get(c => c.EmailSetup.GmailUser);
                    break;
                case EmailProviderType.OutlookWeb:
                    lblProvider.Text = SettingsResources.EmailProviderType_OutlookWeb + '\n' + transactProvider.Get(c => c.EmailSetup.OutlookWebToken);
                    break;
                case EmailProviderType.CustomSmtp:
                    lblProvider.Text = transactProvider.Get(c => c.EmailSetup.SmtpHost) + '\n' + transactProvider.Get(c => c.EmailSetup.SmtpUser);
                    break;
                case EmailProviderType.System:
                    lblProvider.Text = transactProvider.Get(c => c.EmailSetup.SystemProviderName) ?? systemEmailClients.GetDefaultName();
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

            // Clear old run scope
            runTransact.Set(c => c.EmailSettings = new EmailSettings());

            var scope = cbRememberSettings.Checked ? userTransact : runTransact;
            scope.SetAll(new CommonConfig
            {
                EmailSettings = emailSettings
            });

            userTransact.Commit();
            runTransact.Commit();

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnRestoreDefaults_Click(object sender, EventArgs e)
        {
            runTransact.Set(c => c.EmailSettings = new EmailSettings());
            userTransact.Set(c => c.EmailSettings = new EmailSettings());
            userTransact.Set(c => c.RememberEmailSettings = false);
            UpdateValues();
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
