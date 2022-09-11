using System.Threading;
using Eto.Forms;
using NAPS2.EtoForms;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Images;
using NAPS2.Platform.Windows;
using NAPS2.Recovery;
using NAPS2.Remoting;
using NAPS2.Scan;
using NAPS2.Update;

namespace NAPS2.WinForms;

// TODO: We undoubtedly want to decompose this file even further.
// We almost certainly want a DesktopScanController for the scanning-related logic.
// We could have a DesktopPipesController that depends on DesktopScanController.
// Specifically each line in Initialize might make sense as a sub-controller.
// We also need to think about how to pass the Form instance around as needed. (e.g. to Activate it). Maybe this should be something injectable, and could also be used by UpdateOperation instead of searching through open forms.
// i.e. (I)DesktopFormProvider
public class DesktopController
{
    private readonly ScanningContext _scanningContext;
    private readonly UiImageList _imageList;
    private readonly RecoveryStorageManager _recoveryStorageManager;
    private readonly ThumbnailController _thumbnailController;
    private readonly OperationProgress _operationProgress;
    private readonly Naps2Config _config;
    private readonly IOperationFactory _operationFactory;
    private readonly StillImage _stillImage;
    private readonly IUpdateChecker _updateChecker;
    private readonly INotificationManager _notify;
    private readonly ImageTransfer _imageTransfer;
    private readonly ImageClipboard _imageClipboard;
    private readonly ImageListActions _imageListActions;
    private readonly IWinFormsExportHelper _exportHelper;
    private readonly DesktopImagesController _desktopImagesController;
    private readonly IDesktopScanController _desktopScanController;
    private readonly DesktopFormProvider _desktopFormProvider;
    private readonly IScannedImagePrinter _scannedImagePrinter;

    private bool _closed;

    public DesktopController(ScanningContext scanningContext, UiImageList imageList,
        RecoveryStorageManager recoveryStorageManager, ThumbnailController thumbnailController,
        OperationProgress operationProgress, Naps2Config config, IOperationFactory operationFactory,
        StillImage stillImage,
        IUpdateChecker updateChecker, INotificationManager notify, ImageTransfer imageTransfer,
        ImageClipboard imageClipboard, ImageListActions imageListActions, IWinFormsExportHelper exportHelper,
        DesktopImagesController desktopImagesController, IDesktopScanController desktopScanController,
        DesktopFormProvider desktopFormProvider, IScannedImagePrinter scannedImagePrinter)
    {
        _scanningContext = scanningContext;
        _imageList = imageList;
        _recoveryStorageManager = recoveryStorageManager;
        _thumbnailController = thumbnailController;
        _operationProgress = operationProgress;
        _config = config;
        _operationFactory = operationFactory;
        _stillImage = stillImage;
        _updateChecker = updateChecker;
        _notify = notify;
        _imageTransfer = imageTransfer;
        _imageClipboard = imageClipboard;
        _imageListActions = imageListActions;
        _exportHelper = exportHelper;
        _desktopImagesController = desktopImagesController;
        _desktopScanController = desktopScanController;
        _desktopFormProvider = desktopFormProvider;
        _scannedImagePrinter = scannedImagePrinter;
    }

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
            !_config.Get(c => c.NoUpdatePrompt) &&
            (!_config.Get(c => c.HasCheckedForUpdates) ||
             _config.Get(c => c.LastUpdateCheckDate) < DateTime.Now - UpdateChecker.CheckInterval))
        {
            _updateChecker.CheckForUpdates().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Log.ErrorException("Error checking for updates", task.Exception!);
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
                    _notify.UpdateAvailable(_updateChecker, update);
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
            await _desktopScanController.ScanWithDevice(_stillImage.DeviceID!);
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
        _thumbnailController.Dispose();
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
                    var result = MessageBox.Show(_desktopFormProvider.DesktopForm,
                        MiscResources.ExitWithActiveOperations,
                        MiscResources.ActiveOperations,
                        MessageBoxButtons.YesNo, MessageBoxType.Warning, MessageBoxDefaultButton.No);
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
                var result = MessageBox.Show(_desktopFormProvider.DesktopForm, MiscResources.ExitWithUnsavedChanges,
                    MiscResources.UnsavedChanges,
                    MessageBoxButtons.YesNo, MessageBoxType.Warning, MessageBoxDefaultButton.No);
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
            _desktopFormProvider.DesktopForm.Visible = false;
            _desktopFormProvider.DesktopForm.ShowInTaskbar = false;
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
                Invoker.Current.SafeInvoke(_desktopFormProvider.DesktopForm.Close);
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
                Invoker.Current.SafeInvoke(async () =>
                    await _desktopScanController.ScanWithDevice(msg.Substring(Pipes.MSG_SCAN_WITH_DEVICE.Length)));
            }
            if (msg.Equals(Pipes.MSG_ACTIVATE))
            {
                Invoker.Current.SafeInvoke(() =>
                {
                    // TODO: xplat
                    var formOnTop = Application.Instance.Windows.Last();
                    if (formOnTop.WindowState == WindowState.Minimized)
                    {
                        Win32.ShowWindow(formOnTop.NativeHandle, Win32.ShowWindowCommands.Restore);
                    }
                    formOnTop.BringToFront();
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
                _config.Get(c => c.StartupMessageIcon).ToEto());
        }
    }

    private void ShowRecoveryPrompt()
    {
        // Allow scanned images to be recovered in case of an unexpected close
        // TODO: Eto implementation
        // var op = _operationFactory.Create<RecoveryOperation>();
        // if (op.Start(_desktopImagesController.ReceiveScannedImage(),
        //         new RecoveryParams { ThumbnailSize = _config.ThumbnailSize() }))
        // {
        //     _operationProgress.ShowProgress(op);
        // }
    }

    private void InitThumbnailRendering()
    {
        _thumbnailController.Init(_imageList);
    }

    public void ImportFiles(IEnumerable<string> files)
    {
        var op = _operationFactory.Create<ImportOperation>();
        if (op.Start(OrderFiles(files), _desktopImagesController.ReceiveScannedImage(),
                new ImportParams { ThumbnailSize = _thumbnailController.RenderSize }))
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
        if (op.Start(data, copy, _desktopImagesController.ReceiveScannedImage(),
                new DirectImportParams { ThumbnailSize = _thumbnailController.RenderSize }))
        {
            _operationProgress.ShowProgress(op);
        }
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
            if (MessageBox.Show(_desktopFormProvider.DesktopForm,
                    string.Format(MiscResources.ConfirmClearItems, _imageList.Images.Count),
                    MiscResources.Clear, MessageBoxButtons.OKCancel,
                    MessageBoxType.Question) == DialogResult.Ok)
            {
                _imageListActions.DeleteAll();
            }
        }
    }

    public void Delete()
    {
        if (_imageList.Selection.Any())
        {
            if (MessageBox.Show(_desktopFormProvider.DesktopForm,
                    string.Format(MiscResources.ConfirmDeleteItems, _imageList.Selection.Count),
                    MiscResources.Delete, MessageBoxButtons.OKCancel,
                    MessageBoxType.Question) == DialogResult.Ok)
            {
                _imageListActions.DeleteSelected();
            }
        }
    }

    public void ResetImage()
    {
        if (_imageList.Selection.Any())
        {
            if (MessageBox.Show(_desktopFormProvider.DesktopForm,
                    string.Format(MiscResources.ConfirmResetImages, _imageList.Selection.Count),
                    MiscResources.ResetImage,
                    MessageBoxButtons.OKCancel, MessageBoxType.Question) == DialogResult.Ok)
            {
                _imageListActions.ResetTransforms();
            }
        }
    }

    public async Task SavePDF(ICollection<UiImage> images)
    {
        using var imagesToSave = images.Select(x => x.GetClonedImage()).ToDisposableList();
        if (await _exportHelper.SavePDF(imagesToSave.InnerList, _notify))
        {
            if (_config.Get(c => c.DeleteAfterSaving))
            {
                _imageList.Mutate(new ImageListMutation.DeleteSelected(), ListSelection.From(images));
            }
        }
    }

    public async Task SaveImages(ICollection<UiImage> images)
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

    public async Task EmailPDF(ICollection<UiImage> images)
    {
        using var imagesToEmail = images.Select(x => x.GetClonedImage()).ToDisposableList();
        await _exportHelper.EmailPDF(imagesToEmail.InnerList);
    }

    public async Task Print()
    {
        var state = _imageList.CurrentState;
        using var allImages = _imageList.Images.Select(x => x.GetClonedImage()).ToDisposableList();
        using var selectedImages = _imageList.Selection.Select(x => x.GetClonedImage()).ToDisposableList();
        if (await _scannedImagePrinter.PromptToPrint(allImages.InnerList, selectedImages.InnerList))
        {
            _imageList.SavedState = state;
        }
    }

    public void Import(Control parentForm)
    {
        // TODO: Merge this into exporthelper/dialoghelper?
        var ofd = new OpenFileDialog
        {
            MultiSelect = true,
            CheckFileExists = true,
            Filters =
            {
                // TODO: Move filter logic somewhere common
                new FileFilter(MiscResources.FileTypeAllFiles, ".*"),
                new FileFilter(MiscResources.FileTypePdf, ".pdf"),
                new FileFilter(MiscResources.FileTypeImageFiles,
                    ".bmp", ".emf", ".exif", ".gif", "jpg", ".jpeg", ".png", ".tiff", ".tif"),
                new FileFilter(MiscResources.FileTypeBmp, ".bmp"),
                new FileFilter(MiscResources.FileTypeEmf, ".emf"),
                new FileFilter(MiscResources.FileTypeExif, ".exif"),
                new FileFilter(MiscResources.FileTypeGif, ".gif"),
                new FileFilter(MiscResources.FileTypeJpeg, ".jpg", ".jpeg"),
                new FileFilter(MiscResources.FileTypePng, ".png"),
                new FileFilter(MiscResources.FileTypeTiff, ".tiff", ".tif"),
            }
        };
        if (Paths.IsTestAppDataPath)
        {
            // For UI test automation we choose the appdata folder to find the prepared files to import
            ofd.Directory = new Uri(Path.GetFullPath(Paths.AppData));
        }
        if (ofd.ShowDialog(parentForm) == DialogResult.Ok)
        {
            ImportFiles(ofd.Filenames);
        }
    }
}