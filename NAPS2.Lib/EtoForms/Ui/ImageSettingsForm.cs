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
    private readonly CheckBox _singlePageTiff = new() { Text = UiStrings.SinglePageFiles };
    private readonly EnumDropDownWidget<TiffCompression> _compression = new();
    private readonly CheckBox _rememberSettings = new() { Text = UiStrings.RememberTheseSettings };
    private readonly Button _restoreDefaults = new() { Text = UiStrings.RestoreDefaults };

    public ImageSettingsForm(Naps2Config config, DialogHelper dialogHelper, IIconProvider iconProvider) : base(config)
    {
        Title = UiStrings.ImageSettingsFormTitle;
        Icon = iconProvider.GetFormIcon("picture_small");

        _defaultFilePath = new(this, dialogHelper) { ImagesOnly = true };

        UpdateValues(Config);
        UpdateEnabled();

        _restoreDefaults.Click += RestoreDefaults_Click;
        _defaultFilePath.TextChanged += DefaultFilePath_TextChanged;
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
                    C.Label(UiStrings.JpegQualityHelp).DynamicWrap(300)
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
        _defaultFilePath.Text = config.Get(c => c.ImageSettings.DefaultFileName);
        _skipSavePrompt.Checked = config.Get(c => c.ImageSettings.SkipSavePrompt);
        _jpegQuality.IntValue = config.Get(c => c.ImageSettings.JpegQuality);
        _singlePageTiff.Checked = config.Get(c => c.ImageSettings.SinglePageTiff);
        _compression.SelectedItem = config.Get(c => c.ImageSettings.TiffCompression);
        _rememberSettings.Checked = config.Get(c => c.RememberImageSettings);
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