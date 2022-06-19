using System.Windows.Forms;
using NAPS2.ImportExport;
using NAPS2.Scan;

namespace NAPS2.WinForms;

public partial class FAutoSaveSettings : FormBase
{
    private readonly DialogHelper _dialogHelper;

    private bool _result;

    public FAutoSaveSettings(DialogHelper dialogHelper)
    {
        _dialogHelper = dialogHelper;
        InitializeComponent();
    }

    protected override void OnLoad(object sender, EventArgs e)
    {
        if (ScanProfile.AutoSaveSettings != null)
        {
            txtFilePath.Text = ScanProfile.AutoSaveSettings.FilePath;
            cbPromptForFilePath.Checked = ScanProfile.AutoSaveSettings.PromptForFilePath;
            cbClearAfterSave.Checked = ScanProfile.AutoSaveSettings.ClearImagesAfterSaving;
            if (ScanProfile.AutoSaveSettings.Separator == SaveSeparator.FilePerScan)
            {
                rdFilePerScan.Checked = true;
            }
            else if (ScanProfile.AutoSaveSettings.Separator == SaveSeparator.PatchT)
            {
                rdSeparateByPatchT.Checked = true;
            }
            else
            {
                rdFilePerPage.Checked = true;
            }
        }

        new LayoutManager(this)
            .Bind(txtFilePath)
            .WidthToForm()
            .Bind(btnChooseFolder, btnOK, btnCancel)
            .RightToForm()
            .Activate();
    }

    public bool Result => _result;

    public ScanProfile ScanProfile { get; set; }

    private void SaveSettings()
    {
        ScanProfile.AutoSaveSettings = new AutoSaveSettings
        {
            FilePath = txtFilePath.Text,
            PromptForFilePath = cbPromptForFilePath.Checked,
            ClearImagesAfterSaving = cbClearAfterSave.Checked,
            Separator = rdFilePerScan.Checked ? SaveSeparator.FilePerScan
                : rdSeparateByPatchT.Checked ? SaveSeparator.PatchT
                : SaveSeparator.FilePerPage
        };
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtFilePath.Text) && !cbPromptForFilePath.Checked)
        {
            txtFilePath.Focus();
            return;
        }
        _result = true;
        SaveSettings();
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void btnChooseFolder_Click(object sender, EventArgs e)
    {
        if (_dialogHelper.PromptToSavePdfOrImage(null, out string savePath))
        {
            txtFilePath.Text = savePath;
        }
    }

    private void linkPlaceholders_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        var form = FormFactory.Create<FPlaceholders>();
        form.FileName = txtFilePath.Text;
        if (form.ShowDialog() == DialogResult.OK)
        {
            txtFilePath.Text = form.FileName;
        }
    }

    private void linkPatchCodeInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        Process.Start(FBatchScan.PATCH_CODE_INFO_URL);
    }
}