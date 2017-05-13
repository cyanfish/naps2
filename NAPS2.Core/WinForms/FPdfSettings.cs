using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.ImportExport.Pdf;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public partial class FPdfSettings : FormBase
    {
        private readonly PdfSettingsContainer pdfSettingsContainer;
        private readonly IUserConfigManager userConfigManager;
        private readonly DialogHelper dialogHelper;

        public FPdfSettings(PdfSettingsContainer pdfSettingsContainer, IUserConfigManager userConfigManager, DialogHelper dialogHelper)
        {
            this.pdfSettingsContainer = pdfSettingsContainer;
            this.userConfigManager = userConfigManager;
            this.dialogHelper = dialogHelper;
            InitializeComponent();
        }

        protected override void OnLoad(object sender, EventArgs e)
        {
            new LayoutManager(this)
                .Bind(btnOK, btnCancel, cbShowOwnerPassword, cbShowUserPassword, btnChooseFolder)
                    .RightToForm()
                .Bind(groupMetadata, groupProtection)
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
            txtTitle.Text = pdfSettings.Metadata.Title;
            txtAuthor.Text = pdfSettings.Metadata.Author;
            txtSubject.Text = pdfSettings.Metadata.Subject;
            txtKeywords.Text = pdfSettings.Metadata.Keywords;
            cbEncryptPdf.Checked = pdfSettings.Encryption.EncryptPdf;
            txtOwnerPassword.Text = pdfSettings.Encryption.OwnerPassword;
            txtUserPassword.Text = pdfSettings.Encryption.UserPassword;
            cbAllowContentCopyingForAccessibility.Checked = pdfSettings.Encryption.AllowContentCopyingForAccessibility;
            cbAllowAnnotations.Checked = pdfSettings.Encryption.AllowAnnotations;
            cbAllowDocumentAssembly.Checked = pdfSettings.Encryption.AllowDocumentAssembly;
            cbAllowContentCopying.Checked = pdfSettings.Encryption.AllowContentCopying;
            cbAllowFormFilling.Checked = pdfSettings.Encryption.AllowFormFilling;
            cbAllowFullQualityPrinting.Checked = pdfSettings.Encryption.AllowFullQualityPrinting;
            cbAllowDocumentModification.Checked = pdfSettings.Encryption.AllowDocumentModification;
            cbAllowPrinting.Checked = pdfSettings.Encryption.AllowPrinting;
        }

        private void UpdateEnabled()
        {
            cbSkipSavePrompt.Enabled = Path.IsPathRooted(txtDefaultFilePath.Text);

            bool encrypt = cbEncryptPdf.Checked;
            txtUserPassword.Enabled = txtOwnerPassword.Enabled = cbShowOwnerPassword.Enabled = cbShowUserPassword.Enabled =
                lblUserPassword.Enabled = lblOwnerPassword.Enabled = encrypt;
            cbAllowAnnotations.Enabled =
                cbAllowContentCopying.Enabled = cbAllowContentCopyingForAccessibility.Enabled =
                    cbAllowDocumentAssembly.Enabled = cbAllowDocumentModification.Enabled = cbAllowFormFilling.Enabled =
                        cbAllowFullQualityPrinting.Enabled = cbAllowPrinting.Enabled = encrypt;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var pdfSettings = new PdfSettings
            {
                DefaultFileName = txtDefaultFilePath.Text,
                SkipSavePrompt = cbSkipSavePrompt.Checked,
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
                    AllowContentCopyingForAccessibility = cbAllowContentCopyingForAccessibility.Checked,
                    AllowAnnotations = cbAllowAnnotations.Checked,
                    AllowDocumentAssembly = cbAllowDocumentAssembly.Checked,
                    AllowContentCopying = cbAllowContentCopying.Checked,
                    AllowFormFilling = cbAllowFormFilling.Checked,
                    AllowFullQualityPrinting = cbAllowFullQualityPrinting.Checked,
                    AllowDocumentModification = cbAllowDocumentModification.Checked,
                    AllowPrinting = cbAllowPrinting.Checked
                }
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
            string savePath;
            if (dialogHelper.PromptToSavePdf(txtDefaultFilePath.Text, out savePath))
            {
                txtDefaultFilePath.Text = savePath;
            }
        }
    }
}
