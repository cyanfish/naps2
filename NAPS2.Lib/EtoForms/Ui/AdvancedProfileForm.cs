using System.Globalization;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.Images;
using NAPS2.EtoForms.Widgets;
using NAPS2.Lang;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Ui;

public class AdvancedProfileForm : EtoDialogBase
{
    private readonly CheckBox _maximumQuality = new() { Text = UiStrings.MaximumQuality };
    private readonly SliderWithTextBox _quality = new(new SliderWithTextBox.IntConstraints(0, 100, 25));
    private readonly CheckBox _excludeBlank = new() { Text = UiStrings.ExcludeBlankPages };
    private readonly SliderWithTextBox _whiteThreshold = new(new SliderWithTextBox.IntConstraints(0, 100, 10));
    private readonly SliderWithTextBox _coverageThreshold = new(new SliderWithTextBox.IntConstraints(0, 100, 10));
    private readonly CheckBox _deskew = new() { Text = UiStrings.DeskewScannedPages };
    private readonly CheckBox _autoCrop = new() { Text = UiStrings.AutoCropScannedPages };
    private readonly EnumDropDownWidget<AutoCropAxisMode> _autoCropWidthMode = new();
    private readonly EnumDropDownWidget<AutoCropAxisMode> _autoCropHeightMode = new();
    private readonly TextBox _autoCropWidth = new();
    private readonly TextBox _autoCropHeight = new();
    private readonly CheckBox _brightContAfterScan = new() { Text = UiStrings.BrightnessContrastAfterScan };
    private readonly CheckBox _offsetWidth = new() { Text = UiStrings.OffsetWidth };
    private readonly CheckBox _stretchToPageSize = new() { Text = UiStrings.StretchToPageSize };
    private readonly CheckBox _cropToPageSize = new() { Text = UiStrings.CropToPageSize };
    private readonly CheckBox _flipDuplexed = new()
    {
        Text = TranslationMigrator.PickTranslated(
            UiStrings.ResourceManager,
            "FlipDuplexedPages",
            "FlipBackSidesOfDuplexPages")
    };

    private readonly EnumDropDownWidget<WiaApiVersion> _wiaVersion = new();

    private readonly EnumDropDownWidget<TwainImpl> _twainImpl = new();
    private readonly CheckBox _twainProgress = new() { Text = UiStrings.ShowNativeTwainProgress };
    private readonly Button _restoreDefaults = new() { Text = UiStrings.RestoreDefaults };

    public AdvancedProfileForm(Naps2Config config, IIconProvider iconProvider) : base(config)
    {
        Title = UiStrings.AdvancedProfileFormTitle;
        IconName = "blueprints_small";

        _restoreDefaults.Click += RestoreDefaults_Click;
        _maximumQuality.CheckedChanged += MaximumQuality_CheckedChanged;
        _excludeBlank.CheckedChanged += ExcludeBlank_CheckedChanged;
        _autoCrop.CheckedChanged += (_, _) => UpdateEnabled();
        _autoCropWidthMode.Format = AutoCropAxisModeText;
        _autoCropHeightMode.Format = AutoCropAxisModeText;
        _autoCropWidthMode.SelectedItemChanged += (_, _) => UpdateEnabled();
        _autoCropHeightMode.SelectedItemChanged += (_, _) => UpdateEnabled();
        _wiaVersion.Format = value => value switch
        {
            WiaApiVersion.Default => SettingsResources.WiaVersion_Default,
            WiaApiVersion.Wia10 => SettingsResources.WiaVersion_Wia10,
            WiaApiVersion.Wia20 => SettingsResources.WiaVersion_Wia20,
            _ => value.ToString()
        };
        if (!Environment.Is64BitProcess)
        {
            _twainImpl.Items = EnumDropDownWidget<TwainImpl>.DefaultItems.Where(x => x != TwainImpl.X64);
        }
        // Remove obsolete options
        _twainImpl.Items = _twainImpl.Items.Except([TwainImpl.Legacy]);
    }

    protected override void BuildLayout()
    {
        UpdateValues(ScanProfile!);
        UpdateEnabled();

        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);
        FormStateController.FixedHeightLayout = true;

        LayoutController.Content = L.Column(
            L.GroupBox(
                UiStrings.ImageQuality,
                L.Column(
                    _maximumQuality,
                    _quality
                )
            ),
            L.GroupBox(
                UiStrings.BlankPages,
                L.Column(
                    _excludeBlank,
                    C.Label(UiStrings.WhiteThreshold),
                    _whiteThreshold,
                    C.Label(UiStrings.CoverageThreshold),
                    _coverageThreshold
                )
            ),
            L.GroupBox(
                UiStrings.PostProcessing,
                L.Column(
                    _deskew,
                    _autoCrop,
                    C.Label(UiStrings.AutoCropWidthLabel),
                    _autoCropWidthMode,
                    L.Row(
                        C.Label(UiStrings.AutoCropFixedSizeLabel).AlignCenter(),
                        _autoCropWidth.Width(70)
                    ),
                    C.Label(UiStrings.AutoCropHeightLabel),
                    _autoCropHeightMode,
                    L.Row(
                        C.Label(UiStrings.AutoCropFixedSizeLabel).AlignCenter(),
                        _autoCropHeight.Width(70)
                    )
                )
            ),
            L.GroupBox(
                UiStrings.Compatibility,
                L.Column(
                    PlatformCompat.System.IsWiaDriverSupported || PlatformCompat.System.IsTwainDriverSupported
                        ? _brightContAfterScan
                        : C.None(),
                    PlatformCompat.System.IsWiaDriverSupported ? _offsetWidth : C.None(),
                    _stretchToPageSize,
                    _cropToPageSize,
                    _flipDuplexed,
                    PlatformCompat.System.IsWiaDriverSupported ? C.Label(UiStrings.WiaVersionLabel) : C.None(),
                    PlatformCompat.System.IsWiaDriverSupported ? _wiaVersion : C.None(),
                    PlatformCompat.System.IsTwainDriverSupported ? C.Label(UiStrings.TwainImplLabel) : C.None(),
                    PlatformCompat.System.IsTwainDriverSupported ? _twainImpl : C.None(),
                    PlatformCompat.System.IsTwainDriverSupported ? _twainProgress : C.None()
                )
            ),
            L.Row(
                _restoreDefaults.MinWidth(140),
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, SaveSettings),
                    C.CancelButton(this))
            )
        );
    }

    private void UpdateValues(ScanProfile scanProfile)
    {
        _maximumQuality.Checked = scanProfile.MaxQuality;
        _quality.IntValue = scanProfile.Quality;
        _brightContAfterScan.Checked = scanProfile.BrightnessContrastAfterScan;
        _deskew.Checked = scanProfile.AutoDeskew;
        _autoCrop.Checked = scanProfile.AutoCrop;
        _autoCropWidthMode.SelectedItem = scanProfile.AutoCropWidthMode;
        _autoCropHeightMode.SelectedItem = scanProfile.AutoCropHeightMode;
        _autoCropWidth.Text = scanProfile.AutoCropFixedWidthMm?.ToString(CultureInfo.CurrentCulture) ?? "";
        _autoCropHeight.Text = scanProfile.AutoCropFixedHeightMm?.ToString(CultureInfo.CurrentCulture) ?? "";
        _offsetWidth.Checked = scanProfile.WiaOffsetWidth;
        _wiaVersion.SelectedItem = scanProfile.WiaVersion;
        _stretchToPageSize.Checked = scanProfile.ForcePageSize;
        _cropToPageSize.Checked = scanProfile.ForcePageSizeCrop;
        _flipDuplexed.Checked = scanProfile.FlipDuplexedPages;
        _twainImpl.SelectedItem = scanProfile.TwainImpl == TwainImpl.Legacy ? TwainImpl.OldDsm : scanProfile.TwainImpl;
        _twainProgress.Checked = scanProfile.TwainProgress;
        _excludeBlank.Checked = scanProfile.ExcludeBlankPages;
        _whiteThreshold.IntValue = scanProfile.BlankPageWhiteThreshold;
        _coverageThreshold.IntValue = scanProfile.BlankPageCoverageThreshold;
    }

    private void UpdateEnabled()
    {
        _quality.Enabled = !_maximumQuality.IsChecked();
        _whiteThreshold.Enabled = _excludeBlank.IsChecked() && ScanProfile!.BitDepth != ScanBitDepth.BlackWhite;
        _coverageThreshold.Enabled = _excludeBlank.IsChecked();
        var autoCrop = _autoCrop.IsChecked();
        _autoCropWidthMode.Enabled = autoCrop;
        _autoCropHeightMode.Enabled = autoCrop;
        _autoCropWidth.Enabled = autoCrop && _autoCropWidthMode.SelectedItem == AutoCropAxisMode.Fixed;
        _autoCropHeight.Enabled = autoCrop && _autoCropHeightMode.SelectedItem == AutoCropAxisMode.Fixed;
    }

    public ScanProfile? ScanProfile { get; set; }

    private void SaveSettings()
    {
        ScanProfile!.Quality = _quality.IntValue;
        ScanProfile.MaxQuality = _maximumQuality.IsChecked();
        ScanProfile.BrightnessContrastAfterScan = _brightContAfterScan.IsChecked();
        ScanProfile.AutoDeskew = _deskew.IsChecked();
        ScanProfile.AutoCrop = _autoCrop.IsChecked();
        ScanProfile.AutoCropWidthMode = _autoCropWidthMode.SelectedItem;
        ScanProfile.AutoCropHeightMode = _autoCropHeightMode.SelectedItem;
        ScanProfile.AutoCropFixedWidthMm = ParseMm(_autoCropWidth.Text);
        ScanProfile.AutoCropFixedHeightMm = ParseMm(_autoCropHeight.Text);
        ScanProfile.WiaOffsetWidth = _offsetWidth.IsChecked();
        ScanProfile.WiaVersion = _wiaVersion.SelectedItem;
        ScanProfile.ForcePageSize = _stretchToPageSize.IsChecked();
        ScanProfile.ForcePageSizeCrop = _cropToPageSize.IsChecked();
        ScanProfile.FlipDuplexedPages = _flipDuplexed.IsChecked();
        ScanProfile.TwainImpl = _twainImpl.SelectedItem;
        ScanProfile.TwainProgress = _twainProgress.IsChecked();
        ScanProfile.ExcludeBlankPages = _excludeBlank.IsChecked();
        ScanProfile.BlankPageWhiteThreshold = _whiteThreshold.IntValue;
        ScanProfile.BlankPageCoverageThreshold = _coverageThreshold.IntValue;
    }

    private static string AutoCropAxisModeText(AutoCropAxisMode mode) => mode switch
    {
        AutoCropAxisMode.Off => UiStrings.AutoCropModeOff,
        AutoCropAxisMode.Auto => UiStrings.AutoCropModeAuto,
        AutoCropAxisMode.Fixed => UiStrings.AutoCropModeFixed,
        _ => mode.ToString()
    };

    private static double? ParseMm(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }
        return double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var value) && value > 0
            ? value
            : null;
    }

    private void MaximumQuality_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateEnabled();
    }

    private void ExcludeBlank_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateEnabled();
    }

    private void RestoreDefaults_Click(object? sender, EventArgs e)
    {
        UpdateValues(Config.DefaultProfileSettings());
    }
}