using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.ImportExport.Images;

namespace NAPS2.EtoForms.Ui;

public class ImageSettingsForm : EtoDialogBase
{
    private readonly FilePathWithPlaceholders _defaultFilePath;
    private readonly CheckBox _skipSavePrompt = new() { Text = UiStrings.SkipSavePrompt };
    private readonly SliderWithTextBox _jpegQuality = new(new SliderWithTextBox.IntConstraints(0, 100, 25));
    private readonly SliderWithTextBox _resolutionScale = new(new SliderWithTextBox.IntConstraints(10, 100, 10));
    private readonly CheckBox _singlePageTiff = new() { Text = UiStrings.SinglePageFiles };
    private readonly EnumDropDownWidget<TiffCompression> _compression = new();
    private readonly CheckBox _rememberSettings = new() { Text = UiStrings.RememberTheseSettings };
    private readonly Button _restoreDefaults = new() { Text = UiStrings.RestoreDefaults };

    private readonly UiImageList? _imageList;
    private readonly List<(int Width, int Height, int Dpi)> _imageDimensions = new();
    private readonly Label _resolutionLabel = new() { Font = Fonts.Sans(9.0f), TextColor = Colors.Gray };
    private readonly Label _sizeLabel = new() { Font = Fonts.Sans(9.0f), TextColor = Colors.Gray };

    public ImageSettingsForm(Naps2Config config, DialogHelper dialogHelper, IIconProvider iconProvider, UiImageList imageList) : base(config)
    {
        _imageList = imageList;
        Title = UiStrings.ImageSettingsFormTitle;
        IconName = "picture_small";

        _defaultFilePath = new(this, dialogHelper) { ImagesOnly = true };

        UpdateValues(Config);
        UpdateEnabled();

        _restoreDefaults.Click += RestoreDefaults_Click;
        _defaultFilePath.TextChanged += DefaultFilePath_TextChanged;

        _jpegQuality.ValueChanged += UpdateResolutionAndSizeEstimation;
        _resolutionScale.ValueChanged += UpdateResolutionAndSizeEstimation;
    }

    protected override void BuildLayout()
    {
        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);
        FormStateController.FixedHeightLayout = true;

        LayoutController.Content = L.Column(
            C.Label(UiStrings.DefaultFilePathLabel),
            _defaultFilePath,
            _skipSavePrompt,
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
                UiStrings.TiffOptions,
                L.Column(
                    _singlePageTiff,
                    C.Label(UiStrings.CompressionLabel),
                    _compression
                )
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

        _defaultFilePath.Text = config.Get(c => c.ImageSettings.DefaultFileName);
        _skipSavePrompt.Checked = config.Get(c => c.ImageSettings.SkipSavePrompt);
        _jpegQuality.IntValue = config.Get(c => c.ImageSettings.JpegQuality);
        _resolutionScale.IntValue = config.Get(c => c.ImageSettings.ResolutionScale);
        _singlePageTiff.Checked = config.Get(c => c.ImageSettings.SinglePageTiff);
        _compression.SelectedItem = config.Get(c => c.ImageSettings.TiffCompression);
        _rememberSettings.Checked = config.Get(c => c.RememberImageSettings);

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
            _sizeLabel.Text = $"Estimated size per image: ~{FormatSize(singlePageBytes)}\nTotal size for {imageCount} image(s): ~{FormatSize(totalBytes)}";
        }
        else
        {
            _sizeLabel.Text = $"Estimated size (per standard page): ~{FormatSize(singlePageBytes)}";
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
    }

    private void Save()
    {
        var imageSettings = new ImageSettings
        {
            DefaultFileName = _defaultFilePath.Text,
            SkipSavePrompt = _skipSavePrompt.IsChecked(),
            JpegQuality = _jpegQuality.IntValue,
            ResolutionScale = _resolutionScale.IntValue,
            TiffCompression = _compression.SelectedItem,
            SinglePageTiff = _singlePageTiff.IsChecked()
        };

        var runTransact = Config.Run.BeginTransaction();
        var userTransact = Config.User.BeginTransaction();
        bool remember = _rememberSettings.IsChecked();
        var transactToWrite = remember ? userTransact : runTransact;

        runTransact.Remove(c => c.ImageSettings);
        userTransact.Remove(c => c.ImageSettings);
        transactToWrite.Set(c => c.ImageSettings, imageSettings);
        userTransact.Set(c => c.RememberImageSettings, remember);

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
}