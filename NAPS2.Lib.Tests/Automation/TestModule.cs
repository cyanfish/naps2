using Autofac;
using NAPS2.Automation;
using NAPS2.Ocr;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Sdk.Tests.Mocks;
using Xunit.Abstractions;

namespace NAPS2.Lib.Tests.Automation;

internal class TestModule : Module
{
    private readonly ScanningContext _scanningContext;
    private readonly ImageContext _imageContext;
    private readonly IScanDriverFactory _scanDriverFactory;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string _folderPath;
    private readonly Action<ContainerBuilder> _containerBuilderSetup;

    public TestModule(ScanningContext scanningContext, ImageContext imageContext,
        IScanDriverFactory scanDriverFactory,
        ITestOutputHelper testOutputHelper, string folderPath, Action<ContainerBuilder> containerBuilderSetup)
    {
        _scanningContext = scanningContext;
        _imageContext = imageContext;
        _scanDriverFactory = scanDriverFactory;
        _testOutputHelper = testOutputHelper;
        _folderPath = folderPath;
        _containerBuilderSetup = containerBuilderSetup;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterInstance(_imageContext);
        builder.RegisterInstance(_scanDriverFactory);
        builder.RegisterType<InProcScanBridgeFactory>().As<IScanBridgeFactory>();
        builder.RegisterType<ConsoleOutput>().AsSelf()
            .WithParameter("writer", new TestOutputTextWriter(_testOutputHelper));
        builder.RegisterInstance(Naps2Config.Stub());
        builder.Register<IProfileManager>(_ =>
        {
            var userPath = Path.Combine(_folderPath, "profiles.xml");
            var systemPath = Path.Combine(_folderPath, "sysprofiles.xml");
            var profileManager = new ProfileManager(userPath, systemPath, false, false, false);
            var defaultProfile = new ScanProfile
            {
                IsDefault = true,
                Device = new ScanProfileDevice("001", "Some Scanner")
            };
            profileManager.Mutate(
                new ListMutation<ScanProfile>.Append(defaultProfile),
                new Selectable<ScanProfile>());
            return profileManager;
        }).SingleInstance();
        builder.Register(_ =>
        {
            var componentsPath = Path.Combine(_folderPath, "components");
            Directory.CreateDirectory(componentsPath);
            return new TesseractLanguageManager(componentsPath);
        }).SingleInstance();
        builder.Register(_ => _scanningContext.OcrEngine ?? new StubOcrEngine());

        string recoveryFolderPath = Path.Combine(_folderPath, "recovery");
        builder.Register(ctx => RecoveryStorageManager.CreateFolder(recoveryFolderPath, ctx.Resolve<UiImageList>()))
            .SingleInstance();
        builder.RegisterInstance(new FileStorageManager(recoveryFolderPath));

        builder.RegisterBuildCallback(ctx =>
        {
            var scanningContext = ctx.Resolve<ScanningContext>();
            scanningContext.OcrEngine = ctx.Resolve<IOcrEngine>();
            scanningContext.TempFolderPath = _scanningContext.TempFolderPath;
        });

        _containerBuilderSetup?.Invoke(builder);
    }
}