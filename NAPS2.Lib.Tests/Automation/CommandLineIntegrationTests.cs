using NAPS2.Automation;
using NAPS2.ImportExport.Pdf;
using NAPS2.Sdk.Tests;
using NAPS2.Sdk.Tests.Asserts;
using NAPS2.Sdk.Tests.Mocks;
using Ninject;
using Xunit;
using Xunit.Abstractions;

namespace NAPS2.Lib.Tests.Automation;

// TODO: Write tests for every option, or as many as possible
public class CommandLineIntegrationTests : ContextualTests
{
    private static readonly byte[] Image1 = ImageResources.dog;
    private static readonly byte[] Image2 = ImageResources.dog_h_n300;
    private static readonly byte[] Image3 = ImageResources.dog_h_p300;
    private static readonly byte[] Image4 = ImageResources.dog_s_n300;
    private static readonly byte[] Image5 = ImageResources.dog_s_p300;
    // TODO: Figure out why 1bit causes failures, I assume it's just a test issue of some kind but idk
    private static readonly byte[] Image6 = ImageResources.dog_bw_24bit;
    private static readonly byte[] PatchT = ImageResources.patcht;

    private readonly AutomationHelper _automationHelper;

    public CommandLineIntegrationTests(ITestOutputHelper testOutputHelper)
    {
        _automationHelper = new AutomationHelper(this, testOutputHelper);
    }

    [Fact]
    public async Task ScanSanity()
    {
        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                Verbose = true
            },
            Image1);
        PdfAsserts.AssertImages(path, Image1);
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task ScanWithNoImages()
    {
        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                Verbose = true
            });
        Assert.False(File.Exists(path));
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task ScanWithOcr()
    {
        SetUpOcr();
        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                Verbose = true,
                OcrLang = "eng"
            },
            ImageResources.ocr_test);
        PdfAsserts.AssertContainsTextOnce("ADVERTISEMENT.", path);
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task ScanPdfSettings_DefaultMetadata()
    {
        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                Verbose = true
            },
            kernel =>
            {
                var config = kernel.Get<Naps2Config>();
                config.User.Set(c => c.PdfSettings.Metadata, new PdfMetadata
                {
                    Author = "author1",
                    Creator = "creator1",
                    Keywords = "keywords1",
                    Subject = "subject1",
                    Title = "title1"
                });
            },
            Image1);
        PdfAsserts.AssertMetadata(new PdfMetadata
        {
            Author = "NAPS2",
            Creator = "NAPS2",
            Keywords = "",
            Subject = "Scanned Image",
            Title = "Scanned Image"
        }, path);
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task ScanPdfSettings_SavedMetadata()
    {
        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                UseSavedMetadata = true,
                Verbose = true
            },
            kernel =>
            {
                var config = kernel.Get<Naps2Config>();
                config.User.Set(c => c.PdfSettings.Metadata, new PdfMetadata
                {
                    Author = "author1",
                    Creator = "creator1",
                    Keywords = "keywords1",
                    Subject = "subject1",
                    Title = "title1"
                });
            },
            Image1);
        PdfAsserts.AssertMetadata(new PdfMetadata
        {
            Author = "author1",
            Creator = "creator1",
            Keywords = "keywords1",
            Subject = "subject1",
            Title = "title1"
        }, path);
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task ScanPdfSettings_CustomMetadata()
    {
        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                PdfAuthor = "author1",
                PdfSubject = "subject1",
                PdfTitle = "title1",
                PdfKeywords = "keywords1",
                Verbose = true
            },
            Image1);
        PdfAsserts.AssertMetadata(new PdfMetadata
        {
            Author = "author1",
            Creator = "NAPS2",
            Keywords = "keywords1",
            Subject = "subject1",
            Title = "title1"
        }, path);
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task ScanPdfSettings_SavedEncryptConfig()
    {
        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                UseSavedEncryptConfig = true,
                Verbose = true
            },
            kernel =>
            {
                var config = kernel.Get<Naps2Config>();
                config.User.Set(c => c.PdfSettings.Encryption, new PdfEncryption
                {
                    EncryptPdf = true,
                    OwnerPassword = "hello",
                    UserPassword = "world",
                    AllowAnnotations = true,
                    AllowContentCopying = false
                });
            },
            Image1);
        PdfAsserts.AssertEncrypted(path, "hello", "world", x =>
        {
            Assert.True(x.PermitAnnotations);
            Assert.False(x.PermitExtractContent);
        });
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task ScanPdfSettings_FileEncryptConfig()
    {
        var encryptConfigPath = $"{FolderPath}/encrypt.xml";
        File.WriteAllText(encryptConfigPath, @"
<PdfEncryption>
    <EncryptPdf>true</EncryptPdf>
    <OwnerPassword>hello</OwnerPassword>
    <UserPassword>world</UserPassword>
    <AllowAnnotations>true</AllowAnnotations>
    <AllowContentCopying>false</AllowContentCopying>
</PdfEncryption>");

        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                EncryptConfig = encryptConfigPath,
                Verbose = true
            },
            Image1);
        PdfAsserts.AssertEncrypted(path, "hello", "world", x =>
        {
            Assert.True(x.PermitAnnotations);
            Assert.False(x.PermitExtractContent);
        });
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task ScanPdfSettings_InvalidEncryptConfig()
    {
        var encryptConfigPath = $"{FolderPath}/encrypt.xml";
        File.WriteAllText(encryptConfigPath, "blah blah");

        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                EncryptConfig = encryptConfigPath,
                Verbose = true
            },
            Image1);
        Assert.False(File.Exists(path));
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task ExistingFile_NoOverwrite()
    {
        var path = $"{FolderPath}/test.pdf";
        File.WriteAllText(path, "blah");
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                Verbose = true
            },
            Image1);
        Assert.Equal("blah", File.ReadAllText(path));
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task ExistingFile_ForceOverwrite()
    {
        var path = $"{FolderPath}/test.pdf";
        File.WriteAllText(path, "blah");
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                ForceOverwrite = true,
                Verbose = true
            },
            Image1);
        PdfAsserts.AssertImages(path, Image1);
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task MultipleImages()
    {
        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                Verbose = true
            },
            new[] { Image1, Image2, Image3, Image4, Image5, Image6 });
        PdfAsserts.AssertImages(path, Image1, Image2, Image3, Image4, Image5, Image6);
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task MultipleImages_Interleave()
    {
        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                Interleave = true,
                Verbose = true
            },
            new[] { Image1, Image3, Image5, Image2, Image4, Image6 });
        PdfAsserts.AssertImages(path, Image1, Image2, Image3, Image4, Image5, Image6);
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task MultipleImages_AltInterleave()
    {
        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                AltInterleave = true,
                Verbose = true
            },
            new[] { Image1, Image3, Image5, Image6, Image4, Image2 });
        PdfAsserts.AssertImages(path, Image1, Image2, Image3, Image4, Image5, Image6);
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task MultipleImages_Deinterleave()
    {
        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                Deinterleave = true,
                Verbose = true
            },
            new[] { Image1, Image2, Image3, Image4, Image5, Image6 });
        PdfAsserts.AssertImages(path, Image1, Image3, Image5, Image2, Image4, Image6);
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task MultipleImages_AltDeinterleave()
    {
        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                AltDeinterleave = true,
                Verbose = true
            },
            new[] { Image1, Image2, Image3, Image4, Image5, Image6 });
        PdfAsserts.AssertImages(path, Image1, Image3, Image5, Image6, Image4, Image2);
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task MultipleImages_Reverse()
    {
        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                Reverse = true,
                Verbose = true
            },
            new[] { Image1, Image2, Image3, Image4, Image5, Image6 });
        PdfAsserts.AssertImages(path, Image6, Image5, Image4, Image3, Image2, Image1);
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task MultipleImages_MultipleInterleaveOptions()
    {
        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                Interleave = true,
                Deinterleave = true,
                Verbose = true
            },
            new[] { Image1, Image2, Image3, Image4, Image5, Image6 });
        Assert.False(File.Exists(path));
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task MultipleImages_InterleaveAndReverse()
    {
        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                Interleave = true,
                Reverse = true,
                Verbose = true
            },
            new[] { Image1, Image3, Image5, Image2, Image4, Image6 });
        PdfAsserts.AssertImages(path, Image6, Image5, Image4, Image3, Image2, Image1);
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task SplitWithNoPlaceholder()
    {
        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                Split = true,
                Verbose = true
            },
            new[] { Image1, Image2, Image3 });
        PdfAsserts.AssertPageCount(1, $"{FolderPath}/test.1.pdf");
        PdfAsserts.AssertPageCount(1, $"{FolderPath}/test.2.pdf");
        PdfAsserts.AssertPageCount(1, $"{FolderPath}/test.3.pdf");
        Assert.False(File.Exists($"{FolderPath}/test.4.pdf"));
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task SplitWithPlaceholder()
    {
        var path = $"{FolderPath}/test$(n).pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                Split = true,
                Verbose = true
            },
            new[] { Image1, Image2, Image3 });
        PdfAsserts.AssertImages($"{FolderPath}/test1.pdf", Image1);
        PdfAsserts.AssertImages($"{FolderPath}/test2.pdf", Image2);
        PdfAsserts.AssertImages($"{FolderPath}/test3.pdf", Image3);
        Assert.False(File.Exists($"{FolderPath}/test4.pdf"));
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task SplitSize()
    {
        var path = $"{FolderPath}/test$(n).pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                SplitSize = 2,
                Verbose = true
            },
            new[] { Image1, Image2, Image3, Image4, Image5 });
        PdfAsserts.AssertImages($"{FolderPath}/test1.pdf", Image1, Image2);
        PdfAsserts.AssertImages($"{FolderPath}/test2.pdf", Image3, Image4);
        PdfAsserts.AssertImages($"{FolderPath}/test3.pdf", Image5);
        Assert.False(File.Exists($"{FolderPath}/test4.pdf"));
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task SplitSizeWithMultipleScans()
    {
        var path = $"{FolderPath}/test$(n).pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                Number = 3,
                SplitSize = 2,
                Verbose = true
            },
            new ScanDriverFactoryBuilder()
                .WithScannedImages(Image1, Image2, Image3)
                .WithScannedImages(Image4, Image5)
                .WithScannedImages()
                .Build());
        PdfAsserts.AssertImages($"{FolderPath}/test1.pdf", Image1, Image2);
        PdfAsserts.AssertImages($"{FolderPath}/test2.pdf", Image3);
        PdfAsserts.AssertImages($"{FolderPath}/test3.pdf", Image4, Image5);
        Assert.False(File.Exists($"{FolderPath}/test4.pdf"));
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task SplitScans()
    {
        var path = $"{FolderPath}/test$(n).pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                SplitScans = true,
                Number = 4,
                Verbose = true
            },
            new ScanDriverFactoryBuilder()
                .WithScannedImages(Image1)
                .WithScannedImages()
                .WithScannedImages(Image2, Image3)
                .WithScannedImages(Image4)
                .Build());
        PdfAsserts.AssertImages($"{FolderPath}/test1.pdf", Image1);
        PdfAsserts.AssertImages($"{FolderPath}/test2.pdf", Image2, Image3);
        PdfAsserts.AssertImages($"{FolderPath}/test3.pdf", Image4);
        Assert.False(File.Exists($"{FolderPath}/test4.pdf"));
        AssertRecoveryCleanedUp();
    }

    [Fact]
    public async Task SplitPatchT()
    {
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                SplitPatchT = true,
                OutputPath = $"{FolderPath}/$(n).pdf",
                Verbose = true
            },
            Image1, Image2, PatchT, Image3, PatchT, PatchT, Image4, PatchT);
        PdfAsserts.AssertImages($"{FolderPath}/1.pdf", Image1, Image2);
        PdfAsserts.AssertImages($"{FolderPath}/2.pdf", Image3);
        PdfAsserts.AssertImages($"{FolderPath}/3.pdf", Image4);
        Assert.False(File.Exists($"{FolderPath}/4.pdf"));
        AssertRecoveryCleanedUp();
    }

    private void AssertRecoveryCleanedUp()
    {
        Assert.False(Directory.Exists(Path.Combine(FolderPath, "recovery")));
    }

    // TODO: Add tests for all options, as well as key combinations
}