using System;
using System.IO;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.ImportExport.Pdf;

namespace NAPS2.WinForms
{
    public partial class FPdfSettings : FormBase
    {
        private readonly DialogHelper _dialogHelper;
        private TransactionConfigScope<CommonConfig> _userTransact;
        private TransactionConfigScope<CommonConfig> _runTransact;
        private ConfigProvider<CommonConfig> _transactProvider;

        public FPdfSettings(DialogHelper dialogHelper)
        {
            _dialogHelper = dialogHelper;
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

            _userTransact = ConfigScopes.User.BeginTransaction();
            _runTransact = ConfigScopes.Run.BeginTransaction();
            _transactProvider = ConfigProvider.Replace(ConfigScopes.User, _userTransact).Replace(ConfigScopes.Run, _runTransact);
            UpdateValues();
            UpdateEnabled();
        }

        private void UpdateValues()
        {
            txtDefaultFilePath.Text = _transactProvider.Get(c => c.PdfSettings.DefaultFileName);
            cbSkipSavePrompt.Checked = _transactProvider.Get(c => c.PdfSettings.SkipSavePrompt);
            txtTitle.Text = _transactProvider.Get(c => c.PdfSettings.Metadata.Title);
            txtAuthor.Text = _transactProvider.Get(c => c.PdfSettings.Metadata.Author);
            txtSubject.Text = _transactProvider.Get(c => c.PdfSettings.Metadata.Subject);
            txtKeywords.Text = _transactProvider.Get(c => c.PdfSettings.Metadata.Keywords);
            cbEncryptPdf.Checked = _transactProvider.Get(c => c.PdfSettings.Encryption.EncryptPdf);
            txtOwnerPassword.Text = _transactProvider.Get(c => c.PdfSettings.Encryption.OwnerPassword);
            txtUserPassword.Text = _transactProvider.Get(c => c.PdfSettings.Encryption.UserPassword);
            clbPerms.SetItemChecked(0, _transactProvider.Get(c => c.PdfSettings.Encryption.AllowPrinting));
            clbPerms.SetItemChecked(1, _transactProvider.Get(c => c.PdfSettings.Encryption.AllowFullQualityPrinting));
            clbPerms.SetItemChecked(2, _transactProvider.Get(c => c.PdfSettings.Encryption.AllowDocumentModification));
            clbPerms.SetItemChecked(3, _transactProvider.Get(c => c.PdfSettings.Encryption.AllowDocumentAssembly));
            clbPerms.SetItemChecked(4, _transactProvider.Get(c => c.PdfSettings.Encryption.AllowContentCopying));
            clbPerms.SetItemChecked(5, _transactProvider.Get(c => c.PdfSettings.Encryption.AllowContentCopyingForAccessibility));
            clbPerms.SetItemChecked(6, _transactProvider.Get(c => c.PdfSettings.Encryption.AllowAnnotations));
            clbPerms.SetItemChecked(7, _transactProvider.Get(c => c.PdfSettings.Encryption.AllowFormFilling));
            cmbCompat.SelectedIndex = (int)_transactProvider.Get(c => c.PdfSettings.Compat);
            cbRememberSettings.Checked = _transactProvider.Get(c => c.RememberPdfSettings);
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
            _runTransact.Set(c => c.PdfSettings = new PdfSettings());

            var scope = cbRememberSettings.Checked ? _userTransact : _runTransact;
            scope.SetAll(new CommonConfig
            {
                PdfSettings = pdfSettings
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
            _runTransact.Set(c => c.PdfSettings = new PdfSettings());
            _userTransact.Set(c => c.PdfSettings = new PdfSettings());
            _userTransact.Set(c => c.RememberPdfSettings = false);
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
            if (_dialogHelper.PromptToSavePdf(txtDefaultFilePath.Text, out string savePath))
            {
                txtDefaultFilePath.Text = savePath;
            }
        }
    }
}
