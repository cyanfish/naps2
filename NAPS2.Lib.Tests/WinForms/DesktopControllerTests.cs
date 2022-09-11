using Moq;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Images;
using NAPS2.Platform.Windows;
using NAPS2.Recovery;
using NAPS2.Sdk.Tests;
using NAPS2.Sdk.Tests.Asserts;
using NAPS2.Update;
using NAPS2.WinForms;
using Xunit;

namespace NAPS2.Lib.Tests.WinForms;

public class DesktopControllerTests : ContextualTests
{
    // TODO: We should create individual unit tests for everything that's mocked here.
    // TODO: We should also make some things more testable:
    // - Change DesktopFormProvider to e.g. DesktopFormActivator or something else with a mockable interface
    private readonly DesktopController _desktopController;
    private readonly UiImageList _imageList;
    private readonly RecoveryStorageManager _recoveryStorageManager;
    private readonly ThumbnailRenderQueue _thumbnailRenderQueue;
    private readonly Mock<OperationProgress> _operationProgress;
    private readonly Naps2Config _config;
    private readonly Mock<IOperationFactory> _operationFactory;
    private readonly StillImage _stillImage;
    private readonly Mock<IUpdateChecker> _updateChecker;
    private readonly Mock<INotificationManager> _notifcationManager;
    private readonly ImageTransfer _imageTransfer;
    private readonly ImageClipboard _imageClipboard;
    private readonly Mock<IWinFormsExportHelper> _exportHelper;
    private readonly DesktopImagesController _desktopImagesController;
    private readonly Mock<IDesktopScanController> _desktopScanController;
    private readonly DesktopFormProvider _desktopFormProvider;
    private readonly Mock<IScannedImagePrinter> _scannedImagePrinter;
    private readonly ThumbnailController _thumbnailController;

    public DesktopControllerTests()
    {
        ScanningContext.RecoveryPath = Path.Combine(FolderPath, "recovery");
        ScanningContext.FileStorageManager = new FileStorageManager(ScanningContext.RecoveryPath);
        _imageList = new UiImageList();
        _recoveryStorageManager = RecoveryStorageManager.CreateFolder(ScanningContext.RecoveryPath, _imageList);
        _thumbnailRenderQueue = new ThumbnailRenderQueue(ScanningContext, new ThumbnailRenderer(ImageContext));
        _operationProgress = new Mock<OperationProgress>();
        _config = Naps2Config.Stub();
        _operationFactory = new Mock<IOperationFactory>();
        _stillImage = new StillImage();
        _updateChecker = new Mock<IUpdateChecker>();
        _notifcationManager = new Mock<INotificationManager>();
        _imageTransfer = new ImageTransfer();
        _imageClipboard = new ImageClipboard();
        _exportHelper = new Mock<IWinFormsExportHelper>();
        _desktopImagesController = new DesktopImagesController(_imageList);
        _desktopScanController = new Mock<IDesktopScanController>();
        _desktopFormProvider = new DesktopFormProvider();
        _scannedImagePrinter = new Mock<IScannedImagePrinter>();
        _thumbnailController = new ThumbnailController(_thumbnailRenderQueue, _config);
        _desktopController = new DesktopController(
            ScanningContext,
            _imageList,
            _recoveryStorageManager,
            _thumbnailController,
            _operationProgress.Object,
            _config,
            _operationFactory.Object,
            _stillImage,
            _updateChecker.Object,
            _notifcationManager.Object,
            _imageTransfer,
            _imageClipboard,
            new ImageListActions(_imageList, _operationFactory.Object, _operationProgress.Object,
                _config, _thumbnailController),
            _exportHelper.Object,
            _desktopImagesController,
            _desktopScanController.Object,
            _desktopFormProvider,
            _scannedImagePrinter.Object
        );

        // TODO: Enable for eto
        // _operationFactory.Setup(x => x.Create<RecoveryOperation>()).Returns(
        //     new RecoveryOperation(new Mock<IFormFactory>().Object, new RecoveryManager(ScanningContext)));
    }

    public override void Dispose()
    {
        base.Dispose();
        _desktopController.Cleanup();
    }

    [Fact]
    public async Task Initialize_IfNotRunBefore_SetsFirstRunDate()
    {
        Assert.False(_config.Get(c => c.HasBeenRun));
        Assert.Null(_config.Get(c => c.FirstRunDate));

        await _desktopController.Initialize();

        Assert.True(_config.Get(c => c.HasBeenRun));
        DateAsserts.Recent(TimeSpan.FromMilliseconds(100), _config.Get(c => c.FirstRunDate));
        _notifcationManager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Initialize_IfAlreadyRun_DoesntSetFirstRunDate()
    {
        var firstRunDate = DateTime.Now - TimeSpan.FromDays(29);
        _config.User.Set(c => c.HasBeenRun, true);
        _config.User.Set(c => c.FirstRunDate, firstRunDate);

        await _desktopController.Initialize();

        Assert.True(_config.Get(c => c.HasBeenRun));
        Assert.Equal(firstRunDate, _config.Get(c => c.FirstRunDate));
        _notifcationManager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Initialize_IfRun30DaysAgo_ShowsDonatePrompt()
    {
        var firstRunDate = DateTime.Now - TimeSpan.FromDays(31);
        _config.User.Set(c => c.HasBeenRun, true);
        _config.User.Set(c => c.FirstRunDate, firstRunDate);

        await _desktopController.Initialize();

        _notifcationManager.Verify(x => x.DonatePrompt());
        Assert.True(_config.Get(c => c.HasBeenPromptedForDonation));
        DateAsserts.Recent(TimeSpan.FromMilliseconds(100), _config.Get(c => c.LastDonatePromptDate));
    }

    [Fact]
    public async Task Initialize_IfDonatePromptAlreadyShown_DoesntShowDonatePrompt()
    {
        var firstRunDate = DateTime.Now - TimeSpan.FromDays(62);
        _config.User.Set(c => c.HasBeenRun, true);
        _config.User.Set(c => c.FirstRunDate, firstRunDate);
        var donatePromptDate = DateTime.Now - TimeSpan.FromDays(31);
        _config.User.Set(c => c.HasBeenPromptedForDonation, true);
        _config.User.Set(c => c.LastDonatePromptDate, donatePromptDate);

        await _desktopController.Initialize();

        Assert.True(_config.Get(c => c.HasBeenPromptedForDonation));
        Assert.Equal(donatePromptDate, _config.Get(c => c.LastDonatePromptDate));
        _notifcationManager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Initialize_WithStillImageArgs_StartsScan()
    {
        _stillImage.ParseArgs(new[] { "/StiEvent:blah", "/StiDevice:abc" });

        await _desktopController.Initialize();

        _desktopScanController.Verify(c => c.ScanWithDevice("abc"));
        _desktopScanController.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Initialize_WithUpdateChecksDisabled_DoesntCheckForUpdate()
    {
        await _desktopController.Initialize();
        await Task.Delay(50);

        Assert.False(_config.Get(c => c.HasCheckedForUpdates));
        Assert.Null(_config.Get(c => c.LastUpdateCheckDate));
        _updateChecker.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Initialize_WithNoUpdate_DoesntPromptToUpdate()
    {
        _config.User.Set(c => c.CheckForUpdates, true);

        await _desktopController.Initialize();
        await Task.Delay(50);

        Assert.True(_config.Get(c => c.HasCheckedForUpdates));
        DateAsserts.Recent(TimeSpan.FromMilliseconds(100), _config.Get(c => c.LastUpdateCheckDate));
        _updateChecker.Verify(x => x.CheckForUpdates());
        _updateChecker.VerifyNoOtherCalls();
        _notifcationManager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Initialize_WithUpdate_NotifiesOfUpdate()
    {
        _config.User.Set(c => c.CheckForUpdates, true);
        var mockUpdateInfo =
            new UpdateInfo("10.0.0", "https://www.example.com", Array.Empty<byte>(), Array.Empty<byte>());
        _updateChecker.Setup(x => x.CheckForUpdates()).ReturnsAsync(mockUpdateInfo);

        await _desktopController.Initialize();
        await Task.Delay(50);

        Assert.True(_config.Get(c => c.HasCheckedForUpdates));
        DateAsserts.Recent(TimeSpan.FromMilliseconds(100), _config.Get(c => c.LastUpdateCheckDate));
        _updateChecker.Verify(x => x.CheckForUpdates());
        _notifcationManager.Verify(x => x.UpdateAvailable(_updateChecker.Object, mockUpdateInfo));
        _updateChecker.VerifyNoOtherCalls();
        _notifcationManager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Initialize_WithNoUpdatePrompt_DoesntCheckForUpdate()
    {
        _config.AppDefault.Set(c => c.NoUpdatePrompt, true);
        _config.User.Set(c => c.CheckForUpdates, true);

        await _desktopController.Initialize();
        await Task.Delay(50);

        Assert.False(_config.Get(c => c.HasCheckedForUpdates));
        Assert.Null(_config.Get(c => c.LastUpdateCheckDate));
        _updateChecker.VerifyNoOtherCalls();
        _notifcationManager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Initialize_WithRecentUpdateCheck_DoesntCheckForUpdate()
    {
        var updateCheckDate = DateTime.Now - TimeSpan.FromDays(6);
        _config.User.Set(c => c.CheckForUpdates, true);
        _config.User.Set(c => c.HasCheckedForUpdates, true);
        _config.User.Set(c => c.LastUpdateCheckDate, updateCheckDate);

        await _desktopController.Initialize();
        await Task.Delay(50);

        Assert.True(_config.Get(c => c.HasCheckedForUpdates));
        Assert.Equal(updateCheckDate, _config.Get(c => c.LastUpdateCheckDate));
        _updateChecker.VerifyNoOtherCalls();
        _notifcationManager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Initialize_WithOldUpdateCheck_NotifiesOfUpdate()
    {
        var updateCheckDate = DateTime.Now - TimeSpan.FromDays(8);
        _config.User.Set(c => c.CheckForUpdates, true);
        _config.User.Set(c => c.HasCheckedForUpdates, true);
        _config.User.Set(c => c.LastUpdateCheckDate, updateCheckDate);
        var mockUpdateInfo =
            new UpdateInfo("10.0.0", "https://www.example.com", Array.Empty<byte>(), Array.Empty<byte>());
        _updateChecker.Setup(x => x.CheckForUpdates()).ReturnsAsync(mockUpdateInfo);

        await _desktopController.Initialize();
        await Task.Delay(50);

        Assert.True(_config.Get(c => c.HasCheckedForUpdates));
        DateAsserts.Recent(TimeSpan.FromMilliseconds(100), _config.Get(c => c.LastUpdateCheckDate));
        _updateChecker.Verify(x => x.CheckForUpdates());
        _notifcationManager.Verify(x => x.UpdateAvailable(_updateChecker.Object, mockUpdateInfo));
        _updateChecker.VerifyNoOtherCalls();
        _notifcationManager.VerifyNoOtherCalls();
    }
}