using System.Linq.Expressions;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.ImportExport.Images;

namespace NAPS2.EtoForms.Ui;

internal class ImageSettingsPanel : SettingsPanelWithRememberCheckbox<ImageSettings>
{
    private readonly FilePathWithPlaceholders _defaultFilePath;
    private readonly CheckBox _skipSavePrompt = new() { Text = UiStrings.SkipSavePrompt };
    private readonly SliderWithTextBox _jpegQuality = new(new SliderWithTextBox.IntConstraints(0, 100, 25));
    private readonly CheckBox _singlePageTiff = new() { Text = UiStrings.SinglePageFiles };
    private readonly EnumDropDownWidget<TiffCompression> _compression = new();

    public ImageSettingsPanel(IFormBase form, DialogHelper dialogHelper) : base(form)
    {
        _defaultFilePath = new(form, dialogHelper) { ImagesOnly = true };

        LoadValues(Config);

        _defaultFilePath.TextChanged += DefaultFilePath_TextChanged;
    }

    public override string Title => UiStrings.ImageSettings;

    public override string IconName => "picture";

    protected override Expression<Func<CommonConfig, ImageSettings>> SettingsAccessor => c => c.ImageSettings;

    protected override Expression<Func<CommonConfig, bool>> RememberAccessor => c => c.RememberImageSettings;

    public override LayoutElement BuildScrollableLayout()
    {
        return L.Column(
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
            )
        );
    }

    protected override ImageSettings SettingsValues
    {
        get =>
            new()
            {
                DefaultFileName = _defaultFilePath.Text,
                SkipSavePrompt = _skipSavePrompt.IsChecked(),
                JpegQuality = _jpegQuality.IntValue,
                TiffCompression = _compression.SelectedItem,
                SinglePageTiff = _singlePageTiff.IsChecked()
            };
        set
        {
            _defaultFilePath.Text = value.DefaultFileName;
            _skipSavePrompt.Checked = value.SkipSavePrompt;
            _jpegQuality.IntValue = value.JpegQuality;
            _singlePageTiff.Checked = value.SinglePageTiff;
            _compression.SelectedItem = value.TiffCompression;
            UpdateEnabled();
        }
    }

    private void UpdateEnabled()
    {
        _skipSavePrompt.Enabled = Path.IsPathRooted(_defaultFilePath.Text);
    }

    private void DefaultFilePath_TextChanged(object? sender, EventArgs e)
    {
        UpdateEnabled();
    }
}