using NAPS2.Config;
using NAPS2.ImportExport.Email;
using System;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class FEmailSettings : FormBase
    {
        private readonly EmailSettingsContainer emailSettingsContainer;
        private readonly IUserConfigManager userConfigManager;

        public FEmailSettings(EmailSettingsContainer emailSettingsContainer, IUserConfigManager userConfigManager)
        {
            this.emailSettingsContainer = emailSettingsContainer;
            this.userConfigManager = userConfigManager;
            InitializeComponent();
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(BtnRestoreDefaults, BtnOK, BtnCancel)
                    .BottomToForm()
                .Bind(BtnOK, BtnCancel)
                    .RightToForm()
                .Bind(TxtAttachmentName)
                    .WidthToForm()
                .Activate();

            UpdateValues(emailSettingsContainer.EmailSettings);
            cbRememberSettings.Checked = userConfigManager.Config.EmailSettings != null;
        }

        private void UpdateValues(EmailSettings emailSettings)
        {
            TxtAttachmentName.Text = emailSettings.AttachmentName;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            var emailSettings = new EmailSettings
            {
                AttachmentName = TxtAttachmentName.Text
            };

            emailSettingsContainer.EmailSettings = emailSettings;
            userConfigManager.Config.EmailSettings = cbRememberSettings.Checked ? emailSettings : null;
            userConfigManager.Save();

            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnRestoreDefaults_Click(object sender, EventArgs e)
        {
            UpdateValues(new EmailSettings());
            cbRememberSettings.Checked = false;
        }

        private void LinkPlaceholders_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var form = FormFactory.Create<FPlaceholders>();
            form.FileName = TxtAttachmentName.Text;
            if (form.ShowDialog() == DialogResult.OK)
            {
                TxtAttachmentName.Text = form.FileName;
            }
        }
    }
}