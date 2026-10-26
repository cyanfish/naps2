using NAPS2.EtoForms;
using NAPS2.EtoForms.Notifications;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Images;
using NAPS2.Lib.Tests.Automation;
using NAPS2.Pdf;
using NAPS2.Sdk.Tests;
using NAPS2.Sdk.Tests.Asserts;
using NSubstitute;
using Xunit;

namespace NAPS2.Lib.Tests.ImportExport;

public class ExportControllerTests : ContextualTests
{
    private readonly DialogHelper _dialogHelper;
    private readonly IOverwritePrompt _overwritePrompt;
    private readonly ISaveNotify _notify;
    private readonly Naps2Config _config;
    private readonly ExportController _controller;
    private IEmailProviderFactory? _emailProviderFactory;

    public ExportControllerTests()
    {
        _dialogHelper = Substitute.For<DialogHelper>();
        _overwritePrompt = Substitute.For<IOverwritePrompt>();
        _notify = Substitute.For<ISaveNotify>();
        _config = Naps2Config.Stub();

        var nameManager = new NameManager();
        var operationFactory = Substitute.For<IOperationFactory>();
        operationFactory.Create<SavePdfOperation>().Returns(_ =>
            new SavePdfOperation(new PdfExporter(ScanningContext), _overwritePrompt, nameManager,
                _emailProviderFactory));
        operationFactory.Create<SaveImagesOperation>().Returns(_ =>
            new SaveImagesOperation(_overwritePrompt, ImageContext, nameManager));

        _controller = new ExportController(_dialogHelper, operationFactory, Substitute.For<IFormFactory>(),
            Substitute.For<OperationProgress>(), _config, new UiImageList(), nameManager);
    }

    private static string Year => DateTime.Now.ToString("yyyy");

    private List<UiImage> CreateUiImages(params byte[][] images) =>
        CreateScannedImages(images).Select(x => new UiImage(x)).ToList();

    private void SetDialogPath(string path)
    {
        _dialogHelper.PromptToSavePdf(Arg.Any<string?>(), out Arg.Any<string?>()).Returns(x =>
        {
            x[1] = path;
            return true;
        });
        _dialogHelper.PromptToSaveImage(Arg.Any<string?>(), out Arg.Any<string?>()).Returns(x =>
        {
            x[1] = path;
            return true;
        });
    }

    [Fact]
    public async Task SavePdf_FromDialog()
    {
        SetDialogPath(P("test.pdf"));

        Assert.True(await _controller.SavePdf(CreateUiImages(ImageResources.dog, ImageResources.dog_gray), _notify));

        PdfAsserts.AssertImages(P("test.pdf"), ImageResources.dog, ImageResources.dog_gray);
        _notify.Received().PdfSaved(P("test.pdf"));
    }

    [Fact]
    public async Task SavePdf_FromDialog_Existing()
    {
        // The save dialog already confirmed overwriting
        File.WriteAllText(P("test.pdf"), "blah");
        SetDialogPath(P("test.pdf"));

        Assert.True(await _controller.SavePdf(CreateUiImages(ImageResources.dog), _notify));

        _overwritePrompt.DidNotReceiveWithAnyArgs().ConfirmOverwrite(default!);
        PdfAsserts.AssertImages(P("test.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task SavePdf_FromDialog_Placeholders()
    {
        File.WriteAllText(P($"test_{Year}_1.pdf"), "blah");
        SetDialogPath(P("test_$(YYYY)_$(n).pdf"));

        Assert.True(await _controller.SavePdf(CreateUiImages(ImageResources.dog), _notify));

        Assert.Equal("blah", File.ReadAllText(P($"test_{Year}_1.pdf")));
        PdfAsserts.AssertImages(P($"test_{Year}_2.pdf"), ImageResources.dog);
        _notify.Received().PdfSaved(P($"test_{Year}_2.pdf"));
    }

    [Fact]
    public async Task SavePdf_SkipSavePrompt()
    {
        File.WriteAllText(P("test1.pdf"), "blah");
        _config.Run.Set(c => c.PdfSettings.SkipSavePrompt, true);
        _config.Run.Set(c => c.PdfSettings.DefaultFileName, P("test$(n).pdf"));

        Assert.True(await _controller.SavePdf(CreateUiImages(ImageResources.dog), _notify));

        _dialogHelper.DidNotReceiveWithAnyArgs().PromptToSavePdf(default, out _);
        Assert.Equal("blah", File.ReadAllText(P("test1.pdf")));
        PdfAsserts.AssertImages(P("test2.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task SavePdf_SinglePagePdfs()
    {
        _config.Run.Set(c => c.PdfSettings.SinglePagePdfs, true);
        SetDialogPath(P("test.pdf"));

        Assert.True(await _controller.SavePdf(CreateUiImages(ImageResources.dog, ImageResources.dog_gray), _notify));

        PdfAsserts.AssertImages(P("test.1.pdf"), ImageResources.dog);
        PdfAsserts.AssertImages(P("test.2.pdf"), ImageResources.dog_gray);
        _notify.Received().PdfSaved(P("test.1.pdf"));
    }

    [Fact]
    public async Task SaveImages_FromDialog()
    {
        SetDialogPath(P("test.jpg"));

        Assert.True(await _controller.SaveImages(CreateUiImages(ImageResources.dog), _notify));

        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("test.jpg")));
        _notify.Received().ImagesSaved(1, P("test.jpg"));
    }

    [Fact]
    public async Task SaveImages_FromDialog_Existing()
    {
        // The save dialog already confirmed overwriting
        File.WriteAllText(P("test.jpg"), "blah");
        SetDialogPath(P("test.jpg"));

        Assert.True(await _controller.SaveImages(CreateUiImages(ImageResources.dog), _notify));

        _overwritePrompt.DidNotReceiveWithAnyArgs().ConfirmOverwrite(default!);
        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("test.jpg")));
    }

    [Fact]
    public async Task SaveImages_FromDialog_Multiple()
    {
        SetDialogPath(P("test.jpg"));

        Assert.True(await _controller.SaveImages(CreateUiImages(ImageResources.dog, ImageResources.dog_gray),
            _notify));

        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("test.1.jpg")));
        ImageAsserts.Similar(ImageResources.dog_gray, ImageContext.Load(P("test.2.jpg")));
        _notify.Received().ImagesSaved(2, P("test.1.jpg"));
    }

    [Fact]
    public async Task SaveImages_SkipSavePrompt()
    {
        _config.Run.Set(c => c.ImageSettings.SkipSavePrompt, true);
        _config.Run.Set(c => c.ImageSettings.DefaultFileName, P("test_$(YYYY)_$(n).jpg"));

        Assert.True(await _controller.SaveImages(CreateUiImages(ImageResources.dog, ImageResources.dog_gray),
            _notify));

        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P($"test_{Year}_1.jpg")));
        ImageAsserts.Similar(ImageResources.dog_gray, ImageContext.Load(P($"test_{Year}_2.jpg")));
    }

    [Fact]
    public async Task EmailPdf_AttachmentNamePlaceholders()
    {
        var emailProviderFactory = new MockEmailProviderFactory(message =>
        {
            Assert.Single(message.Attachments);
            Assert.Equal($"scan_{Year}.pdf", message.Attachments[0].AttachmentName);
            PdfAsserts.AssertImages(message.Attachments[0].FilePath, ImageResources.dog);
        });
        _emailProviderFactory = emailProviderFactory;
        _config.User.Set(c => c.EmailSetup.ProviderType, EmailProviderType.System);
        _config.Run.Set(c => c.EmailSettings.AttachmentName, "scan_$(YYYY)");

        Assert.True(await _controller.EmailPdf(CreateUiImages(ImageResources.dog)));

        emailProviderFactory.CheckAsserts();
        emailProviderFactory.VerifyExactlyOneMessageSent();
    }
}
