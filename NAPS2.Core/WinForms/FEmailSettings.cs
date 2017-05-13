using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.ImportExport.Email;

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

        protected override void OnLoad(object sender, EventArgs e)
        {
            new LayoutManager(this)
                .Bind(btnRestoreDefaults, btnOK, btnCancel)
                    .BottomToForm()
                .Bind(btnOK, btnCancel)
                    .RightToForm()
                .Bind(txtAttachmentName)
                    .WidthToForm()
                .Activate();

            UpdateValues(emailSettingsContainer.EmailSettings);
            cbRememberSettings.Checked = userConfigManager.Config.EmailSettings != null;
        }

        private void UpdateValues(EmailSettings emailSettings)
        {
            txtAttachmentName.Text = emailSettings.AttachmentName;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var emailSettings = new EmailSettings
            {
                AttachmentName = txtAttachmentName.Text
            };

            emailSettingsContainer.EmailSettings = emailSettings;
            userConfigManager.Config.EmailSettings = cbRememberSettings.Checked ? emailSettings : null;
            userConfigManager.Save();

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
    }
}
