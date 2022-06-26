using System.Threading;
using System.Windows.Forms;
using NAPS2.EtoForms.Ui;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Images;
using NAPS2.Platform.Windows;
using NAPS2.Recovery;
using NAPS2.Remoting;
using NAPS2.Scan;
using NAPS2.Update;
using NAPS2.Wia;

namespace NAPS2.WinForms;

public class DesktopController
{
    private readonly ScanningContext _scanningContext;
    private readonly UiImageList _imageList;
    private readonly RecoveryStorageManager _recoveryStorageManager;
    private readonly ThumbnailRenderQueue _thumbnailRenderQueue;
    private readonly OperationProgress _operationProgress;
    private readonly Naps2Config _config;
    private readonly IOperationFactory _operationFactory;
    private readonly StillImage _stillImage;
    private readonly IFormFactory _formFactory;
    private readonly IProfileManager _profileManager;
    private readonly IScanPerformer _scanPerformer;
    private readonly UpdateChecker _updateChecker;
    private readonly NotificationManager _notify;
    private readonly ImageTransfer _imageTransfer;
    private readonly ImageClipboard _imageClipboard;
    private readonly UserActions _userActions;
    private readonly WinFormsExportHelper _exportHelper;

    private bool _closed;

    public DesktopController(ScanningContext scanningContext, UiImageList imageList,
        RecoveryStorageManager recoveryStorageManager, ThumbnailRenderQueue thumbnailRenderQueue,
        OperationProgress operationProgress, Naps2Config config, IOperationFactory operationFactory,
        StillImage stillImage, IFormFactory formFactory, IProfileManager profileManager, IScanPerformer scanPerformer,
        UpdateChecker updateChecker, NotificationManager notify, ImageTransfer imageTransfer,
        ImageClipboard imageClipboard, UserActions userActions, WinFormsExportHelper exportHelper)
    {
        _scanningContext = scanningContext;
        _imageList = imageList;
        _recoveryStorageManager = recoveryStorageManager;
        _thumbnailRenderQueue = thumbnailRenderQueue;
        _operationProgress = operationProgress;
        _config = config;
        _operationFactory = operationFactory;
        _stillImage = stillImage;
        _formFactory = formFactory;
        _profileManager = profileManager;
        _scanPerformer = scanPerformer;
        _updateChecker = updateChecker;
        _notify = notify;
        _imageTransfer = imageTransfer;
        _imageClipboard = imageClipboard;
        _userActions = userActions;
        _exportHelper = exportHelper;
    }

    public Form Form { get; set; }

    public Action<Action> SafeInvoke { get; set; }

    // TODO: Replace this with change events on ProfileManager
    public Action UpdateScanButton { get; set; }

    public bool SkipRecoveryCleanup { get; set; }

    public async Task Initialize()
    {
        StartPipesServer();
        ShowStartupMessages();
        ShowRecoveryPrompt();
        InitThumbnailRendering();
        await RunStillImageEvents();
        SetFirstRunDate();
        ShowDonationPrompt();
        ShowUpdatePrompt();
    }

    private void ShowDonationPrompt()
    {
        // Show a donation prompt after a month of use
#if !INSTALLER_MSI
        if (!_config.Get(c => c.HiddenButtons).HasFlag(ToolbarButtons.Donate) &&
            !_config.Get(c => c.HasBeenPromptedForDonation) &&
            DateTime.Now - _config.Get(c => c.FirstRunDate) > TimeSpan.FromDays(30))
        {
            var transact = _config.User.BeginTransaction();
            transact.Set(c => c.HasBeenPromptedForDonation, true);
            transact.Set(c => c.LastDonatePromptDate, DateTime.Now);
            transact.Commit();
            _notify.DonatePrompt();
        }
#endif
    }

    private void ShowUpdatePrompt()
    {
#if !INSTALLER_MSI
        if (_config.Get(c => c.CheckForUpdates) &&
            (!_config.Get(c => c.HasCheckedForUpdates) ||
             _config.Get(c => c.LastUpdateCheckDate) < DateTime.Now - _updateChecker.CheckInterval))
        {
            _updateChecker.CheckForUpdates().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Log.ErrorException("Error checking for updates", task.Exception);
                }
                else
                {
                    var transact = _config.User.BeginTransaction();
                    transact.Set(c => c.HasCheckedForUpdates, true);
                    transact.Set(c => c.LastUpdateCheckDate, DateTime.Now);
                    transact.Commit();
                }
                var update = task.Result;
                if (update != null)
                {
                    SafeInvoke(() => _notify.UpdateAvailable(_updateChecker, update));
                }
            }).AssertNoAwait();
        }
#endif
    }

    private void SetFirstRunDate()
    {
        if (!_config.Get(c => c.HasBeenRun))
        {
            var transact = _config.User.BeginTransaction();
            transact.Set(c => c.HasBeenRun, true);
            transact.Set(c => c.FirstRunDate, DateTime.Now);
            transact.Commit();
        }
    }

    private async Task RunStillImageEvents()
    {
        // If NAPS2 was started by the scanner button, do the appropriate actions automatically
        if (_stillImage.ShouldScan)
        {
            await ScanWithDevice(_stillImage.DeviceID!);
        }
    }

    public void Cleanup()
    {
        Pipes.KillServer();
        if (!SkipRecoveryCleanup)
        {
            try
            {
                // TODO: Figure out and fix undisposed processed images
                _scanningContext.Dispose();
                _recoveryStorageManager.Dispose();
            }
            catch (Exception ex)
            {
                Log.ErrorException("Recovery cleanup failed", ex);
            }
        }
        _closed = true;
        _thumbnailRenderQueue.Dispose();
    }

    public bool PrepareForClosing(bool userClosing)
    {
        if (_closed) return true;

        if (_operationProgress.ActiveOperations.Any())
        {
            if (userClosing)
            {
                if (_operationProgress.ActiveOperations.Any(x => !x.SkipExitPrompt))
                {
                    var result = MessageBox.Show(MiscResources.ExitWithActiveOperations,
                        MiscResources.ActiveOperations,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                    if (result != DialogResult.Yes)
                    {
                        return false;
                    }
                }
            }
            else
            {
                SkipRecoveryCleanup = true;
            }
        }
        else if (_imageList.Images.Any() && _imageList.SavedState != _imageList.CurrentState)
        {
            if (userClosing && !SkipRecoveryCleanup)
            {
                var result = MessageBox.Show(MiscResources.ExitWithUnsavedChanges, MiscResources.UnsavedChanges,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return false;
                }
                _imageList.SavedState = _imageList.CurrentState;
            }
            else
            {
                SkipRecoveryCleanup = true;
            }
        }

        if (_operationProgress.ActiveOperations.Any())
        {
            _operationProgress.ActiveOperations.ForEach(op => op.Cancel());
            Form.Hide();
            Form.ShowInTaskbar = false;
            Task.Run(() =>
            {
                var timeoutCts = new CancellationTokenSource();
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));
                try
                {
                    _operationProgress.ActiveOperations.ForEach(op => op.Wait(timeoutCts.Token));
                }
                catch (OperationCanceledException)
                {
                }
                _closed = true;
                SafeInvoke(Form.Close);
            });
            return false;
        }

        return true;
    }

    private void StartPipesServer()
    {
        // Receive messages from other processes
        Pipes.StartServer(msg =>
        {
            if (msg.StartsWith(Pipes.MSG_SCAN_WITH_DEVICE, StringComparison.InvariantCulture))
            {
                SafeInvoke(async () => await ScanWithDevice(msg.Substring(Pipes.MSG_SCAN_WITH_DEVICE.Length)));
            }
            if (msg.Equals(Pipes.MSG_ACTIVATE))
            {
                SafeInvoke(() =>
                {
                    var form = Application.OpenForms.Cast<Form>().Last();
                    if (form.WindowState == FormWindowState.Minimized)
                    {
                        Win32.ShowWindow(form.Handle, Win32.ShowWindowCommands.Restore);
                    }
                    form.Activate();
                });
            }
        });
    }

    private void ShowStartupMessages()
    {
        // If configured (e.g. by a business), show a customizable message box on application startup.
        if (!string.IsNullOrWhiteSpace(_config.Get(c => c.StartupMessageText)))
        {
            MessageBox.Show(_config.Get(c => c.StartupMessageText), _config.Get(c => c.StartupMessageTitle),
                MessageBoxButtons.OK,
                _config.Get(c => c.StartupMessageIcon));
        }
    }

    private void ShowRecoveryPrompt()
    {
        // Allow scanned images to be recovered in case of an unexpected close
        var op = _operationFactory.Create<RecoveryOperation>();
        if (op.Start(ReceiveScannedImage(),
                new RecoveryParams { ThumbnailSize = _config.Get(c => c.ThumbnailSize) }))
        {
            _operationProgress.ShowProgress(op);
        }
    }

    private void InitThumbnailRendering()
    {
        _thumbnailRenderQueue.SetThumbnailSize(_config.Get(c => c.ThumbnailSize));
        _thumbnailRenderQueue.StartRendering(_imageList);
    }

    private ScanParams DefaultScanParams() =>
        new ScanParams
        {
            NoAutoSave = _config.Get(c => c.DisableAutoSave),
            DoOcr = _config.Get(c => c.EnableOcr) && _config.Get(c => c.OcrAfterScanning),
            ThumbnailSize = _config.Get(c => c.ThumbnailSize)
        };

    private async Task ScanWithDevice(string deviceID)
    {
        Form.Activate();
        ScanProfile profile;
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
            var editSettingsForm = _formFactory.Create<FEditProfile>();
            editSettingsForm.ScanProfile = _config.Get(c => c.DefaultProfileSettings);
            try
            {
                // Populate the device field automatically (because we can do that!)
                using var deviceManager = new WiaDeviceManager();
                using var device = deviceManager.FindDevice(deviceID);
                editSettingsForm.CurrentDevice = new ScanDevice(deviceID, device.Name());
            }
            catch (WiaException)
            {
            }
            editSettingsForm.ShowDialog();
            if (!editSettingsForm.Result)
            {
                return;
            }
            profile = editSettingsForm.ScanProfile;
            _profileManager.Mutate(new ListMutation<ScanProfile>.Append(profile),
                ListSelection.Empty<ScanProfile>());
            _profileManager.DefaultProfile = profile;

            UpdateScanButton();
        }
        if (profile != null)
        {
            // We got a profile, yay, so we can actually do the scan now
            var source = await _scanPerformer.PerformScan(profile, DefaultScanParams(), Form.Handle);
            await source.ForEach(ReceiveScannedImage());
            Form.Activate();
        }
    }

    public async Task ScanDefault()
    {
        if (_profileManager.DefaultProfile != null)
        {
            var source =
                await _scanPerformer.PerformScan(_profileManager.DefaultProfile, DefaultScanParams(), Form.Handle);
            await source.ForEach(ReceiveScannedImage());
            Form.Activate();
        }
        else if (_profileManager.Profiles.Count == 0)
        {
            await ScanWithNewProfile();
        }
        else
        {
            ShowProfilesForm();
        }
    }

    public async Task ScanWithNewProfile()
    {
        var editSettingsForm = _formFactory.Create<FEditProfile>();
        editSettingsForm.ScanProfile = _config.Get(c => c.DefaultProfileSettings);
        editSettingsForm.ShowDialog();
        if (!editSettingsForm.Result)
        {
            return;
        }
        _profileManager.Mutate(new ListMutation<ScanProfile>.Append(editSettingsForm.ScanProfile),
            ListSelection.Empty<ScanProfile>());
        _profileManager.DefaultProfile = editSettingsForm.ScanProfile;

        UpdateScanButton();

        var source = await _scanPerformer.PerformScan(editSettingsForm.ScanProfile, DefaultScanParams(), Form.Handle);
        await source.ForEach(ReceiveScannedImage());
        Form.Activate();
    }

    /// <summary>
    /// Constructs a receiver for scanned images.
    /// This keeps images from the same source together, even if multiple sources are providing images at the same time.
    /// </summary>
    /// <returns></returns>
    private Action<ProcessedImage> ReceiveScannedImage()
    {
        UiImage? last = null;
        return scannedImage =>
        {
            SafeInvoke(() =>
            {
                lock (_imageList)
                {
                    var uiImage = new UiImage(scannedImage);
                    _imageList.Mutate(new ImageListMutation.InsertAfter(uiImage, last));
                    last = uiImage;
                }
            });
        };
    }

    public void ShowProfilesForm()
    {
        var form = _formFactory.Create<ProfilesForm>();
        form.ImageCallback = ReceiveScannedImage();
        form.ShowModal();
        UpdateScanButton();
    }

    public void ShowBatchScanForm()
    {
        var form = _formFactory.Create<FBatchScan>();
        form.ImageCallback = ReceiveScannedImage();
        form.ShowDialog();
        UpdateScanButton();
    }

    public void ImportFiles(IEnumerable<string> files)
    {
        var op = _operationFactory.Create<ImportOperation>();
        if (op.Start(OrderFiles(files), ReceiveScannedImage(),
                new ImportParams { ThumbnailSize = _config.Get(c => c.ThumbnailSize) }))
        {
            _operationProgress.ShowProgress(op);
        }
    }

    private List<string> OrderFiles(IEnumerable<string> files)
    {
        // Custom ordering to account for numbers so that e.g. "10" comes after "2"
        var filesList = files.ToList();
        filesList.Sort(new NaturalStringComparer());
        return filesList;
    }

    public void ImportDirect(ImageTransferData data, bool copy)
    {
        var op = _operationFactory.Create<DirectImportOperation>();
        if (op.Start(data, copy, ReceiveScannedImage(),
                new DirectImportParams { ThumbnailSize = _config.Get(c => c.ThumbnailSize) }))
        {
            _operationProgress.ShowProgress(op);
        }
    }

    public async Task ScanWithProfile(ScanProfile profile)
    {
        _profileManager.DefaultProfile = profile;

        UpdateScanButton();

        var source = await _scanPerformer.PerformScan(profile, DefaultScanParams(), Form.Handle);
        await source.ForEach(ReceiveScannedImage());
        Form.Activate();
    }

    public void Paste()
    {
        if (_imageTransfer.IsInClipboard())
        {
            ImportDirect(_imageTransfer.GetFromClipboard(), true);
        }
    }

    public async Task Copy()
    {
        using var imagesToCopy = _imageList.Selection.Select(x => x.GetClonedImage()).ToDisposableList();
        await _imageClipboard.Write(imagesToCopy.InnerList, true);
    }


    public void Clear()
    {
        if (_imageList.Images.Count > 0)
        {
            if (MessageBox.Show(string.Format(MiscResources.ConfirmClearItems, _imageList.Images.Count),
                    MiscResources.Clear, MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question) == DialogResult.OK)
            {
                _userActions.DeleteAll();
            }
        }
    }

    public void Delete()
    {
        if (_imageList.Selection.Any())
        {
            if (MessageBox.Show(string.Format(MiscResources.ConfirmDeleteItems, _imageList.Selection.Count),
                    MiscResources.Delete, MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question) == DialogResult.OK)
            {
                _userActions.DeleteSelected();
            }
        }
    }

    public void PreviewImage()
    {
        if (_imageList.Selection.Any())
        {
            using var viewer = _formFactory.Create<FViewer>();
            viewer.ImageList = _imageList;
            // TODO: Fix this 
            // viewer.ImageIndex = SelectedIndices.First();
            // viewer.DeleteCallback = UpdateThumbnails;
            viewer.SelectCallback = i =>
            {
                if (_imageList.Selection.Count <= 1)
                {
                    // TODO: Fix this
                    // SelectedIndices = new[] { i };
                    //thumbnailList1.Items[i].EnsureVisible();
                }
            };
            viewer.ShowDialog();
        }
    }

    public void ResetImage()
    {
        if (_imageList.Selection.Any())
        {
            if (MessageBox.Show(string.Format(MiscResources.ConfirmResetImages, _imageList.Selection.Count),
                    MiscResources.ResetImage,
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                _userActions.ResetTransforms();
            }
        }
    }

    public void OpenAbout()
    {
        _formFactory.Create<AboutForm>().ShowModal();
    }

    public void OpenSettings()
    {
        // FormFactory.Create<FSettings>().ShowDialog();
    }

    public async Task SavePDF(List<UiImage> images)
    {
        using var imagesToSave = images.Select(x => x.GetClonedImage()).ToDisposableList();
        if (await _exportHelper.SavePDF(imagesToSave.InnerList, _notify))
        {
            if (_config.Get(c => c.DeleteAfterSaving))
            {
                SafeInvoke(() =>
                {
                    _imageList.Mutate(new ImageListMutation.DeleteSelected(), ListSelection.From(images));
                });
            }
        }
    }

    public async Task SaveImages(List<UiImage> images)
    {
        using var imagesToSave = images.Select(x => x.GetClonedImage()).ToDisposableList();
        if (await _exportHelper.SaveImages(imagesToSave.InnerList, _notify))
        {
            if (_config.Get(c => c.DeleteAfterSaving))
            {
                _imageList.Mutate(new ImageListMutation.DeleteSelected(), ListSelection.From(images));
            }
        }
    }

    public async Task EmailPDF(List<UiImage> images)
    {
        using var imagesToEmail = images.Select(x => x.GetClonedImage()).ToDisposableList();
        await _exportHelper.EmailPDF(imagesToEmail.InnerList);
    }

    public void Import()
    {
        var ofd = new OpenFileDialog
        {
            Multiselect = true,
            CheckFileExists = true,
            Filter = MiscResources.FileTypeAllFiles + @"|*.*|" +
                     MiscResources.FileTypePdf + @"|*.pdf|" +
                     MiscResources.FileTypeImageFiles +
                     @"|*.bmp;*.emf;*.exif;*.gif;*.jpg;*.jpeg;*.png;*.tiff;*.tif|" +
                     MiscResources.FileTypeBmp + @"|*.bmp|" +
                     MiscResources.FileTypeEmf + @"|*.emf|" +
                     MiscResources.FileTypeExif + @"|*.exif|" +
                     MiscResources.FileTypeGif + @"|*.gif|" +
                     MiscResources.FileTypeJpeg + @"|*.jpg;*.jpeg|" +
                     MiscResources.FileTypePng + @"|*.png|" +
                     MiscResources.FileTypeTiff + @"|*.tiff;*.tif"
        };
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            ImportFiles(ofd.FileNames);
        }
    }
}