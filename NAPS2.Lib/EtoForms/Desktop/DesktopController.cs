using System.Threading;
using Eto.Drawing;
using Eto.Forms;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NAPS2.EtoForms.Notifications;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Images;
using NAPS2.Platform.Windows;
using NAPS2.Recovery;
using NAPS2.Remoting;
using NAPS2.Remoting.Server;
using NAPS2.Scan;
using NAPS2.Update;

namespace NAPS2.EtoForms.Desktop;

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
    private readonly INotify _notify;
    private readonly ImageClipboard _imageClipboard;
    private readonly ImageListActions _imageListActions;
    private readonly DialogHelper _dialogHelper;
    private readonly DesktopImagesController _desktopImagesController;
    private readonly IDesktopScanController _desktopScanController;
    private readonly DesktopFormProvider _desktopFormProvider;
    private readonly IScannedImagePrinter _scannedImagePrinter;
    private readonly ISharedDeviceManager _sharedDeviceManager;
    private readonly ProcessCoordinator _processCoordinator;
    private readonly RecoveryManager _recoveryManager;
    private readonly ImageTransfer _imageTransfer = new();

    private bool _closed;
    private bool _preInitialized;
    private bool _initialized;
    private bool _suspended;

    public DesktopController(ScanningContext scanningContext, UiImageList imageList,
        RecoveryStorageManager recoveryStorageManager, ThumbnailController thumbnailController,
        OperationProgress operationProgress, Naps2Config config, IOperationFactory operationFactory,
        StillImage stillImage,
        IUpdateChecker updateChecker, INotify notify,
        ImageClipboard imageClipboard, ImageListActions imageListActions,
        DialogHelper dialogHelper,
        DesktopImagesController desktopImagesController, IDesktopScanController desktopScanController,
        DesktopFormProvider desktopFormProvider, IScannedImagePrinter scannedImagePrinter,
        ISharedDeviceManager sharedDeviceManager, ProcessCoordinator processCoordinator,
        RecoveryManager recoveryManager)
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
        _imageClipboard = imageClipboard;
        _imageListActions = imageListActions;
        _dialogHelper = dialogHelper;
        _desktopImagesController = desktopImagesController;
        _desktopScanController = desktopScanController;
        _desktopFormProvider = desktopFormProvider;
        _scannedImagePrinter = scannedImagePrinter;
        _sharedDeviceManager = sharedDeviceManager;
        _processCoordinator = processCoordinator;
        _recoveryManager = recoveryManager;
    }

    public bool SkipRecoveryCleanup { get; set; }

    public void PreInitialize()
    {
        if (_preInitialized) return;
        _preInitialized = true;
        RestoreSession();
    }

    public async Task Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        _sharedDeviceManager.StartSharing();
        StartProcessCoordinator();
        ShowStartupMessages();
        ShowRecoveryPrompt();
        ImportFilesFromCommandLine();
        InitThumbnailRendering();
        await RunStillImageEvents();
        SetFirstRunDate();
        ShowDonationOrReviewPrompt();
        ShowUpdatePrompt();
    }

    public UiImageList GetImages()
    {
        return _imageList;
    }

    private void ShowDonationOrReviewPrompt()
    {
        // Show a review prompt after a month of using the Microsoft Store msix version
#if MSI
        if (WindowsEnvironment.IsRunningAsMsix &&
            !_config.Get(c => c.HasBeenPromptedForReview) &&
            DateTime.Now - _config.Get(c => c.FirstRunDate) > TimeSpan.FromDays(30))
        {
            var transact = _config.User.BeginTransaction();
            transact.Set(c => c.HasBeenPromptedForReview, true);
            transact.Set(c => c.LastReviewPromptDate, DateTime.Now);
            transact.Commit();
            _notify.ReviewPrompt();
        }
#endif
        // Show a donation prompt after a month of use
#if !MSI
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
#if !MSI
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
        if (_suspended) return;
        _processCoordinator.KillServer();
        _sharedDeviceManager.StopSharing();
        if (!SkipRecoveryCleanup && !_config.Get(c => c.KeepSession))
        {
            try
            {
                _scanningContext.Dispose();
                _recoveryStorageManager.Dispose();
                _imageList.Images.DisposeAll();
            }
            catch (Exception ex)
            {
                Log.ErrorException("Recovery cleanup failed", ex);
            }
        }
        _closed = true;
        _thumbnailController.Dispose();
        _scanningContext.WorkerFactory!.StopSpareWorkers();
    }

    public bool PrepareForClosing(bool userClosing)
    {
        if (_suspended || _closed) return true;

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
        else if (_imageList.Images.Any() && _imageList.HasUnsavedChanges)
        {
            if (userClosing && !SkipRecoveryCleanup && !_config.Get(c => c.KeepSession))
            {
                var result = MessageBox.Show(_desktopFormProvider.DesktopForm, MiscResources.ExitWithUnsavedChanges,
                    MiscResources.UnsavedChanges,
                    MessageBoxButtons.YesNo, MessageBoxType.Warning, MessageBoxDefaultButton.No);
                if (result != DialogResult.Yes)
                {
                    return false;
                }
                _imageList.MarkAllSaved();
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
                Invoker.Current.Invoke(_desktopFormProvider.DesktopForm.Close);
            });
            return false;
        }

        return true;
    }

    private void StartProcessCoordinator()
    {
        // Receive messages from other NAPS2 processes
        _processCoordinator.StartServer(new ProcessCoordinatorServiceImpl(this));
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

    private void RestoreSession()
    {
        // In case the user has the "Keep images across sessions" option, this is similar to the RecoveryOperation in
        // ShowRecoveryPrompt, but designed to be faster and more seamless. This means a few things:
        // - Image files are moved instead of copied. This is destructive (higher risk of data loss) but fast.
        // - Thumbnail rendering is deferred.
        // - No operation progress is displayed and the recovery happens synchronously during OnLoad instead of
        // asynchronously after OnShown.
        // - Images are sent back to the UI as a single batch.
        if (!_config.Get(c => c.KeepSession))
        {
            return;
        }
        using var recoverableFolder = _recoveryManager.GetLatestRecoverableFolder();
        if (recoverableFolder != null)
        {
            _desktopImagesController.AppendImageBatch(recoverableFolder.FastRecover());
        }
    }

    private void ShowRecoveryPrompt()
    {
        if (_config.Get(c => c.KeepSession))
        {
            return;
        }
        // Allow scanned images to be recovered in case of an unexpected close
        var op = _operationFactory.Create<RecoveryOperation>();
        var recoveryParams = new RecoveryParams
        {
            ThumbnailSize = _thumbnailController.RenderSize
        };
        if (op.Start(_desktopImagesController.ReceiveScannedImage(), recoveryParams))
        {
            _operationProgress.ShowProgress(op);
        }
    }

    private void ImportFilesFromCommandLine()
    {
        if (Environment.GetCommandLineArgs() is [_, var arg] && File.Exists(arg))
        {
            ImportFiles([arg]);
        }
    }

    private void InitThumbnailRendering()
    {
        _thumbnailController.Init(_imageList);
    }

    public void ImportFiles(ICollection<string> files, bool background = false)
    {
        var op = _operationFactory.Create<ImportOperation>();
        if (op.Start(OrderFiles(files), _desktopImagesController.ReceiveScannedImage(),
                new ImportParams { ThumbnailSize = _thumbnailController.RenderSize }))
        {
            if (background)
            {
                _operationProgress.ShowBackgroundProgress(op);
            }
            else
            {
                _operationProgress.ShowProgress(op);
            }
        }
    }

    private List<string> OrderFiles(IEnumerable<string> files)
    {
        // Custom ordering to account for numbers so that e.g. "10" comes after "2"
        var filesList = files.ToList();
        filesList.Sort(new NaturalStringComparer());
        return filesList;
    }

    internal void ImportDirect(ImageTransferData data, bool copy)
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
        else if (Clipboard.Instance.ContainsImage)
        {
            var etoBitmap = (Bitmap) Clipboard.Instance.Image;
            Task.Run(() =>
            {
                var image = EtoPlatform.Current.FromBitmap(etoBitmap);
                var processedImage = _scanningContext.CreateProcessedImage(image);
                processedImage = ImportPostProcessor.AddPostProcessingData(processedImage, image,
                    _thumbnailController.RenderSize, new BarcodeDetectionOptions(), true);
                _desktopImagesController.ReceiveScannedImage()(processedImage);
            });
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
                    MessageBoxType.Question, MessageBoxDefaultButton.OK) == DialogResult.Ok)
            {
                _imageListActions.DeleteAll();
                GC.Collect();
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
                    MessageBoxType.Question, MessageBoxDefaultButton.OK) == DialogResult.Ok)
            {
                _imageListActions.DeleteSelected();
                GC.Collect();
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
                    MessageBoxButtons.OKCancel, MessageBoxType.Question, MessageBoxDefaultButton.OK) == DialogResult.Ok)
            {
                _imageListActions.ResetTransforms();
            }
        }
    }

    public async Task SavePdf()
    {
        var action = _config.Get(c => c.SaveButtonDefaultAction);

        if (action == SaveButtonDefaultAction.AlwaysPrompt
            || action == SaveButtonDefaultAction.PromptIfSelected && _imageList.Selection.Any())
        {
            _desktopFormProvider.DesktopForm.ShowToolbarMenu(DesktopToolbarMenuType.SavePdf);
        }
        else if (action == SaveButtonDefaultAction.SaveSelected && _imageList.Selection.Any())
        {
            await _imageListActions.SaveSelectedAsPdf();
        }
        else
        {
            await _imageListActions.SaveAllAsPdf();
        }
    }

    public async Task SaveImages()
    {
        var action = _config.Get(c => c.SaveButtonDefaultAction);

        if (action == SaveButtonDefaultAction.AlwaysPrompt
            || action == SaveButtonDefaultAction.PromptIfSelected && _imageList.Selection.Any())
        {
            _desktopFormProvider.DesktopForm.ShowToolbarMenu(DesktopToolbarMenuType.SaveImages);
        }
        else if (action == SaveButtonDefaultAction.SaveSelected && _imageList.Selection.Any())
        {
            await _imageListActions.SaveSelectedAsImages();
        }
        else
        {
            await _imageListActions.SaveAllAsImages();
        }
    }

    public async Task EmailPdf()
    {
        var action = _config.Get(c => c.SaveButtonDefaultAction);

        if (action == SaveButtonDefaultAction.AlwaysPrompt
            || action == SaveButtonDefaultAction.PromptIfSelected && _imageList.Selection.Any())
        {
            _desktopFormProvider.DesktopForm.ShowToolbarMenu(DesktopToolbarMenuType.EmailPdf);
        }
        else if (action == SaveButtonDefaultAction.SaveSelected && _imageList.Selection.Any())
        {
            await _imageListActions.EmailSelectedAsPdf();
        }
        else
        {
            await _imageListActions.EmailAllAsPdf();
        }
    }

    public async Task Print()
    {
        var state = _imageList.CurrentState;
        using var allImages = _imageList.Images.Select(x => x.GetClonedImage()).ToDisposableList();
        using var selectedImages = _imageList.Selection.Select(x => x.GetClonedImage()).ToDisposableList();
        if (await _scannedImagePrinter.PromptToPrint(
                _desktopFormProvider.DesktopForm, allImages.InnerList, selectedImages.InnerList))
        {
            // Ideally we would know the exact images saved but it's not a big deal to get it wrong for printing which
            // is pretty uncommon.
            _imageList.MarkSaved(state, allImages);
        }
    }

    public void Import()
    {
        if (_dialogHelper.PromptToImport(out var fileNames))
        {
            ImportFiles(fileNames!);
        }
    }

    public void Suspend()
    {
        _suspended = true;
    }

    public void Resume()
    {
        _suspended = false;
    }

    private class ProcessCoordinatorServiceImpl(DesktopController controller)
        : ProcessCoordinatorService.ProcessCoordinatorServiceBase
    {
        public override Task<Empty> Activate(ActivateRequest request, ServerCallContext context)
        {
            Invoker.Current.InvokeDispatch(() =>
            {
                var formOnTop = Application.Instance.Windows.Last();
                if (PlatformCompat.System.CanUseWin32)
                {
                    if (formOnTop.WindowState == WindowState.Minimized)
                    {
                        Win32.ShowWindow(formOnTop.NativeHandle, Win32.ShowWindowCommands.Restore);
                    }
                    Win32.SetForegroundWindow(formOnTop.NativeHandle);
                }
                else
                {
                    formOnTop.BringToFront();
                }
            });
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> CloseWindow(CloseWindowRequest request, ServerCallContext context)
        {
            Invoker.Current.InvokeDispatch(() =>
            {
                controller._desktopFormProvider.DesktopForm.Close();
#if NET6_0_OR_GREATER
                if (OperatingSystem.IsMacOS())
                {
                    // Closing the main window isn't enough to quit the app on Mac
                    Application.Instance.Quit();
                }
#endif
            });
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> OpenFile(OpenFileRequest request, ServerCallContext context)
        {
            Invoker.Current.InvokeDispatch(() => controller.ImportFiles(request.Path, true));
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> ScanWithDevice(ScanWithDeviceRequest request, ServerCallContext context)
        {
            Invoker.Current.InvokeDispatch(async () =>
                await controller._desktopScanController.ScanWithDevice(request.Device));
            return Task.FromResult(new Empty());
        }

        public override Task<StopSharingServerResponse> StopSharingServer(StopSharingServerRequest request,
            ServerCallContext context)
        {
            controller._sharedDeviceManager.InvokeSharingServerStopped();
            return Task.FromResult(new StopSharingServerResponse());
        }
    }
}