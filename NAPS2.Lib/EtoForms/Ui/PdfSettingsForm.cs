using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.Pdf;

namespace NAPS2.EtoForms.Ui;

public class PdfSettingsForm : EtoDialogBase
{
    private readonly FilePathWithPlaceholders _defaultFilePath;
    private readonly CheckBox _ocr = new() { Text = UiStrings.Ocr };
    private readonly CheckBox _skipSavePrompt = new() { Text = UiStrings.SkipSavePrompt };
    private readonly CheckBox _singlePagePdfs = new() { Text = UiStrings.SinglePageFiles };
    private readonly SliderWithTextBox _jpegQuality = new(new SliderWithTextBox.IntConstraints(0, 100, 25));
    private readonly SliderWithTextBox _resolutionScale = new(new SliderWithTextBox.IntConstraints(10, 100, 10));
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

    private readonly UiImageList? _imageList;
    private readonly List<(int Width, int Height, int Dpi)> _imageDimensions = new();
    private readonly Label _resolutionLabel = new() { Font = Fonts.Sans(9.0f), TextColor = Colors.Gray };
    private readonly Label _sizeLabel = new() { Font = Fonts.Sans(9.0f), TextColor = Colors.Gray };

    public PdfSettingsForm(Naps2Config config, DialogHelper dialogHelper, IIconProvider iconProvider, UiImageList imageList) : base(config)
    {
        _imageList = imageList;
        Title = UiStrings.PdfSettingsFormTitle;
        IconName = "file_extension_pdf_small";

        _defaultFilePath = new(this, dialogHelper) { PdfOnly = true };
        _defaultFilePath.ShowPlaceholdersLink = false;
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

        _jpegQuality.ValueChanged += UpdateResolutionAndSizeEstimation;
        _resolutionScale.ValueChanged += UpdateResolutionAndSizeEstimation;
    }

    protected override void BuildLayout()
    {
        FormStateController.DefaultExtraLayoutSize = new Size(150, 0);
        FormStateController.FixedHeightLayout = true;

        LayoutController.Content = L.Column(
            C.Label(UiStrings.DefaultFilePathLabel),
            _defaultFilePath,
            L.Row(
                _defaultFilePath.Placeholders.AlignCenter(),
                _ocr.AlignCenter(),
                _skipSavePrompt.AlignCenter(),
                _singlePagePdfs.AlignCenter()
            ).Spacing(20),
            L.GroupBox(
                UiStrings.JpegQuality,
                L.Column(
                    _jpegQuality.AsControl().SpacingAfter(0),
                    C.Label(UiStrings.JpegQualityHelp).DynamicWrap(300).SpacingAfter(5),
                    _sizeLabel
                )
            ),
            L.GroupBox(
                UiStrings.ResolutionScale,
                L.Column(
                    _resolutionScale.AsControl().SpacingAfter(0),
                    C.Label(UiStrings.ResolutionScaleHelp).DynamicWrap(300).SpacingAfter(5),
                    _resolutionLabel
                )
            ),
            L.GroupBox(
                UiStrings.Metadata,
                L.Column(
                    L.Row(
                        C.Label(UiStrings.TitleLabel).AlignCenter().Padding(right: 5),
                        _title.Scale().Padding(right: 15),
                        C.Label(UiStrings.AuthorLabel).AlignCenter().Padding(right: 5),
                        _author.Scale()
                    ).Aligned(),
                    L.Row(
                        C.Label(UiStrings.SubjectLabel).AlignCenter().Padding(right: 5),
                        _subject.Scale().Padding(right: 15),
                        C.Label(UiStrings.KeywordsLabel).AlignCenter().Padding(right: 5),
                        _keywords.Scale()
                    ).Aligned()
                )
            ),
            L.Row(
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
                ).Scale(),
                L.GroupBox(
                    UiStrings.Compatibility,
                    _compat
                ).Scale()
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
        _imageDimensions.Clear();
        if (_imageList?.Images.Count > 0)
        {
            foreach (var img in _imageList.Images)
            {
                try
                {
                    using var rendered = img.GetClonedImage().Render();
                    _imageDimensions.Add((rendered.Width, rendered.Height, (int)Math.Round(rendered.HorizontalResolution)));
                }
                catch
                {
                    _imageDimensions.Add((2480, 3508, 300));
                }
            }
        }

        _defaultFilePath.Text = config.Get(c => c.PdfSettings.DefaultFileName);
        _ocr.Checked = config.Get(c => c.PdfSettings.Ocr) ?? config.Get(c => c.EnableOcr);
        _skipSavePrompt.Checked = config.Get(c => c.PdfSettings.SkipSavePrompt);
        _singlePagePdfs.Checked = config.Get(c => c.PdfSettings.SinglePagePdfs);
        _jpegQuality.IntValue = config.Get(c => c.PdfSettings.JpegQuality);
        _resolutionScale.IntValue = Math.Max(10, config.Get(c => c.PdfSettings.ResolutionScale));
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

        UpdateResolutionAndSizeEstimation();
    }

    private void UpdateResolutionAndSizeEstimation()
    {
        int scale = _resolutionScale.IntValue;
        int quality = _jpegQuality.IntValue;

        int baseDpi = 300;
        int baseWidth = 2480;
        int baseHeight = 3508;
        int imageCount = _imageDimensions.Count;

        if (imageCount > 0)
        {
            baseDpi = _imageDimensions[0].Dpi;
            baseWidth = _imageDimensions[0].Width;
            baseHeight = _imageDimensions[0].Height;
        }

        int newDpi = (int)Math.Round(baseDpi * scale / 100.0);
        int newWidth = (int)Math.Round(baseWidth * scale / 100.0);
        int newHeight = (int)Math.Round(baseHeight * scale / 100.0);

        _resolutionLabel.Text = $"Resolution: {newDpi} DPI (Original: {baseDpi} DPI)  |  Dimensions: {newWidth}x{newHeight} px";

        double mq;
        if (quality >= 75)
        {
            mq = 1.0 + 1.2 * (quality - 75) / 25.0;
        }
        else if (quality >= 50)
        {
            mq = 0.6 + 0.4 * (quality - 50) / 25.0;
        }
        else
        {
            mq = 0.15 + 0.45 * quality / 50.0;
        }

        double singlePageBytes = baseWidth * baseHeight * 0.07 * Math.Pow(scale / 100.0, 2) * mq;

        if (imageCount > 0)
        {
            double totalBytes = 0;
            foreach (var dim in _imageDimensions)
            {
                totalBytes += dim.Width * dim.Height * 0.07 * Math.Pow(scale / 100.0, 2) * mq;
            }
            _sizeLabel.Text = $"Estimated PDF size (for {imageCount} page(s)): ~{FormatSize(totalBytes)}";
        }
        else
        {
            _sizeLabel.Text = $"Estimated PDF size (per standard page): ~{FormatSize(singlePageBytes)}";
        }
    }

    private string FormatSize(double bytes)
    {
        if (bytes >= 1024 * 1024)
        {
            return $"{bytes / (1024 * 1024):F1} MB";
        }
        return $"{bytes / 1024:F0} KB";
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
            Ocr = _ocr.IsChecked(),
            JpegQuality = _jpegQuality.IntValue,
            ResolutionScale = _resolutionScale.IntValue,
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