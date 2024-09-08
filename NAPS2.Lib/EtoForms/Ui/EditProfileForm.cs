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
    private readonly ErrorOutput _errorOutput;
    private readonly ProfileNameTracker _profileNameTracker;
    private readonly DeviceCapsCache _deviceCapsCache;

    private readonly TextBox _displayName = new();
    private readonly DeviceSelectorWidget _deviceSelectorWidget;
    private readonly RadioButton _predefinedSettings;
    private readonly RadioButton _nativeUi;
    private readonly LayoutVisibility _nativeUiVis = new(true);
    private readonly EnumDropDownWidget<ScanSource> _paperSource = new();
    private readonly PageSizeDropDownWidget _pageSize;
    private readonly DropDownWidget<int> _resolution = new();
    private readonly EnumDropDownWidget<ScanBitDepth> _bitDepth = new();
    private readonly EnumDropDownWidget<ScanHorizontalAlign> _horAlign = new();
    private readonly EnumDropDownWidget<ScanScale> _scale = new();
    private readonly CheckBox _enableAutoSave = new() { Text = UiStrings.EnableAutoSave };
    private readonly LinkButton _autoSaveSettings = C.Link(UiStrings.AutoSaveSettings);
    private readonly Button _advanced = new() { Text = UiStrings.Advanced };
    private readonly SliderWithTextBox _brightnessSlider = new();
    private readonly SliderWithTextBox _contrastSlider = new();

    private ScanProfile _scanProfile = null!;
    private bool _isDefault;
    private bool _result;
    private bool _suppressChangeEvent;
    private CancellationTokenSource? _updateCapsCts;

    public EditProfileForm(Naps2Config config, IScanPerformer scanPerformer, ErrorOutput errorOutput,
        ProfileNameTracker profileNameTracker, DeviceCapsCache deviceCapsCache,
        IIconProvider iconProvider) : base(config)
    {
        Title = UiStrings.EditProfileFormTitle;
        IconName = "blueprints_small";

        _errorOutput = errorOutput;
        _profileNameTracker = profileNameTracker;
        _deviceCapsCache = deviceCapsCache;
        _deviceSelectorWidget = new(scanPerformer, deviceCapsCache, iconProvider, this)
        {
            ProfileFunc = GetUpdatedScanProfile,
            AllowAlwaysAsk = true
        };
        _pageSize = new(this);
        _deviceSelectorWidget.DeviceChanged += DeviceChanged;

        _predefinedSettings = new RadioButton { Text = UiStrings.UsePredefinedSettings };
        _nativeUi = new RadioButton(_predefinedSettings) { Text = UiStrings.UseNativeUi };
        _resolution.Format = x => string.Format(SettingsResources.DpiFormat, x.ToString(CultureInfo.InvariantCulture));
        _paperSource.SelectedItemChanged += PaperSource_SelectedItemChanged;
        _predefinedSettings.CheckedChanged += PredefinedSettings_CheckedChanged;
        _nativeUi.CheckedChanged += NativeUi_CheckedChanged;

        _enableAutoSave.CheckedChanged += EnableAutoSave_CheckedChanged;
        _autoSaveSettings.Click += AutoSaveSettings_LinkClicked;
        _advanced.Click += Advanced_Click;
    }

    public void SetDevice(ScanDevice device)
    {
        _deviceSelectorWidget.Choice = DeviceChoice.ForDevice(device);
    }

    private void DeviceChanged(object? sender, DeviceChangedEventArgs e)
    {
        if (e.NewChoice.Device != null && (string.IsNullOrEmpty(_displayName.Text) ||
                                           e.PreviousChoice.Device?.Name == _displayName.Text))
        {
            _displayName.Text = e.NewChoice.Device.Name;
        }
        DeviceDriver = e.NewChoice.Driver;
        IconUri = e.NewChoice.Device?.IconUri;

        UpdateCaps();
        UpdateEnabledControls();
    }

    protected override void BuildLayout()
    {
        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);
        FormStateController.FixedHeightLayout = true;
        base.BuildLayout();

        LayoutController.Content = L.Column(
            C.Label(UiStrings.DisplayNameLabel),
            _displayName,
            C.Spacer(),
            _deviceSelectorWidget,
            C.Spacer(),
            PlatformCompat.System.IsWiaDriverSupported || PlatformCompat.System.IsTwainDriverSupported
                ? L.Row(
                    _predefinedSettings,
                    _nativeUi
                ).Visible(_nativeUiVis)
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
        set
        {
            _scanProfile = value.Clone();
            UpdateUiForScanProfile();
        }
    }

    public bool NewProfile { get; set; }

    private void UpdateUiForCaps()
    {
        _suppressChangeEvent = true;

        _paperSource.Items = ScanProfile.Caps?.PaperSources?.Values is [_, ..] paperSources
            ? paperSources
            : EnumDropDownWidget<ScanSource>.DefaultItems;

        var selectedSource = _paperSource.SelectedItem;
        var perSource = selectedSource switch
        {
            ScanSource.Glass => ScanProfile.Caps?.Glass,
            ScanSource.Feeder => ScanProfile.Caps?.Feeder,
            ScanSource.Duplex => ScanProfile.Caps?.Duplex,
            _ => null
        };

        var validResolutions = perSource?.Resolutions;
        _resolution.Items = validResolutions is [_, ..]
            ? validResolutions
            : EnumDropDownWidget<ScanDpi>.DefaultItems.Select(x => x.ToIntDpi());

        var scanArea = perSource?.ScanArea;
        var sizeCaps = new PageSizeCaps { ScanArea = scanArea };

        var allPresets = EnumDropDownWidget<ScanPageSize>.DefaultItems.SkipLast(2).ToList();
        var conditionalPresets = new[] { ScanPageSize.A3, ScanPageSize.B4 };
        _pageSize.VisiblePresets = allPresets.Where(preset =>
            !conditionalPresets.Contains(preset) || sizeCaps.Fits(preset.PageDimensions()!.ToPageSize()));

        _suppressChangeEvent = false;
    }

    private void UpdateCaps()
    {
        var cts = new CancellationTokenSource();
        _updateCapsCts?.Cancel();
        _updateCapsCts = cts;
        var updatedProfile = GetUpdatedScanProfile();
        var cachedCaps = _deviceCapsCache.GetCachedCaps(updatedProfile);
        if (cachedCaps != null)
        {
            ScanProfile.Caps = MapCaps(cachedCaps);
        }
        else
        {
            ScanProfile.Caps = null;
            if (updatedProfile.Device != null)
            {
                Task.Run(async () =>
                {
                    var caps = await _deviceCapsCache.QueryCaps(updatedProfile);
                    if (caps != null)
                    {
                        Invoker.Current.Invoke(() =>
                        {
                            if (!cts.IsCancellationRequested)
                            {
                                ScanProfile.Caps = MapCaps(caps);
                                UpdateUiForCaps();
                            }
                        });
                    }
                });
            }
        }
        UpdateUiForCaps();
    }

    private ScanProfileCaps MapCaps(ScanCaps? caps)
    {
        List<ScanSource>? paperSources = null;
        if (caps?.PaperSourceCaps is { } paperSourceCaps)
        {
            paperSources = new List<ScanSource>();
            if (paperSourceCaps.SupportsFlatbed) paperSources.Add(ScanSource.Glass);
            if (paperSourceCaps.SupportsFeeder) paperSources.Add(ScanSource.Feeder);
            if (paperSourceCaps.SupportsDuplex) paperSources.Add(ScanSource.Duplex);
        }

        return new ScanProfileCaps
        {
            PaperSources = new PaperSourceProfileCaps { Values = paperSources },
            FeederCheck = caps?.PaperSourceCaps?.CanCheckIfFeederHasPaper,
            Glass = new PerSourceProfileCaps
            {
                ScanArea = caps?.FlatbedCaps?.PageSizeCaps?.ScanArea,
                Resolutions = caps?.FlatbedCaps?.DpiCaps?.CommonValues?.ToList()
            },
            Feeder = new PerSourceProfileCaps
            {
                ScanArea = caps?.FeederCaps?.PageSizeCaps?.ScanArea,
                Resolutions = caps?.FeederCaps?.DpiCaps?.CommonValues?.ToList()
            },
            Duplex = new PerSourceProfileCaps
            {
                ScanArea = caps?.DuplexCaps?.PageSizeCaps?.ScanArea,
                Resolutions = caps?.DuplexCaps?.DpiCaps?.CommonValues?.ToList()
            }
        };
    }

    private Driver DeviceDriver { get; set; }

    private string? IconUri { get; set; }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        UpdateUiForCaps();
        UpdateEnabledControls();
    }

    private void UpdateUiForScanProfile()
    {
        // Don't trigger any onChange events
        _suppressChangeEvent = true;

        DeviceDriver = new ScanOptionsValidator().ValidateDriver(
            Enum.TryParse<Driver>(ScanProfile.DriverName, true, out var driver)
                ? driver
                : Driver.Default);
        IconUri = ScanProfile.Device?.IconUri;

        _displayName.Text = ScanProfile.DisplayName;
        if (_deviceSelectorWidget.Choice == DeviceChoice.None)
        {
            var device = ScanProfile.Device?.ToScanDevice(DeviceDriver);
            if (device != null)
            {
                _deviceSelectorWidget.Choice = DeviceChoice.ForDevice(device);
            }
            else if (!NewProfile)
            {
                _deviceSelectorWidget.Choice = DeviceChoice.ForAlwaysAsk(DeviceDriver);
            }
        }
        _isDefault = ScanProfile.IsDefault;

        if (ScanProfile.PageSize == ScanPageSize.Custom && ScanProfile.CustomPageSize != null)
        {
            _pageSize.SetCustom(ScanProfile.CustomPageSizeName, ScanProfile.CustomPageSize);
        }
        else
        {
            _pageSize.SetPreset(ScanProfile.PageSize);
        }

        _paperSource.SelectedItem = ScanProfile.PaperSource;
        _bitDepth.SelectedItem = ScanProfile.BitDepth;
        _resolution.SelectedItem = ScanProfile.Resolution.Dpi;
        _contrastSlider.IntValue = ScanProfile.Contrast;
        _brightnessSlider.IntValue = ScanProfile.Brightness;
        _scale.SelectedItem = ScanProfile.AfterScanScale;
        _horAlign.SelectedItem = ScanProfile.PageAlign;

        _enableAutoSave.Checked = ScanProfile.EnableAutoSave;

        _nativeUi.Checked = ScanProfile.UseNativeUI;
        _predefinedSettings.Checked = !ScanProfile.UseNativeUI;

        // Start triggering onChange events again
        _suppressChangeEvent = false;
    }

    private bool SaveSettings()
    {
        if (_displayName.Text == "")
        {
            _errorOutput.DisplayError(MiscResources.NameMissing);
            return false;
        }
        if (_deviceSelectorWidget.Choice == DeviceChoice.None)
        {
            _errorOutput.DisplayError(MiscResources.NoDeviceSelected);
            return false;
        }
        _result = true;

        if (ScanProfile.IsLocked)
        {
            if (!ScanProfile.IsDeviceLocked)
            {
                ScanProfile.Device = ScanProfileDevice.FromScanDevice(_deviceSelectorWidget.Choice.Device);
            }
            return true;
        }
        if (ScanProfile.DisplayName != null)
        {
            _profileNameTracker.RenamingProfile(ScanProfile.DisplayName, _displayName.Text);
        }
        _scanProfile = GetUpdatedScanProfile();
        return true;
    }

    private ScanProfile GetUpdatedScanProfile()
    {
        var pageSize = _pageSize.SelectedItem!;
        return new ScanProfile
        {
            Version = ScanProfile.CURRENT_VERSION,

            Device = ScanProfileDevice.FromScanDevice(_deviceSelectorWidget.Choice.Device),
            Caps = ScanProfile.Caps,
            IsDefault = _isDefault,
            DriverName = DeviceDriver.ToString().ToLowerInvariant(),
            DisplayName = _displayName.Text,
            IconID = 0,
            MaxQuality = ScanProfile.MaxQuality,
            UseNativeUI = _nativeUi.Checked,

            AfterScanScale = _scale.SelectedItem,
            BitDepth = _bitDepth.SelectedItem,
            Brightness = _brightnessSlider.IntValue,
            Contrast = _contrastSlider.IntValue,
            PageAlign = _horAlign.SelectedItem,
            PageSize = pageSize.Type,
            CustomPageSizeName = pageSize.CustomName,
            CustomPageSize = pageSize.CustomDimens,
            Resolution = new ScanResolution { Dpi = _resolution.SelectedItem },
            PaperSource = _paperSource.SelectedItem,

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
            _deviceSelectorWidget.Enabled = !deviceLocked;
            _predefinedSettings.Enabled = _nativeUi.Enabled = !locked;
            _nativeUiVis.IsVisible = _deviceSelectorWidget.Choice.Device == null || canUseNativeUi;

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


    private void PaperSource_SelectedItemChanged(object? sender, EventArgs e)
    {
        if (_suppressChangeEvent) return;
        UpdateUiForCaps();
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
        ScanProfile.BitDepth = _bitDepth.SelectedItem;
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
}