using System.Windows.Forms;
using NAPS2.Ocr;
using NAPS2.Scan;

namespace NAPS2.WinForms;

public partial class FOcrSetup : FormBase
{
    private readonly TesseractLanguageManager _tesseractLanguageManager;
    private readonly List<OcrMode> _availableModes = new() { OcrMode.Fast, OcrMode.Best };

    public FOcrSetup(TesseractLanguageManager tesseractLanguageManager)
    {
        _tesseractLanguageManager = tesseractLanguageManager;
        InitializeComponent();

        comboOcrMode.Format += (sender, e) => e.Value = ((Enum)e.ListItem).Description();
        foreach (var mode in _availableModes)
        {
            comboOcrMode.Items.Add(mode);
        }
    }

    protected override void OnLoad(object sender, EventArgs eventArgs)
    {
        new LayoutManager(this)
            .Bind(btnCancel, btnOK)
            .RightToForm()
            .Bind(comboLanguages, comboOcrMode)
            .WidthToForm()
            .Activate();

        LoadLanguages();
        comboLanguages.DisplayMember = "Name";
        comboLanguages.ValueMember = "Code";

        checkBoxEnableOcr.Checked = Config.Get(c => c.EnableOcr);
        SetSelectedValue(comboLanguages, Config.Get(c => c.OcrLanguageCode));
        SetSelectedItem(comboOcrMode, Config.Get(c => c.OcrMode));
        checkBoxRunInBG.Checked = Config.Get(c => c.OcrAfterScanning);

        UpdateView();
    }

    private void SetSelectedValue(ComboBox combo, object value)
    {
        combo.SelectedValue = value;
        // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
        if (combo.SelectedValue == null && combo.Items.Count > 0)
        {
            combo.SelectedIndex = 0;
        }
    }

    private void SetSelectedItem(ComboBox combo, object item)
    {
        combo.SelectedItem = item;
        // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
        if (combo.SelectedItem == null && combo.Items.Count > 0)
        {
            combo.SelectedIndex = 0;
        }
    }

    private void LoadLanguages()
    {
        var languages = _tesseractLanguageManager.InstalledLanguages
            .OrderBy(x => x.Name)
            .ToList();
        comboLanguages.DataSource = languages;
    }

    private void UpdateView()
    {
        bool canChangeEnabled = Config.AppLocked.Get(c => c.EnableOcr) == null;
        bool canChangeLanguage = canChangeEnabled
                                 || Config.Get(c => c.EnableOcr)
                                 && Config.AppLocked.Get(c => c.OcrLanguageCode) == null;
        checkBoxEnableOcr.Enabled = canChangeEnabled;
        comboLanguages.Enabled = checkBoxEnableOcr.Checked && canChangeLanguage;
        linkGetLanguages.Enabled = canChangeLanguage;
        label1.Enabled = canChangeLanguage;
        btnOK.Enabled = canChangeEnabled || canChangeLanguage;
        comboOcrMode.Enabled = checkBoxEnableOcr.Checked && canChangeEnabled;
        checkBoxRunInBG.Enabled = checkBoxEnableOcr.Checked && canChangeEnabled;
    }

    private void checkBoxEnableOcr_CheckedChanged(object sender, EventArgs e)
    {
        UpdateView();
    }

    private void linkGetLanguages_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        FormFactory.Create<FOcrLanguageDownload>().ShowDialog();
        var selectedLang = comboLanguages.SelectedItem;
        LoadLanguages();
        comboLanguages.SelectedItem = selectedLang;
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        if (Config.AppLocked.Get(c => c.EnableOcr) == null)
        {
            Config.User.SetAll(new CommonConfig
            {
                EnableOcr = checkBoxEnableOcr.Checked,
                OcrLanguageCode = (string)comboLanguages.SelectedValue,
                OcrMode = _availableModes != null ? (OcrMode)comboOcrMode.SelectedItem : OcrMode.Default,
                OcrAfterScanning = checkBoxRunInBG.Checked
            });
        }
        Close();
    }
}