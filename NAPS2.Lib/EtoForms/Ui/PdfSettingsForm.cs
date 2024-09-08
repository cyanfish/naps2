using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.Pdf;

namespace NAPS2.EtoForms.Ui;

public class PdfSettingsForm : EtoDialogBase
{
    private readonly FilePathWithPlaceholders _defaultFilePath;
    private readonly CheckBox _skipSavePrompt = new() { Text = UiStrings.SkipSavePrompt };
    private readonly CheckBox _singlePagePdfs = new() { Text = UiStrings.SinglePageFiles };
    private readonly TextBox _title = new();
    private readonly TextBox _author = new();
    private readonly TextBox _subject = new();
    private readonly TextBox _keywords = new();
    private readonly CheckBox _encryptPdf = new() { Text = UiStrings.EncryptPdf };
    private readonly PasswordBoxWithToggle _ownerPassword = new() { Title = UiStrings.OwnerPasswordLabel };
    private readonly PasswordBoxWithToggle _userPassword = new() { Title = UiStrings.UserPasswordLabel };
    private readonly CheckBox _rememberSettings = new() { Text = UiStrings.RememberTheseSettings };
    private readonly Button _restoreDefaults = new() { Text = UiStrings.RestoreDefaults };
    private readonly LayoutVisibility _encryptVis = new(false);

    private readonly List<CheckBox> _permissions =
    [
        new CheckBox { Text = UiStrings.AllowPrinting },
        new CheckBox { Text = UiStrings.AllowFullQualityPrinting },
        new CheckBox { Text = UiStrings.AllowDocumentModification },
        new CheckBox { Text = UiStrings.AllowDocumentAssembly },
        new CheckBox { Text = UiStrings.AllowContentCopying },
        new CheckBox { Text = UiStrings.AllowContentCopyingForAccessibility },
        new CheckBox { Text = UiStrings.AllowAnnotations },
        new CheckBox { Text = UiStrings.AllowFormFilling }
    ];

    private readonly EnumDropDownWidget<PdfCompat> _compat = new();

    public PdfSettingsForm(Naps2Config config, DialogHelper dialogHelper, IIconProvider iconProvider) : base(config)
    {
        Title = UiStrings.PdfSettingsFormTitle;
        IconName = "file_extension_pdf_small";

        _defaultFilePath = new(this, dialogHelper) { PdfOnly = true };
        _compat.Format = compat => compat switch
        {
            PdfCompat.Default => UiStrings.Default,
            PdfCompat.PdfA1B => "PDF/A-1b",
            PdfCompat.PdfA2B => "PDF/A-2b",
            PdfCompat.PdfA3B => "PDF/A-3b",
            PdfCompat.PdfA3U => "PDF/A-3u",
            _ => throw new ArgumentException()
        };

        UpdateValues(Config);
        UpdateEnabled();

        _restoreDefaults.Click += RestoreDefaults_Click;
        _defaultFilePath.TextChanged += DefaultFilePath_TextChanged;
        _encryptPdf.CheckedChanged += EncryptPdf_CheckedChanged;
    }

    protected override void BuildLayout()
    {
        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);
        FormStateController.FixedHeightLayout = true;
        base.BuildLayout();

        LayoutController.Content = L.Column(
            C.Label(UiStrings.DefaultFilePathLabel),
            _defaultFilePath,
            _skipSavePrompt,
            _singlePagePdfs,
            L.GroupBox(
                UiStrings.Metadata,
                L.Column(
                    C.Label(UiStrings.TitleLabel),
                    _title,
                    C.Label(UiStrings.AuthorLabel),
                    _author,
                    C.Label(UiStrings.SubjectLabel),
                    _subject,
                    C.Label(UiStrings.KeywordsLabel),
                    _keywords
                )
            ),
            L.GroupBox(
                UiStrings.Encryption,
                L.Column(
                    _encryptPdf,
                    L.Column(
                        _ownerPassword,
                        _userPassword,
                        L.Column(_permissions.Expand()).Spacing(0)
                    ).Visible(_encryptVis)
                )
            ),
            L.GroupBox(
                UiStrings.Compatibility,
                _compat
            ),
            C.Filler(),
            _rememberSettings,
            L.Row(
                _restoreDefaults.MinWidth(140),
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, Save),
                    C.CancelButton(this))
            )
        );
    }

    private void UpdateValues(Naps2Config config)
    {
        _defaultFilePath.Text = config.Get(c => c.PdfSettings.DefaultFileName);
        _skipSavePrompt.Checked = config.Get(c => c.PdfSettings.SkipSavePrompt);
        _singlePagePdfs.Checked = config.Get(c => c.PdfSettings.SinglePagePdfs);
        _title.Text = config.Get(c => c.PdfSettings.Metadata.Title);
        _author.Text = config.Get(c => c.PdfSettings.Metadata.Author);
        _subject.Text = config.Get(c => c.PdfSettings.Metadata.Subject);
        _keywords.Text = config.Get(c => c.PdfSettings.Metadata.Keywords);
        _encryptPdf.Checked = config.Get(c => c.PdfSettings.Encryption.EncryptPdf);
        _ownerPassword.Text = config.Get(c => c.PdfSettings.Encryption.OwnerPassword);
        _userPassword.Text = config.Get(c => c.PdfSettings.Encryption.UserPassword);
        _permissions[0].Checked = config.Get(c => c.PdfSettings.Encryption.AllowPrinting);
        _permissions[1].Checked = config.Get(c => c.PdfSettings.Encryption.AllowFullQualityPrinting);
        _permissions[2].Checked = config.Get(c => c.PdfSettings.Encryption.AllowDocumentModification);
        _permissions[3].Checked = config.Get(c => c.PdfSettings.Encryption.AllowDocumentAssembly);
        _permissions[4].Checked = config.Get(c => c.PdfSettings.Encryption.AllowContentCopying);
        _permissions[5].Checked =
            config.Get(c => c.PdfSettings.Encryption.AllowContentCopyingForAccessibility);
        _permissions[6].Checked = config.Get(c => c.PdfSettings.Encryption.AllowAnnotations);
        _permissions[7].Checked = config.Get(c => c.PdfSettings.Encryption.AllowFormFilling);
        _compat.SelectedItem = config.Get(c => c.PdfSettings.Compat);
        _rememberSettings.Checked = config.Get(c => c.RememberPdfSettings);
    }

    private void UpdateEnabled()
    {
        _skipSavePrompt.Enabled = Path.IsPathRooted(_defaultFilePath.Text);
        _encryptVis.IsVisible = _encryptPdf.IsChecked();
        _compat.Enabled = !Config.AppLocked.Has(c => c.PdfSettings.Compat);
    }

    private void Save()
    {
        var pdfSettings = new PdfSettings
        {
            DefaultFileName = _defaultFilePath.Text,
            SkipSavePrompt = _skipSavePrompt.IsChecked(),
            SinglePagePdfs = _singlePagePdfs.IsChecked(),
            Metadata = new PdfMetadata
            {
                Title = _title.Text,
                Author = _author.Text,
                Subject = _subject.Text,
                Keywords = _keywords.Text
            },
            Encryption = new PdfEncryption
            {
                EncryptPdf = _encryptPdf.IsChecked(),
                OwnerPassword = _ownerPassword.Text,
                UserPassword = _userPassword.Text,
                AllowPrinting = _permissions[0].IsChecked(),
                AllowFullQualityPrinting = _permissions[1].IsChecked(),
                AllowDocumentModification = _permissions[2].IsChecked(),
                AllowDocumentAssembly = _permissions[3].IsChecked(),
                AllowContentCopying = _permissions[4].IsChecked(),
                AllowContentCopyingForAccessibility = _permissions[5].IsChecked(),
                AllowAnnotations = _permissions[6].IsChecked(),
                AllowFormFilling = _permissions[7].IsChecked()
            },
            Compat = _compat.SelectedItem
        };

        var runTransact = Config.Run.BeginTransaction();
        var userTransact = Config.User.BeginTransaction();
        bool remember = _rememberSettings.IsChecked();
        var transactToWrite = remember ? userTransact : runTransact;

        runTransact.Remove(c => c.PdfSettings);
        userTransact.Remove(c => c.PdfSettings);
        transactToWrite.Set(c => c.PdfSettings, pdfSettings);
        userTransact.Set(c => c.RememberPdfSettings, remember);

        runTransact.Commit();
        userTransact.Commit();
    }

    private void RestoreDefaults_Click(object? sender, EventArgs e)
    {
        UpdateValues(Config.DefaultsOnly);
        UpdateEnabled();
    }

    private void DefaultFilePath_TextChanged(object? sender, EventArgs e)
    {
        UpdateEnabled();
    }

    private void EncryptPdf_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateEnabled();
    }
}