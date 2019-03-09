using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Scan;
using NAPS2.Sdk.Tests.Asserts;
using NAPS2.Sdk.Tests.Mocks;
using NAPS2.Util;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan
{
    public class AutoSaveTests : FileSystemTests
    {
        [Fact]
        public async Task NoImages()
        {
            var errorOutput = new Mock<ErrorOutput>();
            var device = new ScanDevice("test_id", "test_name");
            var driver = new MockScanDriver(errorOutput.Object)
            {
                MockDevices = new List<ScanDevice> { device }
            };

            var scanProfile = new ScanProfile
            {
                Device = device,
                EnableAutoSave = true,
                AutoSaveSettings = new AutoSaveSettings
                {
                    FilePath = Path.Combine(FolderPath, "test$(n).pdf")
                }
            };
            var scanParams = new ScanParams();
            await driver.Scan(scanProfile, scanParams).ToList();
            var files = Folder.GetFiles();

            Assert.Empty(files);
            errorOutput.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task OneImageOnePdf()
        {
            var errorOutput = new Mock<ErrorOutput>();
            var device = new ScanDevice("test_id", "test_name");
            var driver = new MockScanDriver(errorOutput.Object)
            {
                MockDevices = new List<ScanDevice> { device },
                MockOutput = new List<ScannedImage>
                {
                    CreateScannedImage()
                }
            };

            var scanProfile = new ScanProfile
            {
                Device = device,
                EnableAutoSave = true,
                AutoSaveSettings = new AutoSaveSettings
                {
                    FilePath = Path.Combine(FolderPath, "test$(n).pdf")
                }
            };
            var scanParams = new ScanParams();
            await driver.Scan(scanProfile, scanParams).ToList();
            var files = Folder.GetFiles();

            Assert.Single(files);
            PdfAsserts.AssertPageCount(1, files[0].FullName);
            errorOutput.VerifyNoOtherCalls();
        }

        private ScannedImage CreateScannedImage()
        {
            return new ScannedImage(new GdiImage(new Bitmap(100, 100)));
        }
    }
}
