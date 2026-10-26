using System.Collections.Immutable;
using System.Threading;
using NAPS2.EtoForms;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Images;
using NAPS2.Pdf;
using NAPS2.Scan;
using NAPS2.Scan.Batch;
using NAPS2.Sdk.Tests;
using NAPS2.Sdk.Tests.Asserts;
using NSubstitute;
using Xunit;

namespace NAPS2.Lib.Tests.Scan.Batch;

public class BatchScanPerformerTests : ContextualTests
{
    private readonly IScanPerformer _scanPerformer;
    private readonly IOverwritePrompt _overwritePrompt;
    private readonly Naps2Config _config;
    private readonly BatchScanPerformer _batchScanPerformer;
    private readonly Queue<byte[][]> _scans = new();

    public BatchScanPerformerTests()
    {
        _scanPerformer = Substitute.For<IScanPerformer>();
        _scanPerformer.PerformScan(default!, default!).ReturnsForAnyArgs(_ =>
            CreateScannedImages(_scans.Dequeue()).ToAsyncEnumerable());
        _overwritePrompt = Substitute.For<IOverwritePrompt>();
        _config = Naps2Config.Stub();

        var nameManager = new NameManager();
        var operationFactory = Substitute.For<IOperationFactory>();
        operationFactory.Create<SaveImagesOperation>()
            .Returns(_ => new SaveImagesOperation(_overwritePrompt, ImageContext, nameManager));
        var profileManager = Substitute.For<IProfileManager>();
        profileManager.Profiles.Returns(ImmutableList.Create(new ScanProfile { DisplayName = "profile" }));

        _batchScanPerformer = new BatchScanPerformer(
            _scanPerformer,
            new PdfExporter(ScanningContext),
            operationFactory,
            Substitute.For<IFormFactory>(),
            _config,
            profileManager,
            new ThumbnailController(new ThumbnailRenderQueue(ScanningContext, new ThumbnailRenderer(ImageContext)),
                _config),
            nameManager);
    }

    private async Task Run(BatchOutputType outputType, SaveSeparator separator, string savePath,
        params byte[][][] scans)
    {
        foreach (var scan in scans)
        {
            _scans.Enqueue(scan);
        }
        var settings = new BatchSettings
        {
            ProfileDisplayName = "profile",
            ScanType = scans.Length == 1 ? BatchScanType.Single : BatchScanType.MultipleWithDelay,
            ScanCount = scans.Length,
            ScanIntervalSeconds = 0,
            OutputType = outputType,
            SaveSeparator = separator,
            SavePath = savePath
        };
        await _batchScanPerformer.PerformBatchScan(settings, Substitute.For<IFormBase>(), _ => { }, _ => { },
            CancellationToken.None);
    }

    [Fact]
    public async Task SingleFile_Pdf()
    {
        await Run(BatchOutputType.SingleFile, SaveSeparator.FilePerScan, P("test$(n).pdf"),
            [ImageResources.dog, ImageResources.dog_gray], [ImageResources.dog_bw]);

        Assert.Single(Folder.GetFiles());
        PdfAsserts.AssertImages(P("test1.pdf"), ImageResources.dog, ImageResources.dog_gray, ImageResources.dog_bw);
    }

    [Fact]
    public async Task SingleFile_Pdf_DatePlaceholder()
    {
        var year = DateTime.Now.ToString("yyyy");

        await Run(BatchOutputType.SingleFile, SaveSeparator.FilePerScan, P("test_$(YYYY).pdf"),
            [ImageResources.dog]);

        PdfAsserts.AssertImages(P($"test_{year}.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task SingleFile_Pdf_Existing()
    {
        File.WriteAllText(P("test.pdf"), "blah");

        await Run(BatchOutputType.SingleFile, SaveSeparator.FilePerScan, P("test.pdf"), [ImageResources.dog]);

        Assert.Equal("blah", File.ReadAllText(P("test.pdf")));
        PdfAsserts.AssertImages(P("test.1.pdf"), ImageResources.dog);
    }

    [Fact]
    public async Task SingleFile_Pdf_MetadataPlaceholders()
    {
        var year = DateTime.Now.ToString("yyyy");
        _config.Run.Set(c => c.PdfSettings.Metadata, new PdfMetadata { Title = "title $(YYYY)", Creator = "NAPS2" });

        await Run(BatchOutputType.SingleFile, SaveSeparator.FilePerScan, P("test.pdf"), [ImageResources.dog]);

        PdfAsserts.AssertMetadata(new PdfMetadata { Title = $"title {year}", Creator = "NAPS2" }, P("test.pdf"));
    }

    [Fact]
    public async Task MultipleFiles_PerScan_Pdf()
    {
        await Run(BatchOutputType.MultipleFiles, SaveSeparator.FilePerScan, P("test$(n).pdf"),
            [ImageResources.dog, ImageResources.dog_gray], [ImageResources.dog_bw]);

        Assert.Equal(2, Folder.GetFiles().Length);
        PdfAsserts.AssertImages(P("test1.pdf"), ImageResources.dog, ImageResources.dog_gray);
        PdfAsserts.AssertImages(P("test2.pdf"), ImageResources.dog_bw);
    }

    [Fact]
    public async Task MultipleFiles_PerPage_Pdf_SkipsExisting()
    {
        File.WriteAllText(P("test2.pdf"), "blah");

        await Run(BatchOutputType.MultipleFiles, SaveSeparator.FilePerPage, P("test$(n).pdf"),
            [ImageResources.dog, ImageResources.dog_gray, ImageResources.dog_bw]);

        Assert.Equal("blah", File.ReadAllText(P("test2.pdf")));
        PdfAsserts.AssertImages(P("test1.pdf"), ImageResources.dog);
        PdfAsserts.AssertImages(P("test3.pdf"), ImageResources.dog_gray);
        PdfAsserts.AssertImages(P("test4.pdf"), ImageResources.dog_bw);
    }

    [Fact]
    public async Task MultipleFiles_PerPage_Pdf_NoPlaceholder()
    {
        await Run(BatchOutputType.MultipleFiles, SaveSeparator.FilePerPage, P("test.pdf"),
            [ImageResources.dog, ImageResources.dog_gray, ImageResources.dog_bw]);

        Assert.Equal(3, Folder.GetFiles().Length);
        PdfAsserts.AssertImages(P("test.1.pdf"), ImageResources.dog);
        PdfAsserts.AssertImages(P("test.2.pdf"), ImageResources.dog_gray);
        PdfAsserts.AssertImages(P("test.3.pdf"), ImageResources.dog_bw);
    }

    [Fact]
    public async Task MultipleFiles_PerPage_Images()
    {
        await Run(BatchOutputType.MultipleFiles, SaveSeparator.FilePerPage, P("test$(n).jpg"),
            [ImageResources.dog, ImageResources.dog_gray]);

        Assert.Equal(2, Folder.GetFiles().Length);
        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("test1.jpg")));
        ImageAsserts.Similar(ImageResources.dog_gray, ImageContext.Load(P("test2.jpg")));
    }

    [Fact]
    public async Task SingleFile_Images()
    {
        await Run(BatchOutputType.SingleFile, SaveSeparator.FilePerScan, P("test.jpg"),
            [ImageResources.dog, ImageResources.dog_gray]);

        Assert.Equal(2, Folder.GetFiles().Length);
        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("test.1.jpg")));
        ImageAsserts.Similar(ImageResources.dog_gray, ImageContext.Load(P("test.2.jpg")));
    }

    [Fact]
    public async Task MultipleFiles_PerPage_Images_NoPlaceholder()
    {
        await Run(BatchOutputType.MultipleFiles, SaveSeparator.FilePerPage, P("test.jpg"),
            [ImageResources.dog, ImageResources.dog_gray, ImageResources.dog_bw]);

        _overwritePrompt.DidNotReceiveWithAnyArgs().ConfirmOverwrite(default!);
        Assert.Equal(3, Folder.GetFiles().Length);
        ImageAsserts.Similar(ImageResources.dog, ImageContext.Load(P("test.1.jpg")));
        ImageAsserts.Similar(ImageResources.dog_gray, ImageContext.Load(P("test.2.jpg")));
        ImageAsserts.Similar(ImageResources.dog_bw, ImageContext.Load(P("test.3.jpg")));
    }
}
