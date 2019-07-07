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
        private readonly DialogHelper dialogHelper;
        private TransactionConfigScope<CommonConfig> userTransact;
        private TransactionConfigScope<CommonConfig> runTransact;
        private ConfigProvider<CommonConfig> transactProvider;

        public FPdfSettings(DialogHelper dialogHelper)
        {
            this.dialogHelper = dialogHelper;
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

            userTransact = ConfigScopes.User.BeginTransaction();
            runTransact = ConfigScopes.Run.BeginTransaction();
            transactProvider = ConfigProvider.Replace(ConfigScopes.User, userTransact).Replace(ConfigScopes.Run, runTransact);
            UpdateValues();
            UpdateEnabled();
        }

        private void UpdateValues()
        {
            txtDefaultFilePath.Text = transactProvider.Get(c => c.PdfSettings.DefaultFileName);
            cbSkipSavePrompt.Checked = transactProvider.Get(c => c.PdfSettings.SkipSavePrompt);
            txtTitle.Text = transactProvider.Get(c => c.PdfSettings.Metadata.Title);
            txtAuthor.Text = transactProvider.Get(c => c.PdfSettings.Metadata.Author);
            txtSubject.Text = transactProvider.Get(c => c.PdfSettings.Metadata.Subject);
            txtKeywords.Text = transactProvider.Get(c => c.PdfSettings.Metadata.Keywords);
            cbEncryptPdf.Checked = transactProvider.Get(c => c.PdfSettings.Encryption.EncryptPdf);
            txtOwnerPassword.Text = transactProvider.Get(c => c.PdfSettings.Encryption.OwnerPassword);
            txtUserPassword.Text = transactProvider.Get(c => c.PdfSettings.Encryption.UserPassword);
            clbPerms.SetItemChecked(0, transactProvider.Get(c => c.PdfSettings.Encryption.AllowPrinting));
            clbPerms.SetItemChecked(1, transactProvider.Get(c => c.PdfSettings.Encryption.AllowFullQualityPrinting));
            clbPerms.SetItemChecked(2, transactProvider.Get(c => c.PdfSettings.Encryption.AllowDocumentModification));
            clbPerms.SetItemChecked(3, transactProvider.Get(c => c.PdfSettings.Encryption.AllowDocumentAssembly));
            clbPerms.SetItemChecked(4, transactProvider.Get(c => c.PdfSettings.Encryption.AllowContentCopying));
            clbPerms.SetItemChecked(5, transactProvider.Get(c => c.PdfSettings.Encryption.AllowContentCopyingForAccessibility));
            clbPerms.SetItemChecked(6, transactProvider.Get(c => c.PdfSettings.Encryption.AllowAnnotations));
            clbPerms.SetItemChecked(7, transactProvider.Get(c => c.PdfSettings.Encryption.AllowFormFilling));
            cmbCompat.SelectedIndex = (int)transactProvider.Get(c => c.PdfSettings.Compat);
            cbRememberSettings.Checked = transactProvider.Get(c => c.RememberPdfSettings);
        }

        private void UpdateEnabled()
        {
            cbSkipSavePrompt.Enabled = Path.IsPathRooted(txtDefaultFilePath.Text);

            bool encrypt = cbEncryptPdf.Checked;
            txtUserPassword.Enabled = txtOwnerPassword.Enabled = cbShowOwnerPassword.Enabled = cbShowUserPassword.Enabled =
                lblUserPassword.Enabled = lblOwnerPassword.Enabled = encrypt;
            clbPerms.Enabled = encrypt;

            cmbCompat.Enabled = ConfigScopes.AppLocked.Get(c => c.PdfSettings.Compat) == null;
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

            // Clear old run scope
            runTransact.Set(c => c.PdfSettings = new PdfSettings());

            var scope = cbRememberSettings.Checked ? userTransact : runTransact;
            scope.SetAll(new CommonConfig
            {
                PdfSettings = pdfSettings
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
            runTransact.Set(c => c.PdfSettings = new PdfSettings());
            userTransact.Set(c => c.PdfSettings = new PdfSettings());
            userTransact.Set(c => c.RememberPdfSettings = false);
            UpdateValues();
            UpdateEnabled();
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
