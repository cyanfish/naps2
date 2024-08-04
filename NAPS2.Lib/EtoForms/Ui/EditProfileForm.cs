using System.Globalization;
using System.Threading;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.Scan;
using NAPS2.Scan.Internal;

namespace NAPS2.EtoForms.Ui;

public class EditProfileForm : EtoDialogBase
{
    private readonly IScanPerformer _scanPerformer;
    private readonly ErrorOutput _errorOutput;
    private readonly ProfileNameTracker _profileNameTracker;
    private readonly DeviceCapsCache _deviceCapsCache;

    private readonly TextBox _displayName = new();
    private readonly ImageView _deviceIcon = new();
    private readonly Label _deviceName = new();
    private readonly Label _deviceDriver = new();
    private readonly LayoutVisibility _deviceVis = new(false);
    private readonly Button _chooseDevice = new() { Text = UiStrings.ChooseDevice };
    private readonly RadioButton _predefinedSettings;
    private readonly RadioButton _nativeUi;
    private readonly DropDown _paperSource = C.EnumDropDown<ScanSource>();
    private readonly DropDown _pageSize = C.EnumDropDown<ScanPageSize>();
    private readonly DropDown _resolution = C.EnumDropDown<ScanDpi>(
        dpi => string.Format(SettingsResources.DpiFormat, dpi.ToIntDpi().ToString(CultureInfo.CurrentCulture)));
    private readonly DropDown _bitDepth = C.EnumDropDown<ScanBitDepth>();
    private readonly DropDown _horAlign = C.EnumDropDown<ScanHorizontalAlign>();
    private readonly DropDown _scale = C.EnumDropDown<ScanScale>();
    private readonly CheckBox _enableAutoSave = new() { Text = UiStrings.EnableAutoSave };
    private readonly LinkButton _autoSaveSettings = new() { Text = UiStrings.AutoSaveSettings };
    private readonly Button _advanced = new() { Text = UiStrings.Advanced };
    private readonly SliderWithTextBox _brightnessSlider = new();
    private readonly SliderWithTextBox _contrastSlider = new();

    private ScanProfile _scanProfile = null!;
    private DeviceChoice _currentDevice = DeviceChoice.None;
    private bool _isDefault;
    private bool _result;
    private bool _suppressChangeEvent;
    private bool _suppressPageSizeEvent;
    private CancellationTokenSource? _loadIconCts;

    public EditProfileForm(Naps2Config config, IScanPerformer scanPerformer, ErrorOutput errorOutput,
        ProfileNameTracker profileNameTracker, DeviceCapsCache deviceCapsCache) : base(config)
    {
        _scanPerformer = scanPerformer;
        _errorOutput = errorOutput;
        _profileNameTracker = profileNameTracker;
        _deviceCapsCache = deviceCapsCache;

        _predefinedSettings = new RadioButton { Text = UiStrings.UsePredefinedSettings };
        _nativeUi = new RadioButton(_predefinedSettings) { Text = UiStrings.UseNativeUi };
        _pageSize.SelectedIndexChanged += PageSize_SelectedIndexChanged;
        _predefinedSettings.CheckedChanged += PredefinedSettings_CheckedChanged;
        _nativeUi.CheckedChanged += NativeUi_CheckedChanged;

        _chooseDevice.Click += ChooseDevice;
        _enableAutoSave.CheckedChanged += EnableAutoSave_CheckedChanged;
        _autoSaveSettings.Click += AutoSaveSettings_LinkClicked;
        _advanced.Click += Advanced_Click;
    }

    protected override void BuildLayout()
    {
        Title = UiStrings.EditProfileFormTitle;
        Icon = new Icon(1f, Icons.blueprints_small.ToEtoImage());

        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);
        FormStateController.FixedHeightLayout = true;

        LayoutController.Content = L.Column(
            C.Label(UiStrings.DisplayNameLabel),
            _displayName,
            C.Spacer(),
            L.GroupBox(UiStrings.DeviceLabel,
                L.Row(
                    _deviceIcon.Visible(_deviceVis).AlignCenter(),
                    L.Column(
                        C.Filler(),
                        _deviceName,
                        _deviceDriver,
                        C.Filler()
                    ).Spacing(5).Visible(_deviceVis).Scale(),
                    _chooseDevice.AlignCenter()
                )
            ),
            C.Spacer(),
            PlatformCompat.System.IsWiaDriverSupported || PlatformCompat.System.IsTwainDriverSupported
                ? L.Row(
                    _predefinedSettings,
                    _nativeUi
                )
                : C.None(),
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
                    C.OkButton(this, SaveSettings),
                    C.CancelButton(this))
            )
        );
    }

    public bool Result => _result;

    public ScanProfile ScanProfile
    {
        get => _scanProfile;
        set => _scanProfile = value.Clone();
    }

    public bool NewProfile { get; set; }

    public DeviceChoice CurrentDevice
    {
        get => _currentDevice;
        set
        {
            _currentDevice = value;
            if (value == DeviceChoice.None)
            {
                _deviceName.Text = "";
                _deviceVis.IsVisible = false;
            }
            else
            {
                _deviceName.Text = value.Device?.Name ?? UiStrings.AlwaysAsk;
                _deviceDriver.Text = value.Driver switch
                {
                    Driver.Wia => UiStrings.WiaDriver,
                    Driver.Twain => UiStrings.TwainDriver,
                    Driver.Sane => UiStrings.SaneDriver,
                    Driver.Escl => UiStrings.EsclDriver,
                    Driver.Apple => UiStrings.AppleDriver,
                    _ => ""
                };
                _deviceVis.IsVisible = true;

                var iconUri = value.Device?.Caps?.MetadataCaps?.IconUri;
                var cachedIcon = _deviceCapsCache.GetCachedIcon(iconUri);
                _deviceIcon.Image = cachedIcon ?? (value.AlwaysAsk ? Icons.ask.ToEtoImage() : Icons.device.ToEtoImage());
                LayoutController.Invalidate();
                if (cachedIcon == null && iconUri != null)
                {
                    ReloadDeviceIcon(iconUri);
                }
            }
        }
    }

    private void ReloadDeviceIcon(string iconUri)
    {
        var cts = new CancellationTokenSource();
        _loadIconCts?.Cancel();
        _loadIconCts = cts;
        Task.Run(async () =>
        {
            var icon = await _deviceCapsCache.LoadIcon(iconUri);
            if (icon != null)
            {
                Invoker.Current.Invoke(() =>
                {
                    if (!cts.IsCancellationRequested)
                    {
                        _deviceIcon.Image = icon;
                        LayoutController.Invalidate();
                    }
                });
            }
        });
    }

    private Driver DeviceDriver { get; set; }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // Don't trigger any onChange events
        _suppressChangeEvent = true;

        DeviceDriver = new ScanOptionsValidator().ValidateDriver(
            Enum.TryParse<Driver>(ScanProfile.DriverName, true, out var driver)
                ? driver
                : Driver.Default);

        _displayName.Text = ScanProfile.DisplayName;
        if (CurrentDevice == DeviceChoice.None)
        {
            var device = ScanProfile.Device?.ToScanDevice(DeviceDriver);
            if (device != null)
            {
                CurrentDevice = DeviceChoice.ForDevice(device);
            }
            else if (!NewProfile)
            {
                CurrentDevice = DeviceChoice.ForAlwaysAsk(DeviceDriver);
            }
        }
        _isDefault = ScanProfile.IsDefault;

        _paperSource.SelectedIndex = (int) ScanProfile.PaperSource;
        _bitDepth.SelectedIndex = (int) ScanProfile.BitDepth;
        _resolution.SelectedIndex = (int) ScanProfile.Resolution;
        _contrastSlider.IntValue = ScanProfile.Contrast;
        _brightnessSlider.IntValue = ScanProfile.Brightness;
        UpdatePageSizeList();
        SelectPageSize();
        _scale.SelectedIndex = (int) ScanProfile.AfterScanScale;
        _horAlign.SelectedIndex = (int) ScanProfile.PageAlign;

        _enableAutoSave.Checked = ScanProfile.EnableAutoSave;

        _nativeUi.Checked = ScanProfile.UseNativeUI;
        _predefinedSettings.Checked = !ScanProfile.UseNativeUI;

        // Start triggering onChange events again
        _suppressChangeEvent = false;

        UpdateEnabledControls();
    }

    private async void ChooseDevice(object? sender, EventArgs args)
    {
        ScanProfile.DriverName = DeviceDriver.ToString().ToLowerInvariant();
        var choice = await _scanPerformer.PromptForDevice(ScanProfile, true, NativeHandle);
        if (choice.Device != null || choice.AlwaysAsk)
        {
            if ((string.IsNullOrEmpty(_displayName.Text) ||
                 CurrentDevice.Device?.Name == _displayName.Text) && !choice.AlwaysAsk)
            {
                _displayName.Text = choice.Device!.Name;
            }
            CurrentDevice = choice;
            DeviceDriver = choice.Driver;
            UpdateEnabledControls();
        }
    }

    private void UpdatePageSizeList()
    {
        _suppressPageSizeEvent = true;
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
        _suppressPageSizeEvent = false;
    }

    private void SelectPageSize()
    {
        if (ScanProfile.PageSize == ScanPageSize.Custom)
        {
            if (ScanProfile.CustomPageSize != null)
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

    private void SelectCustomPageSize(string? name, PageDimensions dimens)
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

    private bool SaveSettings()
    {
        if (_displayName.Text == "")
        {
            _errorOutput.DisplayError(MiscResources.NameMissing);
            return false;
        }
        if (CurrentDevice == DeviceChoice.None)
        {
            _errorOutput.DisplayError(MiscResources.NoDeviceSelected);
            return false;
        }
        _result = true;

        if (ScanProfile.IsLocked)
        {
            if (!ScanProfile.IsDeviceLocked)
            {
                ScanProfile.Device = ScanProfileDevice.FromScanDevice(CurrentDevice.Device);
            }
            return true;
        }
        var pageSize = (PageSizeListItem) _pageSize.SelectedValue;
        if (ScanProfile.DisplayName != null)
        {
            _profileNameTracker.RenamingProfile(ScanProfile.DisplayName, _displayName.Text);
        }
        _scanProfile = new ScanProfile
        {
            Version = ScanProfile.CURRENT_VERSION,

            Device = ScanProfileDevice.FromScanDevice(CurrentDevice.Device),
            IsDefault = _isDefault,
            DriverName = DeviceDriver.ToString().ToLowerInvariant(),
            DisplayName = _displayName.Text,
            IconID = 0,
            MaxQuality = ScanProfile.MaxQuality,
            UseNativeUI = _nativeUi.Checked,

            AfterScanScale = (ScanScale) _scale.SelectedIndex,
            BitDepth = (ScanBitDepth) _bitDepth.SelectedIndex,
            Brightness = _brightnessSlider.IntValue,
            Contrast = _contrastSlider.IntValue,
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
            TwainProgress = ScanProfile.TwainProgress,

            ExcludeBlankPages = ScanProfile.ExcludeBlankPages,
            BlankPageWhiteThreshold = ScanProfile.BlankPageWhiteThreshold,
            BlankPageCoverageThreshold = ScanProfile.BlankPageCoverageThreshold
        };
        return true;
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

    private int _lastPageSizeIndex = -1;
    private PageSizeListItem? _lastPageSizeItem;

    private void PageSize_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_suppressPageSizeEvent) return;

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
        ScanProfile.BitDepth = (ScanBitDepth) _bitDepth.SelectedIndex;
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

    private class PageSizeListItem : IListItem
    {
        public string Text { get; set; } = null!;

        public string Key => Text;

        public ScanPageSize Type { get; set; }

        public string? CustomName { get; set; }

        public PageDimensions? CustomDimens { get; set; }
    }
}