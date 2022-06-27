using Moq;
using NAPS2.ImportExport.Images;
using NAPS2.Platform.Windows;
using NAPS2.Recovery;
using NAPS2.Update;
using NAPS2.WinForms;
using Xunit;

namespace NAPS2.Sdk.Tests.WinForms;

public class DesktopControllerTests : ContextualTexts
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
    private readonly Mock<StillImage> _stillImage;
    private readonly Mock<IUpdateChecker> _updateChecker;
    private readonly Mock<INotificationManager> _notifcationManager;
    private readonly ImageTransfer _imageTransfer;
    private readonly ImageClipboard _imageClipboard;
    private readonly Mock<IWinFormsExportHelper> _exportHelper;
    private readonly DesktopImagesController _desktopImagesController;
    private readonly Mock<IDesktopScanController> _desktopScanController;
    private readonly DesktopFormProvider _desktopFormProvider;

    public DesktopControllerTests()
    {
        ScanningContext.RecoveryPath = Path.Combine(FolderPath, "recovery");
        ScanningContext.FileStorageManager = new FileStorageManager(ScanningContext.RecoveryPath);
        _recoveryStorageManager = RecoveryStorageManager.CreateFolder(ScanningContext.RecoveryPath);
        _imageList = new UiImageList(_recoveryStorageManager);
        _thumbnailRenderQueue = new ThumbnailRenderQueue(ScanningContext, new ThumbnailRenderer(ImageContext));
        _operationProgress = new Mock<OperationProgress>();
        _config = Naps2Config.Stub();
        _operationFactory = new Mock<IOperationFactory>();
        _stillImage = new Mock<StillImage>();
        _updateChecker = new Mock<IUpdateChecker>();
        _notifcationManager = new Mock<INotificationManager>();
        _imageTransfer = new ImageTransfer(ImageContext);
        _imageClipboard = new ImageClipboard(ImageContext, _imageTransfer);
        _exportHelper = new Mock<IWinFormsExportHelper>();
        _desktopImagesController = new DesktopImagesController(_imageList);
        _desktopScanController = new Mock<IDesktopScanController>();
        _desktopFormProvider = new DesktopFormProvider();
        _desktopController = new DesktopController(
            ScanningContext,
            _imageList,
            _recoveryStorageManager,
            _thumbnailRenderQueue,
            _operationProgress.Object,
            _config,
            _operationFactory.Object,
            _stillImage.Object,
            _updateChecker.Object,
            _notifcationManager.Object,
            _imageTransfer,
            _imageClipboard,
            new ImageListActions(ImageContext, _imageList),
            _exportHelper.Object,
            _desktopImagesController,
            _desktopScanController.Object,
            _desktopFormProvider
        );

        _operationFactory.Setup(x => x.Create<RecoveryOperation>()).Returns(
            new RecoveryOperation(new Mock<IFormFactory>().Object, new RecoveryManager(ScanningContext)));
    }

    public override void Dispose()
    {
        base.Dispose();
        _desktopController.Cleanup();
    }

    [Fact]
    public async Task Initialize_SetsFirstRunDate_IfNotRunBefore()
    {
        Assert.False(_config.Get(c => c.HasBeenRun));
        Assert.Null(_config.Get(c => c.FirstRunDate));

        await _desktopController.Initialize();

        Assert.True(_config.Get(c => c.HasBeenRun));
        Assert.InRange(_config.Get(c => c.FirstRunDate) ?? DateTime.MinValue,
            DateTime.Now - TimeSpan.FromMilliseconds(10), DateTime.Now);
    }

    [Fact]
    public async Task Initialize_DoesntSetFirstRunDate_IfAlreadyRun()
    {
        var firstRunDate = DateTime.Now - TimeSpan.FromDays(1);
        _config.User.Set(c => c.HasBeenRun, true);
        _config.User.Set(c => c.FirstRunDate, firstRunDate);

        await _desktopController.Initialize();

        Assert.True(_config.Get(c => c.HasBeenRun));
        Assert.Equal(firstRunDate, _config.Get(c => c.FirstRunDate));
    }
}