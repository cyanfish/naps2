using NAPS2.Config;
using NAPS2.ImportExport.Pdf;
using System;
using System.IO;
using System.Windows.Forms;

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

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(BtnOK, BtnCancel, CbShowOwnerPassword, CbShowUserPassword, BtnChooseFolder)
                    .RightToForm()
                .Bind(groupMetadata, groupProtection, groupCompat, clbPerms)
                    .WidthToForm()
                .Bind(TxtDefaultFilePath, txtTitle, TxtAuthor, TxtSubject, txtKeywords, txtOwnerPassword, txtUserPassword)
                    .WidthToForm()
                .Activate();

            UpdateValues(pdfSettingsContainer.PdfSettings);
            UpdateEnabled();
            cbRememberSettings.Checked = userConfigManager.Config.PdfSettings != null;
        }

        private void UpdateValues(PdfSettings pdfSettings)
        {
            TxtDefaultFilePath.Text = pdfSettings.DefaultFileName;
            cbSkipSavePrompt.Checked = pdfSettings.SkipSavePrompt;
            txtTitle.Text = pdfSettings.Metadata.Title;
            TxtAuthor.Text = pdfSettings.Metadata.Author;
            TxtSubject.Text = pdfSettings.Metadata.Subject;
            txtKeywords.Text = pdfSettings.Metadata.Keywords;
            CbEncryptPdf.Checked = pdfSettings.Encryption.EncryptPdf;
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
            cbSkipSavePrompt.Enabled = Path.IsPathRooted(TxtDefaultFilePath.Text);

            bool encrypt = CbEncryptPdf.Checked;
            txtUserPassword.Enabled = txtOwnerPassword.Enabled = CbShowOwnerPassword.Enabled = CbShowUserPassword.Enabled =
                lblUserPassword.Enabled = lblOwnerPassword.Enabled = encrypt;
            clbPerms.Enabled = encrypt;

            cmbCompat.Enabled = appConfigManager.Config.ForcePdfCompat == PdfCompat.Default;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            var pdfSettings = new PdfSettings
            {
                DefaultFileName = TxtDefaultFilePath.Text,
                SkipSavePrompt = cbSkipSavePrompt.Checked,
                Metadata =
                {
                    Title = txtTitle.Text,
                    Author = TxtAuthor.Text,
                    Subject = TxtSubject.Text,
                    Keywords = txtKeywords.Text
                },
                Encryption =
                {
                    EncryptPdf = CbEncryptPdf.Checked,
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

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnRestoreDefaults_Click(object sender, EventArgs e)
        {
            UpdateValues(new PdfSettings());
            UpdateEnabled();
            cbRememberSettings.Checked = false;
        }

        private void TxtDefaultFilePath_TextChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void CbEncryptPdf_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void CbShowOwnerPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtOwnerPassword.UseSystemPasswordChar = !CbShowOwnerPassword.Checked;
        }

        private void CbShowUserPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtUserPassword.UseSystemPasswordChar = !CbShowUserPassword.Checked;
        }

        private void LinkPlaceholders_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var form = FormFactory.Create<FPlaceholders>();
            form.FileName = TxtDefaultFilePath.Text;
            if (form.ShowDialog() == DialogResult.OK)
            {
                TxtDefaultFilePath.Text = form.FileName;
            }
        }

        private void BtnChooseFolder_Click(object sender, EventArgs e)
        {
            if (dialogHelper.PromptToSavePdf(TxtDefaultFilePath.Text, out string savePath))
            {
                TxtDefaultFilePath.Text = savePath;
            }
        }
    }
}