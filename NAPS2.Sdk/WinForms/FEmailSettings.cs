using System;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public partial class FEmailSettings : FormBase
    {
        private readonly SystemEmailClients _systemEmailClients;
        private TransactionConfigScope<CommonConfig> _userTransact;
        private TransactionConfigScope<CommonConfig> _runTransact;
        private ConfigProvider<CommonConfig> _transactProvider;

        public FEmailSettings(SystemEmailClients systemEmailClients)
        {
            _systemEmailClients = systemEmailClients;
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

            _userTransact = ConfigScopes.User.BeginTransaction();
            _runTransact = ConfigScopes.Run.BeginTransaction();
            _transactProvider = ConfigProvider.Replace(ConfigScopes.User, _userTransact).Replace(ConfigScopes.Run, _runTransact);
            UpdateProvider();
            UpdateValues();
        }

        private void UpdateValues()
        {
            txtAttachmentName.Text = _transactProvider.Get(c => c.EmailSettings.AttachmentName);
            cbRememberSettings.Checked = _transactProvider.Get(c => c.RememberEmailSettings);
        }

        private void UpdateProvider()
        {
            switch (_transactProvider.Get(c => c.EmailSetup.ProviderType))
            {
                case EmailProviderType.Gmail:
                    lblProvider.Text = SettingsResources.EmailProviderType_Gmail + '\n' + _transactProvider.Get(c => c.EmailSetup.GmailUser);
                    break;
                case EmailProviderType.OutlookWeb:
                    lblProvider.Text = SettingsResources.EmailProviderType_OutlookWeb + '\n' + _transactProvider.Get(c => c.EmailSetup.OutlookWebToken);
                    break;
                case EmailProviderType.CustomSmtp:
                    lblProvider.Text = _transactProvider.Get(c => c.EmailSetup.SmtpHost) + '\n' + _transactProvider.Get(c => c.EmailSetup.SmtpUser);
                    break;
                case EmailProviderType.System:
                    lblProvider.Text = _transactProvider.Get(c => c.EmailSetup.SystemProviderName) ?? _systemEmailClients.GetDefaultName();
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
            _runTransact.Set(c => c.EmailSettings = new EmailSettings());

            var scope = cbRememberSettings.Checked ? _userTransact : _runTransact;
            scope.SetAll(new CommonConfig
            {
                EmailSettings = emailSettings
            });

            _userTransact.Commit();
            _runTransact.Commit();

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnRestoreDefaults_Click(object sender, EventArgs e)
        {
            _runTransact.Set(c => c.EmailSettings = new EmailSettings());
            _userTransact.Set(c => c.EmailSettings = new EmailSettings());
            _userTransact.Set(c => c.RememberEmailSettings = false);
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
