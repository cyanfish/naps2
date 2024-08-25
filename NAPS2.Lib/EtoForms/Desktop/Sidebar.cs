using System.Globalization;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.Scan;
using NAPS2.Scan.Internal;

namespace NAPS2.EtoForms.Desktop;

public class Sidebar
{
    private readonly IScanPerformer _scanPerformer;
    private readonly DeviceCapsCache _deviceCapsCache;
    private readonly IProfileManager _profileManager;
    private readonly Naps2Config _config;
    private readonly IIconProvider _iconProvider;

    private readonly LayoutVisibility _sidebarVis = new(true);
    private readonly EnumDropDownWidget<ScanSource> _paperSource = new();
    private readonly DropDownWidget<int> _resolution = new();
    private readonly EnumDropDownWidget<ScanBitDepth> _bitDepth = new();

    private DeviceSelectorWidget? _deviceSelectorWidget;
    private PageSizeDropDownWidget? _pageSize;

    public Sidebar(IScanPerformer scanPerformer, DeviceCapsCache deviceCapsCache, IProfileManager profileManager,
        Naps2Config config, IIconProvider iconProvider)
    {
        _scanPerformer = scanPerformer;
        _deviceCapsCache = deviceCapsCache;
        _profileManager = profileManager;
        _config = config;
        _iconProvider = iconProvider;
    }

    public LayoutElement CreateView(IFormBase parentWindow)
    {
        var profile = _profileManager.Profiles.First();

        _deviceSelectorWidget = new DeviceSelectorWidget(_scanPerformer, _deviceCapsCache, _iconProvider, parentWindow)
        {
            ShowChooseDevice = false,
            ProfileFunc = () => profile
        };
        _pageSize = new PageSizeDropDownWidget(parentWindow);

        var deviceDriver = new ScanOptionsValidator().ValidateDriver(
            Enum.TryParse<Driver>(profile.DriverName, true, out var driver)
                ? driver
                : Driver.Default);

        // _displayName.Text = ScanProfile.DisplayName;
        if (_deviceSelectorWidget.Choice == DeviceChoice.None)
        {
            var device = profile.Device?.ToScanDevice(deviceDriver);
            if (device != null)
            {
                _deviceSelectorWidget.Choice = DeviceChoice.ForDevice(device);
            }
            else
            {
                _deviceSelectorWidget.Choice = DeviceChoice.ForAlwaysAsk(deviceDriver);
            }
        }

        if (profile.PageSize == ScanPageSize.Custom && profile.CustomPageSize != null)
        {
            _pageSize.SetCustom(profile.CustomPageSizeName, profile.CustomPageSize);
        }
        else
        {
            _pageSize.SetPreset(profile.PageSize);
        }

        _paperSource.SelectedItem = profile.PaperSource;
        _bitDepth.SelectedItem = profile.BitDepth;
        _resolution.SelectedItem = profile.Resolution.Dpi;
        _resolution.Format = dpi =>
            string.Format(SettingsResources.DpiFormat, dpi.ToString(CultureInfo.InvariantCulture));

        _paperSource.Items = profile.Caps?.PaperSources?.Values is [_, ..] paperSources
            ? paperSources
            : EnumDropDownWidget<ScanSource>.DefaultItems;

        var selectedSource = _paperSource.SelectedItem;
        var perSource = selectedSource switch
        {
            ScanSource.Glass => profile.Caps?.Glass,
            ScanSource.Feeder => profile.Caps?.Feeder,
            ScanSource.Duplex => profile.Caps?.Duplex,
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

        return L.Column(
            C.Filler(),
            _deviceSelectorWidget,
            C.Spacer(),
            C.Label(UiStrings.PaperSourceLabel),
            _paperSource,
            C.Label(UiStrings.PageSizeLabel),
            _pageSize,
            C.Label(UiStrings.ResolutionLabel),
            _resolution,
            C.Label(UiStrings.BitDepthLabel),
            _bitDepth,
            C.Spacer(),
            C.Button(new ActionCommand(() => { })
            {
                MenuText = UiStrings.Scan,
                Image = _iconProvider.GetIcon("control_play_blue_small")
            }, ButtonImagePosition.Left).AlignCenter().Height(30),
            C.Filler()
        ).Padding(left: parentWindow.LayoutController.DefaultSpacing + 10, right: 10).Visible(_sidebarVis);
    }

    public void ToggleVisibility()
    {
        _sidebarVis.IsVisible = !_sidebarVis.IsVisible;
    }
}