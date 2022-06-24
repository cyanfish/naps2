using System.Globalization;
using System.Windows.Forms;
using NAPS2.Config.Model;
using NAPS2.ImportExport.Images;

namespace NAPS2.WinForms;

public partial class FImageSettings : FormBase
{
    private readonly DialogHelper _dialogHelper;
    private TransactionConfigScope<CommonConfig> _userTransact;
    private TransactionConfigScope<CommonConfig> _runTransact;
    private ScopedConfig _transactionConfig;

    public FImageSettings(DialogHelper dialogHelper)
    {
        _dialogHelper = dialogHelper;
        InitializeComponent();
        AddEnumItems<TiffCompression>(cmbTiffCompr);
    }

    protected override void OnLoad(object sender, EventArgs e)
    {
        new LayoutManager(this)
            .Bind(btnRestoreDefaults, btnOK, btnCancel)
            .BottomToForm()
            .Bind(txtJpegQuality, btnOK, btnCancel, btnChooseFolder)
            .RightToForm()
            .Bind(txtDefaultFilePath, tbJpegQuality, lblWarning, groupTiff, groupJpeg)
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
        txtDefaultFilePath.Text = _transactionConfig.Get(c => c.ImageSettings.DefaultFileName);
        cbSkipSavePrompt.Checked = _transactionConfig.Get(c => c.ImageSettings.SkipSavePrompt);
        txtJpegQuality.Text = _transactionConfig.Get(c => c.ImageSettings.JpegQuality).ToString(CultureInfo.InvariantCulture);
        cmbTiffCompr.SelectedIndex = (int)_transactionConfig.Get(c => c.ImageSettings.TiffCompression);
        cbSinglePageTiff.Checked = _transactionConfig.Get(c => c.ImageSettings.SinglePageTiff);
        cbRememberSettings.Checked = _transactionConfig.Get(c => c.RememberImageSettings);
    }

    private void UpdateEnabled()
    {
        cbSkipSavePrompt.Enabled = Path.IsPathRooted(txtDefaultFilePath.Text);
    }

    private void txtDefaultFilePath_TextChanged(object sender, EventArgs e)
    {
        UpdateEnabled();
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        var imageSettings = new ImageSettings
        {
            DefaultFileName = txtDefaultFilePath.Text,
            SkipSavePrompt = cbSkipSavePrompt.Checked,
            JpegQuality = tbJpegQuality.Value,
            TiffCompression = (TiffCompression)cmbTiffCompr.SelectedIndex,
            SinglePageTiff = cbSinglePageTiff.Checked
        };

        _runTransact.Remove(c => c.ImageSettings);

        var scope = cbRememberSettings.Checked ? _userTransact : _runTransact;
        scope.Set(c => c.ImageSettings, imageSettings);

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
        _runTransact.Remove(c => c.ImageSettings);
        _userTransact.Remove(c => c.ImageSettings);
        _userTransact.Set(c => c.RememberImageSettings, false);
        UpdateValues();
        UpdateEnabled();
    }

    private void tbJpegQuality_Scroll(object sender, EventArgs e)
    {
        txtJpegQuality.Text = tbJpegQuality.Value.ToString("G");
    }

    private void txtJpegQuality_TextChanged(object sender, EventArgs e)
    {
        if (int.TryParse(txtJpegQuality.Text, out int value))
        {
            if (value >= tbJpegQuality.Minimum && value <= tbJpegQuality.Maximum)
            {
                tbJpegQuality.Value = value;
            }
        }
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
        if (_dialogHelper.PromptToSaveImage(txtDefaultFilePath.Text, out string savePath))
        {
            txtDefaultFilePath.Text = savePath;
        }
    }
}