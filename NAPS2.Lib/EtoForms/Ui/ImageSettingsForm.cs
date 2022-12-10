using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.ImportExport.Images;

namespace NAPS2.EtoForms.Ui;

public class ImageSettingsForm : EtoDialogBase
{
    private readonly DialogHelper _dialogHelper;

    private readonly TextBox _defaultFilePath = new();
    private readonly Button _chooseFolder = new() { Text = UiStrings.Ellipsis };
    private readonly LinkButton _placeholders = new() { Text = UiStrings.Placeholders };
    private readonly CheckBox _skipSavePrompt = new() { Text = UiStrings.SkipSavePrompt };
    private readonly SliderWithTextBox _jpegQuality = new() { MinValue = 0, MaxValue = 100, TickFrequency = 25 };
    private readonly CheckBox _singlePageTiff = new() { Text = UiStrings.SinglePageFiles };
    private readonly DropDown _compression = C.EnumDropDown<TiffCompression>();
    private readonly CheckBox _rememberSettings = new() { Text = UiStrings.RememberTheseSettings };
    private readonly Button _restoreDefaults = new() { Text = UiStrings.RestoreDefaults };

    public ImageSettingsForm(Naps2Config config, DialogHelper dialogHelper) : base(config)
    {
        _dialogHelper = dialogHelper;

        UpdateValues(Config);
        UpdateEnabled();

        _restoreDefaults.Click += RestoreDefaults_Click;
        _defaultFilePath.TextChanged += DefaultFilePath_TextChanged;
        _placeholders.Click += Placeholders_Click;
        _chooseFolder.Click += ChooseFolder_Click;
    }

    protected override void BuildLayout()
    {
        Title = UiStrings.ImageSettingsFormTitle;
        Icon = new Icon(1f, Icons.picture_small.ToEtoImage());

        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);
        FormStateController.FixedHeightLayout = true;

        LayoutController.Content = L.Column(
            C.Label(UiStrings.DefaultFilePathLabel),
            L.Row(
                _defaultFilePath.XScale().AlignCenter(),
                _chooseFolder.Width(40).MaxHeight(22)
            ).SpacingAfter(2),
            _placeholders,
            _skipSavePrompt,
            L.GroupBox(
                UiStrings.JpegQuality,
                L.Column(
                    _jpegQuality.AsControl().SpacingAfter(0),
                    C.Label(UiStrings.JpegQualityHelp).Wrap(300)
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
        _jpegQuality.Value = config.Get(c => c.ImageSettings.JpegQuality);
        _singlePageTiff.Checked = config.Get(c => c.ImageSettings.SinglePageTiff);
        _compression.SelectedIndex = (int) config.Get(c => c.ImageSettings.TiffCompression);
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
            JpegQuality = _jpegQuality.Value,
            TiffCompression = (TiffCompression) _compression.SelectedIndex,
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

    private void Placeholders_Click(object? sender, EventArgs eventArgs)
    {
        var form = FormFactory.Create<PlaceholdersForm>();
        form.FileName = _defaultFilePath.Text;
        form.ShowModal();
        if (form.Updated)
        {
            _defaultFilePath.Text = form.FileName;
        }
    }

    private void ChooseFolder_Click(object? sender, EventArgs e)
    {
        if (_dialogHelper.PromptToSaveImage(_defaultFilePath.Text, out string? savePath))
        {
            _defaultFilePath.Text = savePath!;
        }
    }
}