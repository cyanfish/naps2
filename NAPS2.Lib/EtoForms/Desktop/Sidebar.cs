using System.Globalization;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Ui;
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
    private readonly IFormFactory _formFactory;
    private readonly IDesktopScanController _desktopScanController;

    private readonly LayoutVisibility _sidebarVis = new(true);
    private readonly DropDownWidget<ScanProfile> _profile = new();
    private readonly EnumDropDownWidget<ScanSource> _paperSource = new();
    private readonly DropDownWidget<int> _resolution = new();
    private readonly EnumDropDownWidget<ScanBitDepth> _bitDepth = new();

    private DeviceSelectorWidget? _deviceSelectorWidget;
    private PageSizeDropDownWidget? _pageSize;

    public Sidebar(IScanPerformer scanPerformer, DeviceCapsCache deviceCapsCache, IProfileManager profileManager,
        Naps2Config config, IIconProvider iconProvider, IFormFactory formFactory,
        IDesktopScanController desktopScanController)
    {
        _scanPerformer = scanPerformer;
        _deviceCapsCache = deviceCapsCache;
        _profileManager = profileManager;
        _config = config;
        _iconProvider = iconProvider;
        _formFactory = formFactory;
        _desktopScanController = desktopScanController;
        _profile.Format = x => x.DisplayName;
        _profileManager.ProfilesUpdated += (_, _) => UpdateProfilesDropdown();
        UpdateProfilesDropdown();

        EditProfileCommand = new ActionCommand(EditProfile) { IconName = "pencil_small" };
        NewProfileCommand = new ActionCommand(NewProfile) { IconName = "add_small" };
    }

    private void UpdateProfilesDropdown() => _profile.Items = _profileManager.Profiles;

    private void EditProfile()
    {
        var originalProfile = _profile.SelectedItem;
        if (originalProfile != null)
        {
            var fedit = _formFactory.Create<EditProfileForm>();
            fedit.ScanProfile = originalProfile;
            fedit.ShowModal();
            if (fedit.Result)
            {
                _profile.SelectedItem = fedit.ScanProfile;
                _profileManager.Mutate(new ListMutation<ScanProfile>.ReplaceWith(fedit.ScanProfile),
                    ListSelection.Of(originalProfile));
            }
        }
    }

    private void NewProfile()
    {
        if (!(_config.Get(c => c.NoUserProfiles) && _profileManager.Profiles.Any(x => x.IsLocked)))
        {
            var fedit = _formFactory.Create<EditProfileForm>();
            fedit.NewProfile = true;
            fedit.ScanProfile = _config.DefaultProfileSettings();
            fedit.ShowModal();
            if (fedit.Result)
            {
                _profile.SelectedItem = fedit.ScanProfile;
                _profileManager.Mutate(new ListMutation<ScanProfile>.Append(fedit.ScanProfile),
                    ListSelection.Empty<ScanProfile>());
            }
        }
    }

    private ActionCommand NewProfileCommand { get; }

    private ActionCommand EditProfileCommand { get; }

    public LayoutElement CreateView(IFormBase parentWindow)
    {
        _sidebarVis.IsVisible = _config.Get(c => c.SidebarVisible);
        _profile.SelectedItem = _profileManager.DefaultProfile;

        _deviceSelectorWidget = new DeviceSelectorWidget(_scanPerformer, _deviceCapsCache, _iconProvider, parentWindow)
        {
            ShowChooseDevice = false,
            ProfileFunc = () => _profile.SelectedItem!
        };
        _pageSize = new PageSizeDropDownWidget(parentWindow);
        _profile.SelectedItemChanged += (_, _) =>
        {
            if (_config.Get(c => c.ScanChangesDefaultProfile))
            {
                _profileManager.DefaultProfile = _profile.SelectedItem;
            }
            UpdateUiForProfile();
        };

        UpdateUiForProfile();

        return L.Column(
            C.Filler(),
            L.Row(
                C.Label(UiStrings.ProfileLabel).Scale().AlignTrailing(),
                C.Button(EditProfileCommand, ButtonImagePosition.Overlay).Width(30),
                C.Button(NewProfileCommand, ButtonImagePosition.Overlay).Width(30)
            ),
            _profile.AsControl().NaturalWidth(100),
            C.Spacer(),
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
            C.Button(new ActionCommand(DoScan)
            {
                MenuText = UiStrings.Scan,
                IconName = "control_play_blue_small"
            }, ButtonImagePosition.Left).AlignCenter().Height(30),
            C.Filler()
        ).Padding(left: parentWindow.LayoutController.DefaultSpacing + 10, right: 10).Visible(_sidebarVis);
    }

    private void DoScan()
    {
        var profile = _profile.SelectedItem!;
        var pageSize = _pageSize!.SelectedItem!;
        
        profile.PaperSource = _paperSource.SelectedItem;
        profile.PageSize = pageSize.Type;
        profile.CustomPageSizeName = pageSize.CustomName;
        profile.CustomPageSize = pageSize.CustomDimens;
        profile.Resolution = new ScanResolution { Dpi = _resolution.SelectedItem };
        profile.BitDepth = _bitDepth.SelectedItem;
        _profileManager.Save();
        
        _desktopScanController.ScanWithProfile(profile);
    }

    private void UpdateUiForProfile()
    {
        var profile = _profile.SelectedItem;
        if (profile == null) return;

        var deviceDriver = new ScanOptionsValidator().ValidateDriver(
            Enum.TryParse<Driver>(profile.DriverName, true, out var driver)
                ? driver
                : Driver.Default);

        var device = profile.Device?.ToScanDevice(deviceDriver);
        if (device != null)
        {
            _deviceSelectorWidget!.Choice = DeviceChoice.ForDevice(device);
        }
        else
        {
            _deviceSelectorWidget!.Choice = DeviceChoice.ForAlwaysAsk(deviceDriver);
        }

        if (profile.PageSize == ScanPageSize.Custom && profile.CustomPageSize != null)
        {
            _pageSize!.SetCustom(profile.CustomPageSizeName, profile.CustomPageSize);
        }
        else
        {
            _pageSize!.SetPreset(profile.PageSize);
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
    }

    public void ToggleVisibility()
    {
        _sidebarVis.IsVisible = !_sidebarVis.IsVisible;
        _config.User.Set(c => c.SidebarVisible, _sidebarVis.IsVisible);
    }
}