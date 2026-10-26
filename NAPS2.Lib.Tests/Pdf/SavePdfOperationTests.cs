using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.Lib.Tests.Automation;
using NAPS2.Ocr;
using NAPS2.Pdf;
using NAPS2.Sdk.Tests;
using NAPS2.Sdk.Tests.Asserts;
using NSubstitute;
using Xunit;

namespace NAPS2.Lib.Tests.Pdf;

public class SavePdfOperationTests : ContextualTests
{
    private static readonly DateTime TestDate = new(2024, 3, 7, 21, 5, 2);

    private readonly IOverwritePrompt _overwritePrompt;
    private readonly PdfExporter _pdfExporter;
    private readonly NameManager _nameManager;

    public SavePdfOperationTests()
    {
        _overwritePrompt = Substitute.For<IOverwritePrompt>();
        _pdfExporter = new PdfExporter(ScanningContext);
        _nameManager = new NameManager();
    }

    private SavePdfOperation CreateOperation() => new(_pdfExporter, _overwritePrompt, _nameManager);

    private bool Start(SavePdfOperation op, string fileName, ICollection<ProcessedImage> images,
        PdfSettings pdfSettings = null, OcrParams ocrParams = null, EmailMessage emailMessage = null,
        OverwriteMode overwriteMode = OverwriteMode.Prompt, int? batchCount = null, int? batchIndex = null)
    {
        return op.Start(fileName, images, pdfSettings ?? new PdfSettings(), ocrParams ?? OcrParams.Empty,
            emailMessage, new SaveParams
            {
                OverwriteMode = overwriteMode,
                BatchCount = batchCount,
                BatchIndex = batchIndex,
                DateTime = TestDate
            });
    }

    private async Task<bool> Save(string fileName, ICollection<ProcessedImage> images,
        PdfSettings pdfSettings = null, OverwriteMode overwriteMode = OverwriteMode.Prompt, int? batchCount = null,
        int? batchIndex = null)
    {
        var op = CreateOperation();
        if (!Start(op, fileName, images, pdfSettings, overwriteMode: overwriteMode, batchCount: batchCount,
                batchIndex: batchIndex))
        {
            return false;
        }
        return await op.Success;
    }

    [Fact]
    public async Task SingleFile()
    {
        var op = CreateOperation();
        Assert.True(Start(op, P("test.pdf"), CreateScannedImages(ImageResources.dog, ImageResources.dog_gray)));
        Assert.True(await op.Success);

        Assert.Equal(P("test.pdf"), op.FirstFileSaved);
        PdfAsserts.AssertImages(P("test.pdf"), ImageResources.dog, ImageResources.dog_gray);
        Assert.Single(Folder.GetFiles());
    }

    [Fact]
    public async Task DatePlaceholders()
    {
        Assert.True(await Save(P("test_$(YYYY)-$(MM)-$(DD)_$(hh)_$(mm)_$(ss).pdf"),
            CreateScannedImages(ImageResources.dog)));

        PdfAsserts.AssertImages(P("test_2024-03-07_21_05_02.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task NumberPlaceholder_SkipsExisting()
    {
        File.WriteAllText(P("test1.pdf"), "blah");

        Assert.True(await Save(P("test$(n).pdf"), CreateScannedImages(ImageResources.dog)));

        Assert.Equal("blah", File.ReadAllText(P("test1.pdf")));
        PdfAsserts.AssertImages(P("test2.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task Existing_PromptYes()
    {
        File.WriteAllText(P("test.pdf"), "blah");
        _overwritePrompt.ConfirmOverwrite(P("test.pdf")).Returns(OverwriteResponse.Yes);

        Assert.True(await Save(P("test.pdf"), CreateScannedImages(ImageResources.dog)));

        _overwritePrompt.Received().ConfirmOverwrite(P("test.pdf"));
        PdfAsserts.AssertImages(P("test.pdf"), ImageResources.dog);
    }

    [Fact]
    public void Existing_PromptNo()
    {
        File.WriteAllText(P("test.pdf"), "blah");
        _overwritePrompt.ConfirmOverwrite(P("test.pdf")).Returns(OverwriteResponse.No);

        Assert.False(Start(CreateOperation(), P("test.pdf"), CreateScannedImages(ImageResources.dog)));

        Assert.Equal("blah", File.ReadAllText(P("test.pdf")));
    }

    [Fact]
    public async Task Existing_AlreadyConfirmed()
    {
        // e.g. the user already confirmed overwriting in the save dialog
        File.WriteAllText(P("test.pdf"), "blah");

        Assert.True(await Save(P("test.pdf"), CreateScannedImages(ImageResources.dog),
            overwriteMode: OverwriteMode.Confirmed));

        _overwritePrompt.DidNotReceiveWithAnyArgs().ConfirmOverwrite(default!);
        PdfAsserts.AssertImages(P("test.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task Existing_Confirmed_NumberPlaceholder()
    {
        // Confirming only skips the prompt; auto-numbered files still skip existing files
        File.WriteAllText(P("test1.pdf"), "blah");

        Assert.True(await Save(P("test$(n).pdf"), CreateScannedImages(ImageResources.dog),
            overwriteMode: OverwriteMode.Confirmed));

        _overwritePrompt.DidNotReceiveWithAnyArgs().ConfirmOverwrite(default!);
        Assert.Equal("blah", File.ReadAllText(P("test1.pdf")));
        PdfAsserts.AssertImages(P("test2.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task Existing_Never()
    {
        File.WriteAllText(P("test.pdf"), "blah");

        Assert.True(await Save(P("test.pdf"), CreateScannedImages(ImageResources.dog),
            overwriteMode: OverwriteMode.Never));

        _overwritePrompt.DidNotReceiveWithAnyArgs().ConfirmOverwrite(default!);
        Assert.Equal("blah", File.ReadAllText(P("test.pdf")));
        PdfAsserts.AssertImages(P("test.1.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task Existing_Force()
    {
        File.WriteAllText(P("test.pdf"), "blah");

        Assert.True(await Save(P("test.pdf"), CreateScannedImages(ImageResources.dog),
            overwriteMode: OverwriteMode.Force));

        _overwritePrompt.DidNotReceiveWithAnyArgs().ConfirmOverwrite(default!);
        PdfAsserts.AssertImages(P("test.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task Existing_Force_NumberPlaceholder()
    {
        File.WriteAllText(P("test1.pdf"), "blah");

        Assert.True(await Save(P("test$(n).pdf"), CreateScannedImages(ImageResources.dog),
            overwriteMode: OverwriteMode.Force));

        PdfAsserts.AssertImages(P("test1.pdf"), ImageResources.dog);
        Assert.Single(Folder.GetFiles());
    }

    [Fact]
    public async Task Force_BatchIndex()
    {
        File.WriteAllText(P("test1.pdf"), "blah");
        File.WriteAllText(P("test2.pdf"), "blah");

        Assert.True(await Save(P("test$(n).pdf"), CreateScannedImages(ImageResources.dog),
            overwriteMode: OverwriteMode.Force, batchCount: 2, batchIndex: 0));
        Assert.True(await Save(P("test$(n).pdf"), CreateScannedImages(ImageResources.dog_gray),
            overwriteMode: OverwriteMode.Force, batchCount: 2, batchIndex: 1));

        PdfAsserts.AssertImages(P("test1.pdf"), ImageResources.dog);
        PdfAsserts.AssertImages(P("test2.pdf"), ImageResources.dog_gray);
        Assert.Equal(2, Folder.GetFiles().Length);
    }

    [Fact]
    public async Task BatchCount_SingleFileIsNumbered()
    {
        Assert.True(await Save(P("test.pdf"), CreateScannedImages(ImageResources.dog), batchCount: 2));
        Assert.True(await Save(P("test.pdf"), CreateScannedImages(ImageResources.dog_gray), batchCount: 2));

        _overwritePrompt.DidNotReceiveWithAnyArgs().ConfirmOverwrite(default!);
        PdfAsserts.AssertImages(P("test.1.pdf"), ImageResources.dog);
        PdfAsserts.AssertImages(P("test.2.pdf"), ImageResources.dog_gray);
        Assert.Equal(2, Folder.GetFiles().Length);
    }

    [Fact]
    public async Task BatchCount_PadsDigits()
    {
        Assert.True(await Save(P("test.pdf"), CreateScannedImages(ImageResources.dog), batchCount: 10));

        PdfAsserts.AssertImages(P("test.01.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task SinglePagePdfs()
    {
        var op = CreateOperation();
        Assert.True(Start(op, P("test.pdf"), CreateScannedImages(ImageResources.dog, ImageResources.dog_gray),
            new PdfSettings { SinglePagePdfs = true }));
        Assert.True(await op.Success);

        Assert.Equal(P("test.1.pdf"), op.FirstFileSaved);
        PdfAsserts.AssertImages(P("test.1.pdf"), ImageResources.dog);
        PdfAsserts.AssertImages(P("test.2.pdf"), ImageResources.dog_gray);
        Assert.Equal(2, Folder.GetFiles().Length);
    }

    [Fact]
    public async Task SinglePagePdfs_NumberPlaceholder()
    {
        Assert.True(await Save(P("test$(nn).pdf"), CreateScannedImages(ImageResources.dog, ImageResources.dog_gray),
            new PdfSettings { SinglePagePdfs = true }));

        PdfAsserts.AssertImages(P("test01.pdf"), ImageResources.dog);
        PdfAsserts.AssertImages(P("test02.pdf"), ImageResources.dog_gray);
        Assert.Equal(2, Folder.GetFiles().Length);
    }

    [Fact]
    public async Task SinglePagePdfs_SkipsExisting()
    {
        File.WriteAllText(P("test.1.pdf"), "blah");

        Assert.True(await Save(P("test.pdf"), CreateScannedImages(ImageResources.dog, ImageResources.dog_gray),
            new PdfSettings { SinglePagePdfs = true }));

        _overwritePrompt.DidNotReceiveWithAnyArgs().ConfirmOverwrite(default!);
        Assert.Equal("blah", File.ReadAllText(P("test.1.pdf")));
        PdfAsserts.AssertImages(P("test.2.pdf"), ImageResources.dog);
        PdfAsserts.AssertImages(P("test.3.pdf"), ImageResources.dog_gray);
    }

    [Fact]
    public async Task SinglePagePdfs_OneImage()
    {
        // With only one page there's no need to number the file
        Assert.True(await Save(P("test.pdf"), CreateScannedImages(ImageResources.dog),
            new PdfSettings { SinglePagePdfs = true }));

        PdfAsserts.AssertImages(P("test.pdf"), ImageResources.dog);
        Assert.Single(Folder.GetFiles());
    }

    [Fact]
    public async Task MetadataPlaceholders()
    {
        Assert.True(await Save(P("test.pdf"), CreateScannedImages(ImageResources.dog), new PdfSettings
        {
            Metadata = new PdfMetadata
            {
                Author = "author $(YYYY)",
                Creator = "creator $(MM)",
                Keywords = "keywords $(DD)",
                Subject = "subject $(hh)",
                Title = "title $(mm)"
            }
        }));

        PdfAsserts.AssertMetadata(new PdfMetadata
        {
            Author = "author 2024",
            Creator = "creator 03",
            Keywords = "keywords 07",
            Subject = "subject 21",
            Title = "title 05"
        }, P("test.pdf"));
    }

    [Fact]
    public async Task SinglePagePdfs_MetadataNotNumbered()
    {
        Assert.True(await Save(P("test.pdf"), CreateScannedImages(ImageResources.dog, ImageResources.dog_gray),
            new PdfSettings
            {
                SinglePagePdfs = true,
                Metadata = new PdfMetadata { Title = "title", Creator = "creator", Keywords = "" }
            }));

        var expected = new PdfMetadata { Title = "title", Creator = "creator", Keywords = "" };
        PdfAsserts.AssertMetadata(expected, P("test.1.pdf"));
        PdfAsserts.AssertMetadata(expected, P("test.2.pdf"));
    }

    [Fact]
    public async Task Directory_UsesNumberedFileName()
    {
        File.WriteAllText(P("1.pdf"), "blah");

        Assert.True(await Save(FolderPath, CreateScannedImages(ImageResources.dog)));

        PdfAsserts.AssertImages(P("2.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task Email_AddsAttachment()
    {
        var emailProviderFactory = new MockEmailProviderFactory(message =>
        {
            Assert.Single(message.Attachments);
            Assert.Equal(P("attachment_2024.pdf"), message.Attachments[0].FilePath);
            Assert.Equal("attachment_2024.pdf", message.Attachments[0].AttachmentName);
        });

        var op = new SavePdfOperation(_pdfExporter, _overwritePrompt, _nameManager, emailProviderFactory);
        Assert.True(Start(op, P("attachment_$(YYYY).pdf"), CreateScannedImages(ImageResources.dog),
            emailMessage: new EmailMessage()));
        Assert.True(await op.Success);

        emailProviderFactory.CheckAsserts();
        emailProviderFactory.VerifyExactlyOneMessageSent();
    }

    [Fact]
    public async Task Concurrent_NumberPlaceholder_ChoosesDistinctFiles()
    {
        // The first file isn't written until OCR finishes, so the second save can't rely on the file existing
        SetUpFakeOcr(ifNoMatch: "hello", delay: 500);
        var ocrParams = new OcrParams("eng", OcrMode.Fast, 0);

        var op1 = CreateOperation();
        Assert.True(Start(op1, P("test$(n).pdf"), CreateScannedImages(ImageResources.dog, ImageResources.dog_gray),
            ocrParams: ocrParams));
        var op2 = CreateOperation();
        Assert.True(Start(op2, P("test$(n).pdf"), CreateScannedImages(ImageResources.dog_bw), ocrParams: ocrParams));
        Assert.True(await op1.Success);
        Assert.True(await op2.Success);

        Assert.Equal(P("test1.pdf"), op1.FirstFileSaved);
        Assert.Equal(P("test2.pdf"), op2.FirstFileSaved);
        PdfAsserts.AssertImages(P("test1.pdf"), ImageResources.dog, ImageResources.dog_gray);
        PdfAsserts.AssertImages(P("test2.pdf"), ImageResources.dog_bw);
    }
}
