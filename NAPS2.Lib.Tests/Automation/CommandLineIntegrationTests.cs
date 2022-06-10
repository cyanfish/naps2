using System.Drawing;
using System.Text;
using NAPS2.Automation;
using NAPS2.Modules;
using NAPS2.Ocr;
using NAPS2.Scan.Internal;
using NAPS2.Sdk.Tests;
using NAPS2.Sdk.Tests.Asserts;
using NAPS2.Sdk.Tests.Images;
using NAPS2.Sdk.Tests.Mocks;
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
        var kernel = new StandardKernel(new CommonModule(), new ConsoleModule(), new TestModule(ImageContext, scanDriverFactory, _testOutputHelper, FolderPath));
        var automatedScanning = kernel.Get<AutomatedScanning>(new ConstructorArgument("options", options));
        await automatedScanning.Execute();
    }

    [Fact]
    public async Task ScanSanity()
    {
        await RunCommand(
            new AutomatedScanningOptions
            {
                Number = 1,
                OutputPath = $"{FolderPath}/test.pdf",
                Verbose = true
            },
            BarcodeTestsData.color_image);
        PdfAsserts.AssertPageCount(1, $"{FolderPath}/test.pdf");
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

    private void AssertRecoveryCleanedUp()
    {
        Assert.False(new DirectoryInfo($"{FolderPath}/recovery").Exists);
    }

    // TODO: Add tests for all options, as well as key combinations

    private class TestModule : NinjectModule
    {
        private readonly ImageContext _imageContext;
        private readonly IScanDriverFactory _scanDriverFactory;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly string _folderPath;

        public TestModule(ImageContext imageContext, IScanDriverFactory scanDriverFactory,
            ITestOutputHelper testOutputHelper, string folderPath)
        {
            _imageContext = imageContext;
            _scanDriverFactory = scanDriverFactory;
            _testOutputHelper = testOutputHelper;
            _folderPath = folderPath;
        }

        public override void Load()
        {
            Rebind<ImageContext>().ToConstant(_imageContext);
            Rebind<OcrEngineManager>().ToConstant(new OcrEngineManager());
            Rebind<IScanDriverFactory>().ToConstant(_scanDriverFactory);
            Rebind<IScanBridgeFactory>().To<InProcScanBridgeFactory>();
            Rebind<ConsoleOutput>().ToSelf().WithConstructorArgument("writer", new TestOutputTextWriter(_testOutputHelper));
            
            string recoveryFolderPath = Path.Combine(_folderPath, "recovery");
            var recoveryStorageManager = RecoveryStorageManager.CreateFolder(recoveryFolderPath);
            var fileStorageManager = new FileStorageManager(recoveryFolderPath);
            Kernel.Bind<RecoveryStorageManager>().ToConstant(recoveryStorageManager);
            Kernel.Bind<FileStorageManager>().ToConstant(fileStorageManager);
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