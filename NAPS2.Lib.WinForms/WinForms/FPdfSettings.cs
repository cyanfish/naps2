using System.Windows.Forms;
using NAPS2.Config.Model;
using NAPS2.ImportExport.Pdf;

namespace NAPS2.WinForms;

public partial class FPdfSettings : FormBase
{
    private readonly DialogHelper _dialogHelper;
    private TransactionConfigScope<CommonConfig> _userTransact;
    private TransactionConfigScope<CommonConfig> _runTransact;
    private ScopedConfig _transactionConfig;

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

        _userTransact = Config.User.BeginTransaction();
        _runTransact = Config.Run.BeginTransaction();
        _transactionConfig = Config.WithTransaction(_userTransact, _runTransact);
        UpdateValues();
        UpdateEnabled();
    }

    private void UpdateValues()
    {
        txtDefaultFilePath.Text = _transactionConfig.Get(c => c.PdfSettings.DefaultFileName);
        cbSkipSavePrompt.Checked = _transactionConfig.Get(c => c.PdfSettings.SkipSavePrompt);
        txtTitle.Text = _transactionConfig.Get(c => c.PdfSettings.Metadata.Title);
        txtAuthor.Text = _transactionConfig.Get(c => c.PdfSettings.Metadata.Author);
        txtSubject.Text = _transactionConfig.Get(c => c.PdfSettings.Metadata.Subject);
        txtKeywords.Text = _transactionConfig.Get(c => c.PdfSettings.Metadata.Keywords);
        cbEncryptPdf.Checked = _transactionConfig.Get(c => c.PdfSettings.Encryption.EncryptPdf);
        txtOwnerPassword.Text = _transactionConfig.Get(c => c.PdfSettings.Encryption.OwnerPassword);
        txtUserPassword.Text = _transactionConfig.Get(c => c.PdfSettings.Encryption.UserPassword);
        clbPerms.SetItemChecked(0, _transactionConfig.Get(c => c.PdfSettings.Encryption.AllowPrinting));
        clbPerms.SetItemChecked(1, _transactionConfig.Get(c => c.PdfSettings.Encryption.AllowFullQualityPrinting));
        clbPerms.SetItemChecked(2, _transactionConfig.Get(c => c.PdfSettings.Encryption.AllowDocumentModification));
        clbPerms.SetItemChecked(3, _transactionConfig.Get(c => c.PdfSettings.Encryption.AllowDocumentAssembly));
        clbPerms.SetItemChecked(4, _transactionConfig.Get(c => c.PdfSettings.Encryption.AllowContentCopying));
        clbPerms.SetItemChecked(5, _transactionConfig.Get(c => c.PdfSettings.Encryption.AllowContentCopyingForAccessibility));
        clbPerms.SetItemChecked(6, _transactionConfig.Get(c => c.PdfSettings.Encryption.AllowAnnotations));
        clbPerms.SetItemChecked(7, _transactionConfig.Get(c => c.PdfSettings.Encryption.AllowFormFilling));
        cmbCompat.SelectedIndex = (int)_transactionConfig.Get(c => c.PdfSettings.Compat);
        cbRememberSettings.Checked = _transactionConfig.Get(c => c.RememberPdfSettings);
    }

    private void UpdateEnabled()
    {
        cbSkipSavePrompt.Enabled = Path.IsPathRooted(txtDefaultFilePath.Text);

        bool encrypt = cbEncryptPdf.Checked;
        txtUserPassword.Enabled = txtOwnerPassword.Enabled = cbShowOwnerPassword.Enabled = cbShowUserPassword.Enabled =
            lblUserPassword.Enabled = lblOwnerPassword.Enabled = encrypt;
        clbPerms.Enabled = encrypt;

        cmbCompat.Enabled = !Config.AppLocked.TryGet(c => c.PdfSettings.Compat, out _);
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        var pdfSettings = new PdfSettings
        {
            DefaultFileName = txtDefaultFilePath.Text,
            SkipSavePrompt = cbSkipSavePrompt.Checked,
            Metadata = new PdfMetadata
            {
                Title = txtTitle.Text,
                Author = txtAuthor.Text,
                Subject = txtSubject.Text,
                Keywords = txtKeywords.Text
            },
            Encryption = new PdfEncryption
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

        _runTransact.Remove(c => c.PdfSettings);
        var scope = cbRememberSettings.Checked ? _userTransact : _runTransact;
        scope.Set(c => c.PdfSettings, pdfSettings);

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
        _runTransact.Remove(c => c.PdfSettings);
        _userTransact.Remove(c => c.PdfSettings);
        _userTransact.Set(c => c.RememberPdfSettings, false);
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