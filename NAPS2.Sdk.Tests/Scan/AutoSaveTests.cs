using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Ocr;
using NAPS2.Operation;
using NAPS2.Scan;
using NAPS2.Sdk.Tests.Asserts;
using NAPS2.Sdk.Tests.Mocks;
using NAPS2.Util;
using NAPS2.WinForms;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan
{
    public class AutoSaveTests : FileSystemTests
    {
        [Fact]
        public async Task NoImages()
        {
            var errorOutput = new Mock<ErrorOutput>();
            var driver = Driver(errorOutput.Object, 0);

            var scanProfile = Profile(new AutoSaveSettings
            {
                FilePath = Path.Combine(FolderPath, "test$(n).pdf")
            });
            var scanParams = new ScanParams();
            var scannedImages = await driver.Scan(scanProfile, scanParams).ToList();
            var files = Folder.GetFiles();

            Assert.Empty(scannedImages);
            Assert.Empty(files);
            errorOutput.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task OneImageOnePdf()
        {
            var errorOutput = new Mock<ErrorOutput>();
            var driver = Driver(errorOutput.Object, 1);

            var scanProfile = Profile(new AutoSaveSettings
            {
                FilePath = Path.Combine(FolderPath, "test$(n).pdf")
            });
            var scanParams = new ScanParams();
            var scannedImages = await driver.Scan(scanProfile, scanParams).ToList();
            var files = Folder.GetFiles();

            Assert.Single(scannedImages);
            Assert.Single(files);
            PdfAsserts.AssertPageCount(1, files[0].FullName);
            errorOutput.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task TwoImagesOnePdf()
        {
            var errorOutput = new Mock<ErrorOutput>();
            var driver = Driver(errorOutput.Object, 2);

            var scanProfile = Profile(new AutoSaveSettings
            {
                FilePath = Path.Combine(FolderPath, "test$(n).pdf"),
                Separator = SaveSeparator.FilePerScan
            });
            var scanParams = new ScanParams();
            var scannedImages = await driver.Scan(scanProfile, scanParams).ToList();
            var files = Folder.GetFiles();

            Assert.Equal(2, scannedImages.Count);
            Assert.Single(files);
            PdfAsserts.AssertPageCount(2, files[0].FullName);
            errorOutput.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task TwoImagesTwoPdfs()
        {
            var errorOutput = new Mock<ErrorOutput>();
            var driver = Driver(errorOutput.Object, 2);

            var scanProfile = Profile(new AutoSaveSettings
            {
                FilePath = Path.Combine(FolderPath, "test$(n).pdf"),
                Separator = SaveSeparator.FilePerPage
            });
            var scanParams = new ScanParams();
            var scannedImages = await driver.Scan(scanProfile, scanParams).ToList();
            var files = Folder.GetFiles();

            Assert.Equal(2, scannedImages.Count);
            Assert.Equal(2, files.Length);
            PdfAsserts.AssertPageCount(1, files[0].FullName);
            PdfAsserts.AssertPageCount(1, files[1].FullName);
            errorOutput.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task TwoImagesTwoJpegs()
        {
            var errorOutput = new Mock<ErrorOutput>();
            var driver = Driver(errorOutput.Object, 2);

            var scanProfile = Profile(new AutoSaveSettings
            {
                FilePath = Path.Combine(FolderPath, "test$(n).jpg")
            });
            var scanParams = new ScanParams();
            var scannedImages = await driver.Scan(scanProfile, scanParams).ToList();
            var files = Folder.GetFiles();

            Assert.Equal(2, scannedImages.Count);
            Assert.Equal(2, files.Length);
            errorOutput.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ClearAfterSaving()
        {
            var errorOutput = new Mock<ErrorOutput>();
            var driver = Driver(errorOutput.Object, 2);

            var scanProfile = Profile(new AutoSaveSettings
            {
                FilePath = Path.Combine(FolderPath, "test$(n).jpg"),
                ClearImagesAfterSaving = true
            });
            var scanParams = new ScanParams();
            var scannedImages = await driver.Scan(scanProfile, scanParams).ToList();
            var files = Folder.GetFiles();

            Assert.Empty(scannedImages);
            Assert.Equal(2, files.Length);
            errorOutput.VerifyNoOtherCalls();
        }

        // TODO: ClearAfterSaving with error, PromptForFilePath, SaveSeparator 

        private ScanDevice Device => new ScanDevice("test_id", "test_name");

        private MockScanDriver Driver(ErrorOutput errorOutput, int images) => new MockScanDriver(errorOutput, CreateAutoSaver(errorOutput))
        {
            MockDevices = new List<ScanDevice> { Device },
            MockOutput = Enumerable.Range(0, images).Select(i => CreateScannedImage()).ToList()
        };

        private ScanProfile Profile(AutoSaveSettings autoSaveSettings) => new ScanProfile
        {
            Device = Device,
            EnableAutoSave = true,
            AutoSaveSettings = autoSaveSettings
        };

        private static AutoSaver CreateAutoSaver(ErrorOutput errorOutput)
        {
            return new AutoSaver(
                PdfSettingsProvider.Wrap(new PdfSettings()),
                ImageSettingsProvider.Wrap(new ImageSettings()),
                new OcrEngineManager(),
                new OcrRequestQueue(new OcrEngineManager(), new StubOperationProgress()),
                errorOutput,
                new StubDialogHelper(),
                new StubOperationProgress(),
                null,
                new PdfSharpExporter(new MemoryStreamRenderer()),
                new StubOverwritePrompt(),
                new BitmapRenderer());
        }

        private ScannedImage CreateScannedImage()
        {
            return new ScannedImage(new GdiImage(new Bitmap(100, 100)));
        }
    }
}
