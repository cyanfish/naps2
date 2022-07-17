using NAPS2.Automation;
using NAPS2.Ocr;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Sdk.Tests.Mocks;
using Ninject;
using Ninject.Modules;
using Xunit.Abstractions;

namespace NAPS2.Lib.Tests.Automation;

internal class TestModule : NinjectModule
{
    private readonly ScanningContext _scanningContext;
    private readonly ImageContext _imageContext;
    private readonly IScanDriverFactory _scanDriverFactory;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string _folderPath;

    public TestModule(ScanningContext scanningContext, ImageContext imageContext,
        IScanDriverFactory scanDriverFactory,
        ITestOutputHelper testOutputHelper, string folderPath)
    {
        _scanningContext = scanningContext;
        _imageContext = imageContext;
        _scanDriverFactory = scanDriverFactory;
        _testOutputHelper = testOutputHelper;
        _folderPath = folderPath;
    }

    public override void Load()
    {
        Rebind<ImageContext>().ToConstant(_imageContext);
        Rebind<IScanDriverFactory>().ToConstant(_scanDriverFactory);
        Rebind<IScanBridgeFactory>().To<InProcScanBridgeFactory>();
        Rebind<ConsoleOutput>().ToSelf()
            .WithConstructorArgument("writer", new TestOutputTextWriter(_testOutputHelper));
        Rebind<Naps2Config>().ToConstant(Naps2Config.Stub());
        Rebind<IProfileManager>().ToMethod(_ =>
        {
            var userPath = Path.Combine(_folderPath, "profiles.xml");
            var systemPath = Path.Combine(_folderPath, "sysprofiles.xml");
            var profileManager = new ProfileManager(userPath, systemPath, false, false, false);
            var defaultProfile = new ScanProfile
            {
                IsDefault = true,
                Device = new ScanDevice("001", "Some Scanner")
            };
            profileManager.Mutate(
                new ListMutation<ScanProfile>.Append(defaultProfile),
                new Selectable<ScanProfile>());
            return profileManager;
        }).InSingletonScope();
        Rebind<TesseractLanguageManager>().ToMethod(_ =>
        {
            var componentsPath = Path.Combine(_folderPath, "components");
            Directory.CreateDirectory(componentsPath);
            return new TesseractLanguageManager(componentsPath);
        }).InSingletonScope();
        Rebind<IOcrEngine>().ToMethod(ctx => new TesseractOcrEngine(
            Path.Combine(_folderPath, "tesseract.exe"),
            _folderPath,
            _folderPath)).InSingletonScope();

        string recoveryFolderPath = Path.Combine(_folderPath, "recovery");
        var recoveryStorageManager =
            RecoveryStorageManager.CreateFolder(recoveryFolderPath, Kernel.Get<UiImageList>());
        var fileStorageManager = new FileStorageManager(recoveryFolderPath);
        Kernel.Bind<RecoveryStorageManager>().ToConstant(recoveryStorageManager);
        Kernel.Bind<FileStorageManager>().ToConstant(fileStorageManager);

        Kernel.Get<ScanningContext>().TempFolderPath = _scanningContext.TempFolderPath;
    }
}