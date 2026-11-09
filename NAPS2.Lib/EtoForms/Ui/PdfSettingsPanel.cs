using System.Linq.Expressions;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.Pdf;

namespace NAPS2.EtoForms.Ui;

internal class PdfSettingsPanel : SettingsPanelWithRememberCheckbox<PdfSettings>
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

    public PdfSettingsPanel(IFormBase form, DialogHelper dialogHelper) : base(form)
    {
        _defaultFilePath = new(form, dialogHelper) { PdfOnly = true };
        _compat.Format = compat => compat switch
        {
            PdfCompat.Default => UiStrings.Default,
            PdfCompat.PdfA1B => "PDF/A-1b",
            PdfCompat.PdfA2B => "PDF/A-2b",
            PdfCompat.PdfA3B => "PDF/A-3b",
            PdfCompat.PdfA3U => "PDF/A-3u",
            _ => throw new ArgumentException()
        };

        LoadValues(Config);

        _defaultFilePath.TextChanged += DefaultFilePath_TextChanged;
        _encryptPdf.CheckedChanged += EncryptPdf_CheckedChanged;
    }

    public override string Title => UiStrings.PdfSettings;

    public override string IconName => "file_extension_pdf";

    protected override Expression<Func<CommonConfig, PdfSettings>> SettingsAccessor => c => c.PdfSettings;

    protected override Expression<Func<CommonConfig, bool>> RememberAccessor => c => c.RememberPdfSettings;

    public override LayoutElement BuildScrollableLayout()
    {
        return L.Column(
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
            )
        );
    }

    protected override PdfSettings SettingsValues
    {
        get =>
            new()
            {
                DefaultFileName = _defaultFilePath.Text,
                SkipSavePrompt = _skipSavePrompt.IsChecked(),
                SinglePagePdfs = _singlePagePdfs.IsChecked(),
                Metadata = new PdfMetadata
                {
                    Title = _title.Text,
                    Author = _author.Text,
                    Subject = _subject.Text,
                    Keywords = _keywords.Text,
                    // Not editable in the UI, but it can still be configured
                    Creator = Config.Get(c => c.PdfSettings.Metadata.Creator)
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
        set
        {
            _defaultFilePath.Text = value.DefaultFileName;
            _skipSavePrompt.Checked = value.SkipSavePrompt;
            _singlePagePdfs.Checked = value.SinglePagePdfs;
            _title.Text = value.Metadata.Title;
            _author.Text = value.Metadata.Author;
            _subject.Text = value.Metadata.Subject;
            _keywords.Text = value.Metadata.Keywords;
            _encryptPdf.Checked = value.Encryption.EncryptPdf;
            _ownerPassword.Text = value.Encryption.OwnerPassword;
            _userPassword.Text = value.Encryption.UserPassword;
            _permissions[0].Checked = value.Encryption.AllowPrinting;
            _permissions[1].Checked = value.Encryption.AllowFullQualityPrinting;
            _permissions[2].Checked = value.Encryption.AllowDocumentModification;
            _permissions[3].Checked = value.Encryption.AllowDocumentAssembly;
            _permissions[4].Checked = value.Encryption.AllowContentCopying;
            _permissions[5].Checked = value.Encryption.AllowContentCopyingForAccessibility;
            _permissions[6].Checked = value.Encryption.AllowAnnotations;
            _permissions[7].Checked = value.Encryption.AllowFormFilling;
            _compat.SelectedItem = value.Compat;
            UpdateEnabled();
        }
    }

    private void UpdateEnabled()
    {
        _skipSavePrompt.Enabled = Path.IsPathRooted(_defaultFilePath.Text);
        _encryptVis.IsVisible = _encryptPdf.IsChecked();
        _compat.Enabled = !Config.AppLocked.Has(c => c.PdfSettings.Compat);
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