using System.Drawing;
using NAPS2.Automation;
using NAPS2.ImportExport.Pdf;
using NAPS2.Sdk.Tests;
using NAPS2.Sdk.Tests.Asserts;
using NAPS2.Sdk.Tests.Images;
using NAPS2.Sdk.Tests.Ocr;
using Ninject;
using Xunit;
using Xunit.Abstractions;

namespace NAPS2.Lib.Tests.Automation;

// TODO: Write tests for every option, or as many as possible
public class CommandLineIntegrationTests : ContextualTests
{
    private static readonly Bitmap Image1 = SharedData.color_image;
    private static readonly Bitmap Image2 = TransformTestsData.color_image_h_n300;
    private static readonly Bitmap Image3 = TransformTestsData.color_image_h_p300;
    private static readonly Bitmap Image4 = TransformTestsData.color_image_s_n300;
    private static readonly Bitmap Image5 = TransformTestsData.color_image_s_p300;
    private static readonly Bitmap Image6 = TransformTestsData.color_image_bw;
    private static readonly Bitmap PatchT = SharedData.patcht;
    
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
        Assert.True(File.Exists(path));
        PdfAsserts.AssertPageCount(1, path);
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
            Image1,
            Image1,
            PatchT,
            Image1,
            PatchT,
            PatchT,
            Image1,
            PatchT);
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
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                OutputPath = path,
                Verbose = true,
                OcrLang = "eng"
            },
            SharedData.ocr_test);
        Assert.True(File.Exists(path));
        PdfAsserts.AssertContainsText("ADVERTISEMENT.", path);
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
        Assert.True(File.Exists(path));
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
        Assert.True(File.Exists(path));
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
        Assert.True(File.Exists(path));
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
        Assert.True(File.Exists(path));
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
        Assert.True(File.Exists(path));
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
        PdfAsserts.AssertPageCount(1, path);
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
        PdfAsserts.AssertImages(new[] { Image1, Image2, Image3, Image4, Image5, Image6 }, path);
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
        PdfAsserts.AssertImages(new[] { Image1, Image2, Image3, Image4, Image5, Image6 }, path);
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
        PdfAsserts.AssertImages(new[] { Image1, Image2, Image3, Image4, Image5, Image6 }, path);
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
        PdfAsserts.AssertImages(new[] { Image1, Image3, Image5, Image2, Image4, Image6 }, path);
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
        PdfAsserts.AssertImages(new[] { Image1, Image3, Image5, Image6, Image4, Image2 }, path);
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
        PdfAsserts.AssertImages(new[] { Image6, Image5, Image4, Image3, Image2, Image1 }, path);
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
        PdfAsserts.AssertImages(new[] { Image6, Image5, Image4, Image3, Image2, Image1 }, path);
        AssertRecoveryCleanedUp();
    }

    private void AssertRecoveryCleanedUp()
    {
    }

    // TODO: Add tests for all options, as well as key combinations
}