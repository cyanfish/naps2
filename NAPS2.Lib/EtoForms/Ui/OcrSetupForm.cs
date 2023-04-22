using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.Ocr;

namespace NAPS2.EtoForms.Ui;

public class OcrSetupForm : EtoDialogBase
{
    private readonly TesseractLanguageManager _tesseractLanguageManager;

    private readonly CheckBox _enableOcr = C.CheckBox(UiStrings.MakePdfsSearchable);
    private readonly DropDown _ocrLang = C.DropDown();
    private readonly DropDown _ocrMode = C.EnumDropDown(LocalizedOcrMode.Fast, LocalizedOcrMode.Best);
    private readonly CheckBox _ocrAfterScanning = C.CheckBox(UiStrings.RunOcrAfterScanning);
    private readonly LinkButton _moreLanguages = C.Link(UiStrings.GetMoreLanguages);

    public OcrSetupForm(Naps2Config config, TesseractLanguageManager tesseractLanguageManager) : base(config)
    {
        _tesseractLanguageManager = tesseractLanguageManager;

        _enableOcr.CheckedChanged += EnableOcr_CheckedChanged;
        _moreLanguages.Click += MoreLanguages_Click;

        LoadLanguages();

        _enableOcr.Checked = Config.Get(c => c.EnableOcr);
        _ocrLang.SelectedKey = Config.Get(c => c.OcrLanguageCode) ?? "";
        if (_ocrLang.SelectedIndex == -1) _ocrLang.SelectedIndex = 0;
        _ocrMode.SelectedIndex = (int) Config.Get(c => c.OcrMode);
        if (_ocrMode.SelectedIndex == -1) _ocrMode.SelectedIndex = 0;
        _ocrAfterScanning.Checked = Config.Get(c => c.OcrAfterScanning);

        UpdateView();
    }

    protected override void BuildLayout()
    {
        Title = UiStrings.OcrSetupFormTitle;
        Icon = new Icon(1f, Icons.text_small.ToEtoImage());

        FormStateController.Resizable = false;

        LayoutController.Content = L.Column(
            _enableOcr,
            L.Row(
                C.Label(UiStrings.OcrLanguageLabel).AlignCenter().Padding(right: 40),
                _ocrLang.Scale()
            ).Aligned(),
            L.Row(
                C.Label(UiStrings.OcrModeLabel).AlignCenter().Padding(right: 40),
                _ocrMode.Scale()
            ).Aligned(),
            _ocrAfterScanning,
            C.Filler(),
            L.Row(
                _moreLanguages.AlignCenter().Padding(right: 30),
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, Save),
                    // TODO: Should we allow Esc to close the window if there are unsaved changes?
                    C.CancelButton(this))
            )
        );
    }

    private void LoadLanguages()
    {
        var languages = _tesseractLanguageManager.InstalledLanguages
            .OrderBy(x => x.Name)
            .ToList();
        var selectedKey = _ocrLang.SelectedKey;
        _ocrLang.Items.Clear();
        _ocrLang.Items.AddRange(languages.Select(lang => new ListItem
        {
            Key = lang.Code,
            Text = lang.Name
        }));
        _ocrLang.SelectedKey = selectedKey;
    }

    private void UpdateView()
    {
        bool isEnabled = _enableOcr.IsChecked();
        _enableOcr.Enabled = !Config.AppLocked.Has(c => c.EnableOcr);
        _ocrLang.Enabled = isEnabled && !Config.AppLocked.Has(c => c.OcrLanguageCode);
        _ocrMode.Enabled = isEnabled && !Config.AppLocked.Has(c => c.OcrMode);
        _ocrAfterScanning.Enabled = isEnabled && !Config.AppLocked.Has(c => c.OcrAfterScanning);
        _moreLanguages.Enabled = !Config.AppLocked.Has(c => c.OcrLanguageCode);
    }

    private void EnableOcr_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateView();
    }

    private void MoreLanguages_Click(object? sender, EventArgs e)
    {
        FormFactory.Create<OcrDownloadForm>().ShowModal();
        LoadLanguages();
    }

    private void Save()
    {
        if (!Config.AppLocked.Has(c => c.EnableOcr))
        {
            var transact = Config.User.BeginTransaction();
            transact.Set(c => c.EnableOcr, _enableOcr.IsChecked());
            transact.Set(c => c.OcrLanguageCode, _ocrLang.SelectedKey);
            transact.Set(c => c.OcrMode, (LocalizedOcrMode) _ocrMode.SelectedIndex);
            transact.Set(c => c.OcrAfterScanning, _ocrAfterScanning.IsChecked());
            transact.Commit();
        }
    }
}