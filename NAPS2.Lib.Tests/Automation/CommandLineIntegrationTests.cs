using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NAPS2.Automation;
using NAPS2.Images.Storage;
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

namespace NAPS2.Lib.Tests.Automation
{
    public class CommandLineIntegrationTests : ContextualTexts
    {
        private readonly ITestOutputHelper testOutputHelper;

        public CommandLineIntegrationTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        private async Task RunCommand(AutomatedScanningOptions options, params Bitmap[] imagesToScan)
        {
            Console.SetOut(new TestWriter(testOutputHelper));
            var scanDriverFactory = new ScanDriverFactoryBuilder().WithScannedImages(imagesToScan).Build();
            var kernel = new StandardKernel(new CommonModule(), new ConsoleModule(), new TestModule(ImageContext, scanDriverFactory));
            // TODO: Consider how best to handle this - it isn't thread safe.
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
                    OutputPath = $"{FolderPath}/test.pdf"
                },
                BarcodeTestsData.color_image);
            PdfAsserts.AssertPageCount(1, $"{FolderPath}/test.pdf");
        }

        [Fact]
        public async Task SplitPatchT()
        {
            await RunCommand(
                new AutomatedScanningOptions
                {
                    Number = 1,
                    SplitPatchT = true,
                    OutputPath = $"{FolderPath}/$(n).pdf"
                },
                BarcodeTestsData.color_image,
                BarcodeTestsData.color_image,
                BarcodeTestsData.patcht,
                BarcodeTestsData.color_image,
                BarcodeTestsData.patcht,
                BarcodeTestsData.patcht,
                BarcodeTestsData.color_image,
                BarcodeTestsData.patcht);
            PdfAsserts.AssertPageCount(2, $"{FolderPath}/1.pdf");
            PdfAsserts.AssertPageCount(1, $"{FolderPath}/2.pdf");
            PdfAsserts.AssertPageCount(1, $"{FolderPath}/3.pdf");
            Assert.False(File.Exists($"{FolderPath}/4.pdf"));
        }
        
        // TODO: Add tests for all options, as well as key combinations

        private class TestModule : NinjectModule
        {
            private readonly ImageContext imageContext;
            private readonly IScanDriverFactory scanDriverFactory;

            public TestModule(ImageContext imageContext, IScanDriverFactory scanDriverFactory)
            {
                this.imageContext = imageContext;
                this.scanDriverFactory = scanDriverFactory;
            }
            
            public override void Load()
            {
                Rebind<ImageContext>().ToConstant(imageContext);
                Rebind<OcrEngineManager>().ToConstant(new OcrEngineManager());
                Rebind<IScanDriverFactory>().ToConstant(scanDriverFactory);
                Rebind<IScanBridgeFactory>().To<InProcScanBridgeFactory>();
            }
        }
        
        // TODO: Clean this up.
        // TODO: Probably AutomatedScanning should take a TextWriter (instead of Console.WriteLine) for test isolation.
        

        private class TestWriter : TextWriter
        {
            readonly ITestOutputHelper output;
            
            public TestWriter(ITestOutputHelper output)
            {
                this.output = output;
            }
            public override Encoding Encoding => Encoding.UTF8;

            public override void WriteLine(string message) => output.WriteLine(message);

            public override void WriteLine(string format, params object[] args) => output.WriteLine(format, args);
        }
    }
}