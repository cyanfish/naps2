using System.Globalization;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.Lang;
using NAPS2.Ocr;

namespace NAPS2.EtoForms.Ui;

public class OcrSetupForm : EtoDialogBase
{
    private readonly TesseractLanguageManager _tesseractLanguageManager;

    private readonly CheckBox _enableOcr = C.CheckBox(UiStrings.MakePdfsSearchable);
    private readonly DropDown _ocrLang = C.DropDown();
    private readonly EnumDropDownWidget<LocalizedOcrMode> _ocrMode = new()
        { Items = [LocalizedOcrMode.Fast, LocalizedOcrMode.Best] };
    private readonly CheckBox _ocrPreProcessing = C.CheckBox(UiStrings.OcrPreProcessing);
    private readonly CheckBox _ocrAfterScanning = C.CheckBox(
        TranslationMigrator.PickTranslated(
            UiStrings.ResourceManager,
            "RunOcrAfterScanning",
            "PreemptivelyOcrAfterScanning"));
    private readonly LinkButton _moreLanguages = C.Link(UiStrings.GetMoreLanguages);

    private string? _code;
    private string? _multiLangCode;
    private bool _suppressLangChangeEvent;

    public OcrSetupForm(Naps2Config config, TesseractLanguageManager tesseractLanguageManager,
        IIconProvider iconProvider) : base(config)
    {
        Title = UiStrings.OcrSetupFormTitle;
        Icon = iconProvider.GetFormIcon("text_small");

        _tesseractLanguageManager = tesseractLanguageManager;

        _enableOcr.CheckedChanged += EnableOcr_CheckedChanged;
        _moreLanguages.Click += MoreLanguages_Click;

        var configOcrMode = Config.Get(c => c.OcrMode);
        if (configOcrMode == LocalizedOcrMode.Legacy)
        {
            // Legacy is no longer supported
            configOcrMode = LocalizedOcrMode.Fast;
        }

        _code = Config.Get(c => c.OcrLanguageCode);
        _multiLangCode = Config.Get(c => c.LastOcrMultiLangCode);

        _enableOcr.Checked = Config.Get(c => c.EnableOcr);
        _ocrLang.SelectedIndexChanged += OcrLang_SelectedIndexChanged;
        _ocrMode.SelectedItem = configOcrMode;
        _ocrPreProcessing.Checked = Config.Get(c => c.OcrPreProcessing);
        _ocrAfterScanning.Checked = Config.Get(c => c.OcrAfterScanning);

        LoadLanguages();
        UpdateView();
    }

    protected override void BuildLayout()
    {
        FormStateController.Resizable = false;

        LayoutController.Content = L.Column(
            _enableOcr,
            L.Row(
                C.Label(UiStrings.OcrLanguageLabel).AlignCenter().Padding(right: 40),
                _ocrLang.Scale()
            ).Aligned(),
            L.Row(
                C.Label(UiStrings.OcrModeLabel).AlignCenter().Padding(right: 40),
                _ocrMode.AsControl().Scale()
            ).Aligned(),
            _ocrPreProcessing,
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
        _suppressLangChangeEvent = true;
        var languages = _tesseractLanguageManager.InstalledLanguages
            .OrderBy(x => x.Name)
            .ToList();
        _ocrLang.Items.Clear();
        _ocrLang.Items.AddRange(languages.Select(lang => new ListItem
        {
            Key = lang.Code,
            Text = lang.Name
        }));
        if (languages.Count > 1)
        {
            if (_multiLangCode?.Contains("+") == true)
            {
                _ocrLang.Items.Add(new ListItem
                {
                    Key = _multiLangCode,
                    Text = string.Join($"{CultureInfo.CurrentCulture.TextInfo.ListSeparator} ",
                        _multiLangCode.Split('+')
                            .Select(code => languages.SingleOrDefault(lang => lang.Code == code)?.Name))
                });
            }
            _ocrLang.Items.Add(new ListItem
            {
                Key = "",
                Text = UiStrings.MultipleLanguages
            });
        }
        if (!string.IsNullOrEmpty(_code))
        {
            _ocrLang.SelectedKey = _code;
        }
        if (_ocrLang.SelectedIndex == -1)
        {
            _ocrLang.SelectedIndex = 0;
        }
        _suppressLangChangeEvent = false;
    }

    private void UpdateView()
    {
        bool isEnabled = _enableOcr.IsChecked();
        _enableOcr.Enabled = !Config.AppLocked.Has(c => c.EnableOcr);
        _ocrLang.Enabled = isEnabled && !Config.AppLocked.Has(c => c.OcrLanguageCode);
        _ocrMode.Enabled = isEnabled && !Config.AppLocked.Has(c => c.OcrMode);
        _ocrPreProcessing.Enabled = isEnabled && !Config.AppLocked.Has(c => c.OcrPreProcessing);
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

    private void OcrLang_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_suppressLangChangeEvent) return;
        if (_ocrLang.SelectedIndex == _ocrLang.Items.Count - 1)
        {
            var multiLangForm = FormFactory.Create<OcrMultiLangForm>();
            multiLangForm.ShowModal();
            if (multiLangForm.Code != null)
            {
                _code = multiLangForm.Code;
            }
            if (multiLangForm.Code?.Contains("+") == true)
            {
                _multiLangCode = multiLangForm.Code;
            }
            LoadLanguages();
        }
        _code = _ocrLang.SelectedKey;
    }

    private void Save()
    {
        if (!Config.AppLocked.Has(c => c.EnableOcr))
        {
            var transact = Config.User.BeginTransaction();
            transact.Set(c => c.EnableOcr, _enableOcr.IsChecked());
            transact.Set(c => c.OcrLanguageCode, _ocrLang.SelectedKey);
            if (_multiLangCode?.Contains("+") == true)
            {
                Config.User.Set(c => c.LastOcrMultiLangCode, _multiLangCode);
            }
            transact.Set(c => c.OcrMode, _ocrMode.SelectedItem);
            transact.Set(c => c.OcrPreProcessing, _ocrPreProcessing.IsChecked());
            transact.Set(c => c.OcrAfterScanning, _ocrAfterScanning.IsChecked());
            transact.Commit();
        }
    }
}