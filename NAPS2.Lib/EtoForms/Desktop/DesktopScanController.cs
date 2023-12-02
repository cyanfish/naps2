using NAPS2.EtoForms.Ui;
using NAPS2.Scan;
#if !MAC
using NAPS2.Wia;
#endif

namespace NAPS2.EtoForms.Desktop;

public class DesktopScanController : IDesktopScanController
{
    private readonly Naps2Config _config;
    private readonly IProfileManager _profileManager;
    private readonly IFormFactory _formFactory;
    private readonly IScanPerformer _scanPerformer;
    private readonly DesktopImagesController _desktopImagesController;
    private readonly IDesktopSubFormController _desktopSubFormController;
    private readonly DesktopFormProvider _desktopFormProvider;
    private readonly ThumbnailController _thumbnailController;

    public DesktopScanController(Naps2Config config, IProfileManager profileManager, IFormFactory formFactory,
        IScanPerformer scanPerformer, DesktopImagesController desktopImagesController,
        IDesktopSubFormController desktopSubFormController, DesktopFormProvider desktopFormProvider,
        ThumbnailController thumbnailController)
    {
        _config = config;
        _profileManager = profileManager;
        _formFactory = formFactory;
        _scanPerformer = scanPerformer;
        _desktopImagesController = desktopImagesController;
        _desktopSubFormController = desktopSubFormController;
        _desktopFormProvider = desktopFormProvider;
        _thumbnailController = thumbnailController;
    }

    private ScanParams DefaultScanParams() =>
        new()
        {
            NoAutoSave = _config.Get(c => c.DisableAutoSave),
            OcrParams = _config.OcrAfterScanningParams(),
            ThumbnailSize = _thumbnailController.RenderSize
        };

    public async Task ScanWithDevice(string deviceID)
    {
        _desktopFormProvider.DesktopForm.BringToFront();
        ScanProfile? profile;
        if (_profileManager.DefaultProfile?.Device?.ID == deviceID)
        {
            // Try to use the default profile if it has the right device
            profile = _profileManager.DefaultProfile;
        }
        else
        {
            // Otherwise just pick any old profile with the right device
            // Not sure if this is the best way to do it, but it's hard to prioritize profiles
            profile = _profileManager.Profiles.FirstOrDefault(x => x.Device != null && x.Device.ID == deviceID);
        }
        if (profile == null)
        {
            if (_config.Get(c => c.NoUserProfiles) && _profileManager.Profiles.Any(x => x.IsLocked))
            {
                return;
            }

            // No profile for the device we're scanning with, so prompt to create one
            var editSettingsForm = _formFactory.Create<EditProfileForm>();
            editSettingsForm.ScanProfile = _config.DefaultProfileSettings();
#if !MAC
#if NET6_0_OR_GREATER
            if (OperatingSystem.IsWindows())
            {
#endif
                try
                {
                    // Populate the device field automatically (because we can do that!)
                    using var deviceManager = new WiaDeviceManager();
                    using var device = deviceManager.FindDevice(deviceID);
                    editSettingsForm.CurrentDevice = new ScanDevice(Driver.Wia, deviceID, device.Name());
                }
                catch (WiaException)
                {
                }
#if NET6_0_OR_GREATER
            }
#endif
#endif
            editSettingsForm.ShowModal();
            if (!editSettingsForm.Result)
            {
                return;
            }
            profile = editSettingsForm.ScanProfile;
            _profileManager.Mutate(new ListMutation<ScanProfile>.Append(profile),
                ListSelection.Empty<ScanProfile>());
            _profileManager.DefaultProfile = profile;
        }

        // We got a profile, yay, so we can actually do the scan now
        await DoScan(profile);
    }

    public async Task ScanDefault()
    {
        if (_profileManager.DefaultProfile != null)
        {
            await DoScan(_profileManager.DefaultProfile);
        }
        else if (_profileManager.Profiles.Count == 0)
        {
            await ScanWithNewProfile();
        }
        else
        {
            _desktopSubFormController.ShowProfilesForm();
        }
    }

    public async Task ScanWithNewProfile()
    {
        var editSettingsForm = _formFactory.Create<EditProfileForm>();
        editSettingsForm.ScanProfile = _config.DefaultProfileSettings();
        editSettingsForm.ShowModal();
        if (!editSettingsForm.Result)
        {
            return;
        }
        _profileManager.Mutate(new ListMutation<ScanProfile>.Append(editSettingsForm.ScanProfile),
            ListSelection.Empty<ScanProfile>());
        _profileManager.DefaultProfile = editSettingsForm.ScanProfile;

        await DoScan(editSettingsForm.ScanProfile);
    }

    public async Task ScanWithProfile(ScanProfile profile)
    {
        _profileManager.DefaultProfile = profile;
        await DoScan(profile);
    }

    private async Task DoScan(ScanProfile profile)
    {
        var images =
            _scanPerformer.PerformScan(profile, DefaultScanParams(), _desktopFormProvider.DesktopForm.NativeHandle);
        var imageCallback = _desktopImagesController.ReceiveScannedImage();
        await foreach (var image in images)
        {
            imageCallback(image);
        }
        _desktopFormProvider.DesktopForm.BringToFront();
    }
}