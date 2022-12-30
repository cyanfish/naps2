using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Internal;

namespace NAPS2.EtoForms.Ui;

public class EditProfileForm : EtoDialogBase
{
    private readonly IScanPerformer _scanPerformer;
    private readonly ErrorOutput _errorOutput;
    private readonly ProfileNameTracker _profileNameTracker;

    private readonly TextBox _displayName = new();
    private readonly RadioButton _wiaDriver;
    private readonly RadioButton _twainDriver;
    private readonly RadioButton _appleDriver;
    private readonly RadioButton _saneDriver;
    private readonly TextBox _deviceName = new() { Enabled = false };
    private readonly Button _chooseDevice = new() { Text = UiStrings.ChooseDevice };
    private readonly RadioButton _predefinedSettings;
    private readonly RadioButton _nativeUi;
    private readonly DropDown _paperSource = C.EnumDropDown<ScanSource>();
    private readonly DropDown _pageSize = C.EnumDropDown<ScanPageSize>();
    private readonly DropDown _resolution = C.EnumDropDown<ScanDpi>();
    private readonly DropDown _bitDepth = C.EnumDropDown<ScanBitDepth>();
    private readonly DropDown _horAlign = C.EnumDropDown<ScanHorizontalAlign>();
    private readonly DropDown _scale = C.EnumDropDown<ScanScale>();
    private readonly CheckBox _enableAutoSave = new() { Text = UiStrings.EnableAutoSave };
    private readonly LinkButton _autoSaveSettings = new() { Text = UiStrings.AutoSaveSettings };
    private readonly Button _advanced = new() { Text = UiStrings.Advanced };
    private readonly Button _ok = new() { Text = UiStrings.OK };
    private readonly Button _cancel = new() { Text = UiStrings.Cancel };
    private readonly SliderWithTextBox _brightnessSlider = new();
    private readonly SliderWithTextBox _contrastSlider = new();

    private ScanProfile _scanProfile = null!;
    private ScanDevice? _currentDevice;
    private bool _isDefault;
    private bool _result;
    private bool _suppressChangeEvent;

    public EditProfileForm(Naps2Config config, IScanPerformer scanPerformer, ErrorOutput errorOutput,
        ProfileNameTracker profileNameTracker) : base(config)
    {
        _scanPerformer = scanPerformer;
        _errorOutput = errorOutput;
        _profileNameTracker = profileNameTracker;

        _wiaDriver = new RadioButton { Text = UiStrings.WiaDriver };
        _twainDriver = new RadioButton(_wiaDriver) { Text = UiStrings.TwainDriver };
        _appleDriver = new RadioButton(_wiaDriver) { Text = UiStrings.AppleDriver };
        _saneDriver = new RadioButton(_wiaDriver) { Text = UiStrings.SaneDriver };
        _predefinedSettings = new RadioButton { Text = UiStrings.UsePredefinedSettings };
        _nativeUi = new RadioButton(_predefinedSettings) { Text = UiStrings.UseNativeUi };
        _pageSize.SelectedIndexChanged += PageSize_SelectedIndexChanged;
        _wiaDriver.CheckedChanged += Driver_CheckedChanged;
        _twainDriver.CheckedChanged += Driver_CheckedChanged;
        _appleDriver.CheckedChanged += Driver_CheckedChanged;
        _saneDriver.CheckedChanged += Driver_CheckedChanged;
        _predefinedSettings.CheckedChanged += PredefinedSettings_CheckedChanged;
        _nativeUi.CheckedChanged += NativeUi_CheckedChanged;
        _ok.Click += Ok_Click;
        _cancel.Click += Cancel_Click;

        _chooseDevice.Click += ChooseDevice;
        _enableAutoSave.CheckedChanged += EnableAutoSave_CheckedChanged;
        _autoSaveSettings.Click += AutoSaveSettings_LinkClicked;
        _advanced.Click += Advanced_Click;
        _deviceName.KeyDown += DeviceName_KeyDown;
    }

    protected override void BuildLayout()
    {
        // TODO: Don't show if only one driver is available
        var driverElements = new List<LayoutElement>();
        if (PlatformCompat.System.IsWiaDriverSupported)
        {
            driverElements.Add(_wiaDriver.Scale());
        }
        if (PlatformCompat.System.IsTwainDriverSupported)
        {
            driverElements.Add(_twainDriver.Scale());
        }
        if (PlatformCompat.System.IsAppleDriverSupported)
        {
            driverElements.Add(_appleDriver.Scale());
        }
        if (PlatformCompat.System.IsSaneDriverSupported)
        {
            driverElements.Add(_saneDriver.Scale());
        }

        Title = UiStrings.EditProfileFormTitle;
        Icon = new Icon(1f, Icons.blueprints_small.ToEtoImage());

        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);
        FormStateController.FixedHeightLayout = true;

        LayoutController.Content = L.Column(
            L.Row(
                L.Column(
                    C.Label(UiStrings.DisplayNameLabel),
                    _displayName,
                    L.Row(
                        driverElements.ToArray()
                    ),
                    C.Spacer(),
                    C.Label(UiStrings.DeviceLabel),
                    L.Row(
                        _deviceName.Scale(),
                        _chooseDevice
                    )
                ).Scale(),
                new ImageView { Image = Icons.scanner_48.ToEtoImage() }
            ),
            C.Spacer(),
            L.Row(
                _predefinedSettings,
                _nativeUi
            ),
            C.Spacer(),
            L.Row(
                L.Column(
                    C.Label(UiStrings.PaperSourceLabel),
                    _paperSource,
                    C.Label(UiStrings.PageSizeLabel),
                    _pageSize,
                    C.Label(UiStrings.ResolutionLabel),
                    _resolution,
                    C.Label(UiStrings.BrightnessLabel),
                    _brightnessSlider
                ).Scale(),
                L.Column(
                    C.Label(UiStrings.BitDepthLabel),
                    _bitDepth,
                    C.Label(UiStrings.HorizontalAlignLabel),
                    _horAlign,
                    C.Label(UiStrings.ScaleLabel),
                    _scale,
                    C.Label(UiStrings.ContrastLabel),
                    _contrastSlider
                ).Scale()
            ),
            L.Row(
                _enableAutoSave,
                _autoSaveSettings
            ),
            C.Filler(),
            L.Row(
                _advanced,
                C.Filler(),
                L.OkCancel(
                    _ok,
                    _cancel)
            )
        );
    }

    public bool Result => _result;

    public ScanProfile ScanProfile
    {
        get => _scanProfile;
        set => _scanProfile = value.Clone();
    }

    public ScanDevice? CurrentDevice
    {
        get => _currentDevice;
        set
        {
            _currentDevice = value;
            _deviceName.Text = value?.Name ?? "";
        }
    }

    private Driver DeviceDriver
    {
        get => _twainDriver.Checked ? Driver.Twain
            : _wiaDriver.Checked ? Driver.Wia
            : _appleDriver.Checked ? Driver.Apple
            : _saneDriver.Checked ? Driver.Sane
            : ScanOptionsValidator.SystemDefaultDriver;
        set
        {
            if (value == Driver.Twain)
            {
                _twainDriver.Checked = true;
            }
            else if (value == Driver.Wia)
            {
                _wiaDriver.Checked = true;
            }
            else if (value == Driver.Apple)
            {
                _appleDriver.Checked = true;
            }
            else if (value == Driver.Sane)
            {
                _saneDriver.Checked = true;
            }
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // Don't trigger any onChange events
        _suppressChangeEvent = true;

        _displayName.Text = ScanProfile.DisplayName;
        CurrentDevice ??= ScanProfile.Device;
        _isDefault = ScanProfile.IsDefault;

        _paperSource.SelectedIndex = (int) ScanProfile.PaperSource;
        _bitDepth.SelectedIndex = (int) ScanProfile.BitDepth;
        _resolution.SelectedIndex = (int) ScanProfile.Resolution;
        _contrastSlider.Value = ScanProfile.Contrast;
        _brightnessSlider.Value = ScanProfile.Brightness;
        UpdatePageSizeList();
        SelectPageSize();
        _scale.SelectedIndex = (int) ScanProfile.AfterScanScale;
        _horAlign.SelectedIndex = (int) ScanProfile.PageAlign;

        _enableAutoSave.Checked = ScanProfile.EnableAutoSave;

        DeviceDriver = new ScanOptionsValidator().ValidateDriver(
            Enum.TryParse<Driver>(ScanProfile.DriverName, true, out var driver)
                ? driver
                : Driver.Default);

        _nativeUi.Checked = ScanProfile.UseNativeUI;
        _predefinedSettings.Checked = !ScanProfile.UseNativeUI;

        // Start triggering onChange events again
        _suppressChangeEvent = false;

        UpdateEnabledControls();
    }

    private async void ChooseDevice(object? sender, EventArgs args)
    {
        ScanProfile.DriverName = DeviceDriver.ToString().ToLowerInvariant();
        try
        {
            var device = await _scanPerformer.PromptForDevice(ScanProfile, NativeHandle);
            if (device != null)
            {
                if (string.IsNullOrEmpty(_displayName.Text) ||
                    CurrentDevice != null && CurrentDevice.Name == _displayName.Text)
                {
                    _displayName.Text = device.Name;
                }
                CurrentDevice = device;
            }
        }
        catch (ScanDriverException ex)
        {
            if (ex is ScanDriverUnknownException)
            {
                Log.ErrorException(ex.Message, ex.InnerException!);
                _errorOutput.DisplayError(ex.Message, ex);
            }
            else
            {
                _errorOutput.DisplayError(ex.Message);
            }
        }
    }

    private void UpdatePageSizeList()
    {
        _pageSize.Items.Clear();

        // Defaults
        foreach (ScanPageSize item in Enum.GetValues(typeof(ScanPageSize)))
        {
            _pageSize.Items.Add(new PageSizeListItem
            {
                Type = item,
                Text = item.Description()
            });
        }

        // Custom Presets
        foreach (var preset in Config.Get(c => c.CustomPageSizePresets).OrderBy(x => x.Name))
        {
            _pageSize.Items.Insert(_pageSize.Items.Count - 1, new PageSizeListItem
            {
                Type = ScanPageSize.Custom,
                Text = string.Format(MiscResources.NamedPageSizeFormat, preset.Name, preset.Dimens.Width,
                    preset.Dimens.Height, preset.Dimens.Unit.Description()),
                CustomName = preset.Name,
                CustomDimens = preset.Dimens
            });
        }
    }

    private void SelectPageSize()
    {
        if (ScanProfile.PageSize == ScanPageSize.Custom)
        {
            if (ScanProfile.CustomPageSizeName != null && ScanProfile.CustomPageSize != null)
            {
                SelectCustomPageSize(ScanProfile.CustomPageSizeName, ScanProfile.CustomPageSize);
            }
            else
            {
                _pageSize.SelectedIndex = 0;
            }
        }
        else
        {
            _pageSize.SelectedIndex = (int) ScanProfile.PageSize;
        }
    }

    private void SelectCustomPageSize(string name, PageDimensions dimens)
    {
        for (int i = 0; i < _pageSize.Items.Count; i++)
        {
            var item = (PageSizeListItem) _pageSize.Items[i];
            if (item.Type == ScanPageSize.Custom && item.CustomName == name && item.CustomDimens == dimens)
            {
                _pageSize.SelectedIndex = i;
                return;
            }
        }

        // Not found, so insert a new item
        _pageSize.Items.Insert(_pageSize.Items.Count - 1, new PageSizeListItem
        {
            Type = ScanPageSize.Custom,
            Text = string.IsNullOrEmpty(name)
                ? string.Format(MiscResources.CustomPageSizeFormat, dimens.Width, dimens.Height,
                    dimens.Unit.Description())
                : string.Format(MiscResources.NamedPageSizeFormat, name, dimens.Width, dimens.Height,
                    dimens.Unit.Description()),
            CustomName = name,
            CustomDimens = dimens
        });
        _pageSize.SelectedIndex = _pageSize.Items.Count - 2;
    }


    private void SaveSettings()
    {
        if (ScanProfile.IsLocked)
        {
            if (!ScanProfile.IsDeviceLocked)
            {
                ScanProfile.Device = CurrentDevice;
            }
            return;
        }
        var pageSize = (PageSizeListItem) _pageSize.SelectedValue;
        if (ScanProfile.DisplayName != null)
        {
            _profileNameTracker.RenamingProfile(ScanProfile.DisplayName, _displayName.Text);
        }
        _scanProfile = new ScanProfile
        {
            Version = ScanProfile.CURRENT_VERSION,

            Device = CurrentDevice,
            IsDefault = _isDefault,
            DriverName = DeviceDriver.ToString().ToLowerInvariant(),
            DisplayName = _displayName.Text,
            IconID = 0,
            MaxQuality = ScanProfile.MaxQuality,
            UseNativeUI = _nativeUi.Checked,

            AfterScanScale = (ScanScale) _scale.SelectedIndex,
            BitDepth = (ScanBitDepth) _bitDepth.SelectedIndex,
            Brightness = _brightnessSlider.Value,
            Contrast = _contrastSlider.Value,
            PageAlign = (ScanHorizontalAlign) _horAlign.SelectedIndex,
            PageSize = pageSize.Type,
            CustomPageSizeName = pageSize.CustomName,
            CustomPageSize = pageSize.CustomDimens,
            Resolution = (ScanDpi) _resolution.SelectedIndex,
            PaperSource = (ScanSource) _paperSource.SelectedIndex,

            EnableAutoSave = _enableAutoSave.IsChecked(),
            AutoSaveSettings = ScanProfile.AutoSaveSettings,
            Quality = ScanProfile.Quality,
            BrightnessContrastAfterScan = ScanProfile.BrightnessContrastAfterScan,
            AutoDeskew = ScanProfile.AutoDeskew,
            WiaOffsetWidth = ScanProfile.WiaOffsetWidth,
            WiaRetryOnFailure = ScanProfile.WiaRetryOnFailure,
            WiaDelayBetweenScans = ScanProfile.WiaDelayBetweenScans,
            WiaDelayBetweenScansSeconds = ScanProfile.WiaDelayBetweenScansSeconds,
            WiaVersion = ScanProfile.WiaVersion,
            ForcePageSize = ScanProfile.ForcePageSize,
            ForcePageSizeCrop = ScanProfile.ForcePageSizeCrop,
            FlipDuplexedPages = ScanProfile.FlipDuplexedPages,
            TwainImpl = ScanProfile.TwainImpl,

            ExcludeBlankPages = ScanProfile.ExcludeBlankPages,
            BlankPageWhiteThreshold = ScanProfile.BlankPageWhiteThreshold,
            BlankPageCoverageThreshold = ScanProfile.BlankPageCoverageThreshold
        };
    }

    private void Ok_Click(object? sender, EventArgs e)
    {
        // Note: If CurrentDevice is null, that's fine. A prompt will be shown when scanning.

        if (_displayName.Text == "")
        {
            _errorOutput.DisplayError(MiscResources.NameMissing);
            return;
        }
        _result = true;
        SaveSettings();
        Close();
    }

    private void Cancel_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private void PredefinedSettings_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateEnabledControls();
    }

    private void NativeUi_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateEnabledControls();
    }

    private void UpdateEnabledControls()
    {
        if (!_suppressChangeEvent)
        {
            _suppressChangeEvent = true;

            bool canUseNativeUi = DeviceDriver is Driver.Wia or Driver.Twain;
            bool locked = ScanProfile.IsLocked;
            bool deviceLocked = ScanProfile.IsDeviceLocked;
            bool settingsEnabled = !locked && (_predefinedSettings.Checked || !canUseNativeUi);

            _displayName.Enabled = !locked;
            _wiaDriver.Enabled = _twainDriver.Enabled = _appleDriver.Enabled = _saneDriver.Enabled = !locked;
            _chooseDevice.Enabled = !deviceLocked;
            _predefinedSettings.Enabled = _nativeUi.Enabled = !locked;

            _paperSource.Enabled = settingsEnabled;
            _resolution.Enabled = settingsEnabled;
            _pageSize.Enabled = settingsEnabled;
            _bitDepth.Enabled = settingsEnabled;
            _horAlign.Enabled = settingsEnabled;
            _scale.Enabled = settingsEnabled;
            _brightnessSlider.Enabled = settingsEnabled;
            _contrastSlider.Enabled = settingsEnabled;

            _enableAutoSave.Enabled = !locked && !Config.Get(c => c.DisableAutoSave);
            _autoSaveSettings.Enabled = _enableAutoSave.IsChecked();
            _autoSaveSettings.Visible = !locked && !Config.Get(c => c.DisableAutoSave);

            _advanced.Enabled = !locked;

            _suppressChangeEvent = false;
        }
    }

    private void Driver_CheckedChanged(object? sender, EventArgs e)
    {
        if (((RadioButton) sender!).Checked && !_suppressChangeEvent)
        {
            ScanProfile.Device = null;
            CurrentDevice = null;
            UpdateEnabledControls();
        }
    }

    private int _lastPageSizeIndex = -1;
    private PageSizeListItem? _lastPageSizeItem;

    private void PageSize_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_pageSize.SelectedIndex == _pageSize.Items.Count - 1)
        {
            if (_lastPageSizeItem == null)
            {
                Log.Error("Expected last page size to be set");
                return;
            }
            // "Custom..." selected
            var form = FormFactory.Create<PageSizeForm>();
            form.PageSizeDimens = _lastPageSizeItem.Type == ScanPageSize.Custom
                ? _lastPageSizeItem.CustomDimens
                : _lastPageSizeItem.Type.PageDimensions();
            form.ShowModal();
            if (form.Result)
            {
                UpdatePageSizeList();
                SelectCustomPageSize(form.PageSizeName!, form.PageSizeDimens!);
            }
            else
            {
                _pageSize.SelectedIndex = _lastPageSizeIndex;
            }
        }
        _lastPageSizeIndex = _pageSize.SelectedIndex;
        _lastPageSizeItem = (PageSizeListItem) _pageSize.SelectedValue;
    }

    private void AutoSaveSettings_LinkClicked(object? sender, EventArgs eventArgs)
    {
        if (Config.Get(c => c.DisableAutoSave))
        {
            return;
        }
        var form = FormFactory.Create<AutoSaveSettingsForm>();
        ScanProfile.DriverName = DeviceDriver.ToString().ToLowerInvariant();
        form.ScanProfile = ScanProfile;
        form.ShowModal();
    }

    private void Advanced_Click(object? sender, EventArgs e)
    {
        var form = FormFactory.Create<AdvancedProfileForm>();
        ScanProfile.DriverName = DeviceDriver.ToString().ToLowerInvariant();
        ScanProfile.BitDepth = (ScanBitDepth)_bitDepth.SelectedIndex;
        form.ScanProfile = ScanProfile;
        form.ShowModal();
    }

    private void EnableAutoSave_CheckedChanged(object? sender, EventArgs e)
    {
        if (!_suppressChangeEvent)
        {
            if (_enableAutoSave.IsChecked())
            {
                _autoSaveSettings.Enabled = true;
                var form = FormFactory.Create<AutoSaveSettingsForm>();
                form.ScanProfile = ScanProfile;
                form.ShowModal();
                if (!form.Result)
                {
                    _enableAutoSave.Checked = false;
                }
            }
        }
        _autoSaveSettings.Enabled = _enableAutoSave.IsChecked();
    }

    private void DeviceName_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Keys.Delete)
        {
            CurrentDevice = null;
        }
    }

    private class PageSizeListItem : IListItem
    {
        public string Text { get; set; } = null!;

        public string Key => Text;

        public ScanPageSize Type { get; set; }

        public string? CustomName { get; set; }

        public PageDimensions? CustomDimens { get; set; }
    }
}