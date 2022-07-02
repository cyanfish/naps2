using System.Drawing;
using System.Text;
using NAPS2.Automation;
using NAPS2.Modules;
using NAPS2.Ocr;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Sdk.Tests;
using NAPS2.Sdk.Tests.Asserts;
using NAPS2.Sdk.Tests.Mocks;
using NAPS2.Sdk.Tests.Ocr;
using Ninject;
using Ninject.Modules;
using Ninject.Parameters;
using Xunit;
using Xunit.Abstractions;

namespace NAPS2.Lib.Tests.Automation;

public class CommandLineIntegrationTests : ContextualTexts
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CommandLineIntegrationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private async Task RunCommand(AutomatedScanningOptions options, params Bitmap[] imagesToScan)
    {
        var scanDriverFactory = new ScanDriverFactoryBuilder().WithScannedImages(imagesToScan).Build();
        var kernel = new StandardKernel(new CommonModule(), new ConsoleModule(options),
            new TestModule(ScanningContext, ImageContext, scanDriverFactory, _testOutputHelper, FolderPath));
        var automatedScanning = kernel.Get<AutomatedScanning>();
        await automatedScanning.Execute();
    }

    [Fact]
    public async Task ScanSanity()
    {
        var path = $"{FolderPath}/test.pdf";
        await RunCommand(
            new AutomatedScanningOptions
            {
                Number = 1,
                OutputPath = path,
                Verbose = true
            },
            SharedData.color_image);
        Assert.True(File.Exists(path));
        PdfAsserts.AssertPageCount(1, path);
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task SplitPatchT()
    {
        await RunCommand(
            new AutomatedScanningOptions
            {
                Number = 1,
                SplitPatchT = true,
                OutputPath = $"{FolderPath}/$(n).pdf",
                Verbose = true
            },
            SharedData.color_image,
            SharedData.color_image,
            SharedData.patcht,
            SharedData.color_image,
            SharedData.patcht,
            SharedData.patcht,
            SharedData.color_image,
            SharedData.patcht);
        PdfAsserts.AssertPageCount(2, $"{FolderPath}/1.pdf");
        PdfAsserts.AssertPageCount(1, $"{FolderPath}/2.pdf");
        PdfAsserts.AssertPageCount(1, $"{FolderPath}/3.pdf");
        Assert.False(File.Exists($"{FolderPath}/4.pdf"));
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task ScanWithOcr()
    {
        var fast = Path.Combine(FolderPath, "fast");
        Directory.CreateDirectory(fast);
        CopyResourceToFile(TesseractResources.tesseract_x64, FolderPath, "tesseract.exe");
        CopyResourceToFile(TesseractResources.eng_traineddata, fast, "eng.traineddata");

        var path = $"{FolderPath}/test.pdf";
        await RunCommand(
            new AutomatedScanningOptions
            {
                Number = 1,
                OutputPath = path,
                Verbose = true,
                OcrLang = "eng"
            },
            SharedData.ocr_test);
        Assert.True(File.Exists(path));
        PdfAsserts.AssertContainsText("ADVERTISEMENT.", path);
        AssertRecoveryCleanedUp();
    }

    private void AssertRecoveryCleanedUp()
    {
        Assert.False(new DirectoryInfo($"{FolderPath}/recovery").Exists);
    }

    // TODO: Add tests for all options, as well as key combinations

    private class TestModule : NinjectModule
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

    private class TestOutputTextWriter : TextWriter
    {
        readonly ITestOutputHelper _output;

        public TestOutputTextWriter(ITestOutputHelper output)
        {
            _output = output;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string message) => _output.WriteLine(message);

        public override void WriteLine(string format, params object[] args) => _output.WriteLine(format, args);
    }
}