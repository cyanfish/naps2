using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.ImportExport.Pdf;

namespace NAPS2.WinForms
{
    public partial class FPdfSettings : FormBase
    {
        private readonly PdfSettingsContainer pdfSettingsContainer;
        private readonly IUserConfigManager userConfigManager;
        private readonly DialogHelper dialogHelper;
        private readonly AppConfigManager appConfigManager;

        public FPdfSettings(PdfSettingsContainer pdfSettingsContainer, IUserConfigManager userConfigManager, DialogHelper dialogHelper, AppConfigManager appConfigManager)
        {
            this.pdfSettingsContainer = pdfSettingsContainer;
            this.userConfigManager = userConfigManager;
            this.dialogHelper = dialogHelper;
            this.appConfigManager = appConfigManager;
            InitializeComponent();
            AddEnumItems<PdfCompat>(cmbCompat);
        }

        protected override void OnLoad(object sender, EventArgs e)
        {
            new LayoutManager(this)
                .Bind(btnOK, btnCancel, cbShowOwnerPassword, cbShowUserPassword, btnChooseFolder)
                    .RightToForm()
                .Bind(groupMetadata, groupProtection, groupCompat, clbPerms)
                    .WidthToForm()
                .Bind(txtDefaultFilePath, txtTitle, txtAuthor, txtSubject, txtKeywords, txtOwnerPassword, txtUserPassword)
                    .WidthToForm()
                .Activate();

            UpdateValues(pdfSettingsContainer.PdfSettings);
            UpdateEnabled();
            cbRememberSettings.Checked = userConfigManager.Config.PdfSettings != null;
        }

        private void UpdateValues(PdfSettings pdfSettings)
        {
            txtDefaultFilePath.Text = pdfSettings.DefaultFileName;
            cbSkipSavePrompt.Checked = pdfSettings.SkipSavePrompt;
            cbSinglePagePdf.Checked = pdfSettings.SinglePagePdf;
            txtTitle.Text = pdfSettings.Metadata.Title;
            txtAuthor.Text = pdfSettings.Metadata.Author;
            txtSubject.Text = pdfSettings.Metadata.Subject;
            txtKeywords.Text = pdfSettings.Metadata.Keywords;
            cbEncryptPdf.Checked = pdfSettings.Encryption.EncryptPdf;
            txtOwnerPassword.Text = pdfSettings.Encryption.OwnerPassword;
            txtUserPassword.Text = pdfSettings.Encryption.UserPassword;
            clbPerms.SetItemChecked(0, pdfSettings.Encryption.AllowPrinting);
            clbPerms.SetItemChecked(1, pdfSettings.Encryption.AllowFullQualityPrinting);
            clbPerms.SetItemChecked(2, pdfSettings.Encryption.AllowDocumentModification);
            clbPerms.SetItemChecked(3, pdfSettings.Encryption.AllowDocumentAssembly);
            clbPerms.SetItemChecked(4, pdfSettings.Encryption.AllowContentCopying);
            clbPerms.SetItemChecked(5, pdfSettings.Encryption.AllowContentCopyingForAccessibility);
            clbPerms.SetItemChecked(6, pdfSettings.Encryption.AllowAnnotations);
            clbPerms.SetItemChecked(7, pdfSettings.Encryption.AllowFormFilling);
            var forced = appConfigManager.Config.ForcePdfCompat;
            cmbCompat.SelectedIndex = (int)(forced == PdfCompat.Default ? pdfSettings.Compat : forced);
        }

        private void UpdateEnabled()
        {
            cbSkipSavePrompt.Enabled = Path.IsPathRooted(txtDefaultFilePath.Text);

            bool encrypt = cbEncryptPdf.Checked;
            txtUserPassword.Enabled = txtOwnerPassword.Enabled = cbShowOwnerPassword.Enabled = cbShowUserPassword.Enabled =
                lblUserPassword.Enabled = lblOwnerPassword.Enabled = encrypt;
            clbPerms.Enabled = encrypt;

            cmbCompat.Enabled = appConfigManager.Config.ForcePdfCompat == PdfCompat.Default;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var pdfSettings = new PdfSettings
            {
                DefaultFileName = txtDefaultFilePath.Text,
                SkipSavePrompt = cbSkipSavePrompt.Checked,
                SinglePagePdf = cbSinglePagePdf.Checked,
                Metadata =
                {
                    Title = txtTitle.Text,
                    Author = txtAuthor.Text,
                    Subject = txtSubject.Text,
                    Keywords = txtKeywords.Text
                },
                Encryption =
                {
                    EncryptPdf = cbEncryptPdf.Checked,
                    OwnerPassword = txtOwnerPassword.Text,
                    UserPassword = txtUserPassword.Text,
                    AllowPrinting = clbPerms.GetItemChecked(0),
                    AllowFullQualityPrinting = clbPerms.GetItemChecked(1),
                    AllowDocumentModification = clbPerms.GetItemChecked(2),
                    AllowDocumentAssembly = clbPerms.GetItemChecked(3),
                    AllowContentCopying = clbPerms.GetItemChecked(4),
                    AllowContentCopyingForAccessibility = clbPerms.GetItemChecked(5),
                    AllowAnnotations = clbPerms.GetItemChecked(6),
                    AllowFormFilling = clbPerms.GetItemChecked(7)
                },
                Compat = (PdfCompat)cmbCompat.SelectedIndex
            };

            pdfSettingsContainer.PdfSettings = pdfSettings;
            userConfigManager.Config.PdfSettings = cbRememberSettings.Checked ? pdfSettings : null;
            userConfigManager.Save();

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnRestoreDefaults_Click(object sender, EventArgs e)
        {
            UpdateValues(new PdfSettings());
            UpdateEnabled();
            cbRememberSettings.Checked = false;
        }

        private void txtDefaultFilePath_TextChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void cbEncryptPdf_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void cbShowOwnerPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtOwnerPassword.UseSystemPasswordChar = !cbShowOwnerPassword.Checked;
        }

        private void cbShowUserPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtUserPassword.UseSystemPasswordChar = !cbShowUserPassword.Checked;
        }

        private void linkPlaceholders_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var form = FormFactory.Create<FPlaceholders>();
            form.FileName = txtDefaultFilePath.Text;
            if (form.ShowDialog() == DialogResult.OK)
            {
                txtDefaultFilePath.Text = form.FileName;
            }
        }

        private void btnChooseFolder_Click(object sender, EventArgs e)
        {
            if (dialogHelper.PromptToSavePdf(txtDefaultFilePath.Text, out string savePath))
            {
                txtDefaultFilePath.Text = savePath;
            }
        }
    }
}
