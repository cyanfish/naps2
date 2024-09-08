using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.ImportExport;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Ui;

public class AutoSaveSettingsForm : EtoDialogBase
{
    private const string PATCH_CODE_INFO_URL = "https://www.naps2.com/doc/batch-scan#patch-t";

    private readonly FilePathWithPlaceholders _filePath;
    private readonly CheckBox _promptForFilePath = new() { Text = UiStrings.PromptForFilePath };
    private readonly RadioButton _filePerPage;
    private readonly RadioButton _filePerScan;
    private readonly RadioButton _separateByPatchT;
    private readonly CheckBox _clearAfterSaving = new() { Text = UiStrings.ClearAfterSaving };

    public AutoSaveSettingsForm(Naps2Config config, DialogHelper dialogHelper)
        : base(config)
    {
        _filePath = new(this, dialogHelper);
        _filePerPage = new() { Text = UiStrings.OneFilePerPage, Checked = true };
        _filePerScan = new(_filePerPage) { Text = UiStrings.OneFilePerScan };
        _separateByPatchT = new RadioButton(_filePerPage) { Text = UiStrings.SeparateByPatchT };
    }

    public ScanProfile? ScanProfile { get; set; }

    public bool Result { get; private set; }

    protected override void BuildLayout()
    {
        if (ScanProfile?.AutoSaveSettings != null)
        {
            _filePath.Text = ScanProfile.AutoSaveSettings.FilePath;
            _promptForFilePath.Checked = ScanProfile.AutoSaveSettings.PromptForFilePath;
            _clearAfterSaving.Checked = ScanProfile.AutoSaveSettings.ClearImagesAfterSaving;
            if (ScanProfile.AutoSaveSettings.Separator == SaveSeparator.FilePerScan)
            {
                _filePerScan.Checked = true;
            }
            else if (ScanProfile.AutoSaveSettings.Separator == SaveSeparator.PatchT)
            {
                _separateByPatchT.Checked = true;
            }
            else
            {
                _filePerPage.Checked = true;
            }
        }

        Title = UiStrings.AutoSaveSettingsFormTitle;

        FormStateController.FixedHeightLayout = true;
        base.BuildLayout();

        LayoutController.Content = L.Column(
            C.Label(UiStrings.FilePathLabel).NaturalWidth(300),
            _filePath,
            _promptForFilePath,
            C.Spacer(),
            C.Spacer(),
            _filePerPage,
            _filePerScan,
            _separateByPatchT,
            C.UrlLink(PATCH_CODE_INFO_URL, UiStrings.MoreInfo),
            C.Spacer(),
            C.Spacer(),
            _clearAfterSaving,
            C.Filler(),
            L.Row(
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, Save),
                    C.CancelButton(this))
            )
        );
    }

    private bool Save()
    {
        if (string.IsNullOrWhiteSpace(_filePath.Text) && !_promptForFilePath.IsChecked())
        {
            _filePath.Focus();
            return false;
        }
        var separator = _filePerScan.Checked ? SaveSeparator.FilePerScan
            : _separateByPatchT.Checked ? SaveSeparator.PatchT
            : SaveSeparator.FilePerPage;
        ScanProfile!.AutoSaveSettings = new AutoSaveSettings
        {
            FilePath = _filePath.Text!,
            PromptForFilePath = _promptForFilePath.IsChecked(),
            ClearImagesAfterSaving = _clearAfterSaving.IsChecked(),
            Separator = separator
        };
        Result = true;
        return true;
    }
}