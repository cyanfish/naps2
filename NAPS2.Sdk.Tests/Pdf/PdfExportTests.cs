using System.Threading;
using Moq;
using NAPS2.Pdf;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Pdf;

public class PdfExporterTests : ContextualTests
{
    private readonly PdfExporter _exporter;

    public PdfExporterTests()
    {
        _exporter = new PdfExporter(ScanningContext);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportJpegImage(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        using var image = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));

        await _exporter.Export(filePath, new[] { image }, new PdfExportParams());

        PdfAsserts.AssertImages(filePath, ImageResources.dog);
        PdfAsserts.AssertImageFilter(filePath, 0, "DCTDecode");
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportGrayJpegImage(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        using var image = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog_gray))
            .WithTransform(new GrayscaleTransform(), true);

        await _exporter.Export(filePath, new[] { image }, new PdfExportParams());

        PdfAsserts.AssertImages(filePath, ImageResources.dog_gray);
        PdfAsserts.AssertImageFilter(filePath, 0, "DCTDecode");
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportPngImage(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        using var image = ScanningContext.CreateProcessedImage(
            LoadImage(ImageResources.dog_png), BitDepth.Color, true, -1);

        await _exporter.Export(filePath, new[] { image }, new PdfExportParams());

        PdfAsserts.AssertImages(filePath, ImageResources.dog);
        PdfAsserts.AssertImageFilter(filePath, 0, "FlateDecode");
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportUnalignedPngImage(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        // Width is 99 (not divisible by 4)
        var image = ScanningContext.CreateProcessedImage(
            LoadImage(ImageResources.dog_99w), BitDepth.Color, true, -1);

        await _exporter.Export(filePath, new[] { image }, new PdfExportParams());

        PdfAsserts.AssertImages(filePath, ImageResources.dog_99w);
        PdfAsserts.AssertImageFilter(filePath, 0, "FlateDecode");
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportAlphaImage(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        using var image = ScanningContext.CreateProcessedImage(
            LoadImage(ImageResources.dog_alpha), BitDepth.Color, false, -1);

        await _exporter.Export(filePath, new[] { image }, new PdfExportParams());

        // TODO: This assert is broken as pdfium rendering doesn't work for images with masks yet
        // PdfAsserts.AssertImages(filePath, ImageResources.dog_alpha);
        PdfAsserts.AssertImageFilter(filePath, 0, "FlateDecode");
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportMaskedImage(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        using var image = ScanningContext.CreateProcessedImage(
            LoadImage(ImageResources.dog_mask), BitDepth.Color, false, -1);

        await _exporter.Export(filePath, new[] { image }, new PdfExportParams());

        // TODO: This assert is broken as pdfium rendering doesn't work for images with masks yet
        // PdfAsserts.AssertImages(filePath, ImageResources.dog_alpha);
        PdfAsserts.AssertImageFilter(filePath, 0, "FlateDecode");
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportBlackAndWhiteImage(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        var storageImage = LoadImage(ImageResources.dog_bw);
        using var image = ScanningContext.CreateProcessedImage(storageImage);

        await _exporter.Export(filePath, new[] { image }, new PdfExportParams());

        PdfAsserts.AssertImages(filePath, ImageResources.dog_bw);
        PdfAsserts.AssertImageFilter(filePath, 0, "CCITTFaxDecode");
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportGrayImage(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        var storageImage = LoadImage(ImageResources.dog_clustered_gray);
        using var image = ScanningContext.CreateProcessedImage(storageImage);

        await _exporter.Export(filePath, new[] { image }, new PdfExportParams());

        PdfAsserts.AssertImages(filePath, ImageResources.dog_clustered_gray);
        PdfAsserts.AssertImageFilter(filePath, 0, "FlateDecode");
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportBlackAndWhiteImageByMetadata(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        using var image = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog_bw_24bit),
            BitDepth.BlackAndWhite, true, -1);

        await _exporter.Export(filePath, new[] { image }, new PdfExportParams());

        PdfAsserts.AssertImages(filePath, ImageResources.dog_bw);
        PdfAsserts.AssertImageFilter(filePath, 0, "CCITTFaxDecode");
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportMetadata(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        using var image = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));
        var metadata = new PdfMetadata
        {
            Author = "author",
            Title = "title",
            Keywords = "keywords",
            Subject = "subject"
        };

        await _exporter.Export(filePath, new[] { image }, new PdfExportParams { Metadata = metadata });

        PdfAsserts.AssertMetadata(metadata with { Creator = "NAPS2" }, filePath, "world");
        // TODO: We should also test embedded dates etc. somewhere, no tests for that yet 
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportUnicodeMetadata(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        using var image = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));
        var metadata = new PdfMetadata
        {
            Author = "מְחַבֵּר",
            Title = "כותרת",
            Keywords = "מילות מפתח",
            Subject = "נושא"
        };

        await _exporter.Export(filePath, new[] { image }, new PdfExportParams { Metadata = metadata });

        PdfAsserts.AssertMetadata(metadata with { Creator = "NAPS2" }, filePath, "world");
        // TODO: We should also test embedded dates etc. somewhere, no tests for that yet 
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportUnicodePath(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "מְחַבֵּר.pdf");
        using var image = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));

        await _exporter.Export(filePath, new[] { image }, new PdfExportParams());

        PdfAsserts.AssertImages(filePath, ImageResources.dog);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportEncrypted(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        using var image = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));

        await _exporter.Export(filePath, new[] { image }, new PdfExportParams
        {
            Encryption = new()
            {
                EncryptPdf = true,
                OwnerPassword = "hello",
                UserPassword = "world"
            }
        });

        PdfAsserts.AssertEncrypted(filePath, "hello", "world");
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportMetadataEncrypted(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        using var image = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));
        var metadata = new PdfMetadata
        {
            Author = "author",
            Title = "title",
            Keywords = "keywords",
            Subject = "subject"
        };

        await _exporter.Export(filePath, new[] { image }, new PdfExportParams
        {
            Encryption = new()
            {
                EncryptPdf = true,
                OwnerPassword = "hello",
                UserPassword = "world"
            },
            Metadata = metadata
        });

        PdfAsserts.AssertEncrypted(filePath, "hello", "world");
        PdfAsserts.AssertMetadata(metadata with { Creator = "NAPS2" }, filePath, "world");
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportUnicodeMetadataEncrypted(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        using var image = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));
        var metadata = new PdfMetadata
        {
            Author = "מְחַבֵּר",
            Title = "כותרת",
            Keywords = "מילות מפתח",
            Subject = "נושא"
        };

        await _exporter.Export(filePath, new[] { image }, new PdfExportParams
        {
            Encryption = new()
            {
                EncryptPdf = true,
                OwnerPassword = "hello",
                UserPassword = "world"
            },
            Metadata = metadata
        });

        PdfAsserts.AssertEncrypted(filePath, "hello", "world");
        PdfAsserts.AssertMetadata(metadata with { Creator = "NAPS2" }, filePath, "world");
    }

    [Theory]
    [ClassData(typeof(OcrTestData))]
    public async Task ExportProgress(OcrTestConfig config)
    {
        config.StorageConfig.Apply(this);

        var progressMock = new Mock<ProgressCallback>();

        var filePath = Path.Combine(FolderPath, "test.pdf");
        var images = new[] { CreateScannedImage(), CreateScannedImage(), CreateScannedImage() };
        var result = await _exporter.Export(filePath, images, new PdfExportParams(), config.OcrParams,
            progress: progressMock.Object);

        Assert.True(result);
        progressMock.Verify(x => x(0, 3));
        progressMock.Verify(x => x(1, 3));
        progressMock.Verify(x => x(2, 3));
        progressMock.Verify(x => x(3, 3));
        progressMock.VerifyNoOtherCalls();
    }

    [Theory]
    [ClassData(typeof(OcrTestData))]
    public async Task ExportCancellation(OcrTestConfig config)
    {
        config.StorageConfig.Apply(this);

        var progressMock = new Mock<ProgressCallback>();
        var cts = new CancellationTokenSource();

        void Progress(int current, int total)
        {
            progressMock.Object(current, total);
            if (current == 1) cts.Cancel();
        }

        var filePath = Path.Combine(FolderPath, "test.pdf");
        var images = new[] { CreateScannedImage(), CreateScannedImage(), CreateScannedImage() };
        var result = await _exporter.Export(filePath, images, new PdfExportParams(), config.OcrParams,
            progress: new ProgressHandler(Progress, cts.Token));

        Assert.False(result);
        progressMock.Verify(x => x(0, 3));
        progressMock.Verify(x => x(1, 3));
        progressMock.VerifyNoOtherCalls();
    }
}