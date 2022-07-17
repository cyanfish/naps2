using NAPS2.Automation;
using NAPS2.ImportExport.Pdf;
using NAPS2.Sdk.Tests;
using NAPS2.Sdk.Tests.Asserts;
using NAPS2.Sdk.Tests.Ocr;
using Ninject;
using PdfSharp.Pdf.Security;
using Xunit;
using Xunit.Abstractions;

namespace NAPS2.Lib.Tests.Automation;

// TODO: Write tests for every option, or as many as possible
public class CommandLineIntegrationTests : ContextualTests
{
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
        await _automationHelper.RunCommand(
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
        await _automationHelper.RunCommand(
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

    [Fact]
    public async Task ScanPdfSettings_DefaultMetadata()
    {
        var path = $"{FolderPath}/test.pdf";
        await _automationHelper.RunCommand(
            new AutomatedScanningOptions
            {
                Number = 1,
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
            SharedData.color_image);
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
                Number = 1,
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
            SharedData.color_image);
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
                Number = 1,
                OutputPath = path,
                PdfAuthor = "author1",
                PdfSubject = "subject1",
                PdfTitle = "title1",
                PdfKeywords = "keywords1",
                Verbose = true
            },
            SharedData.color_image);
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
                Number = 1,
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
            SharedData.color_image);
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
                Number = 1,
                OutputPath = path,
                EncryptConfig = encryptConfigPath,
                Verbose = true
            },
            SharedData.color_image);
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
                Number = 1,
                OutputPath = path,
                EncryptConfig = encryptConfigPath,
                Verbose = true
            },
            SharedData.color_image);
        Assert.False(File.Exists(path));
        AssertRecoveryCleanedUp();
    }

    private void AssertRecoveryCleanedUp()
    {
    }

    // TODO: Add tests for all options, as well as key combinations
}