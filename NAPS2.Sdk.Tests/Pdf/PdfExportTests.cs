using System.Threading;
using NAPS2.Pdf;
using NAPS2.Sdk.Tests.Asserts;
using NSubstitute;
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

        await _exporter.Export(filePath, new[] { image });

        PdfAsserts.AssertImages(filePath, ImageResources.dog);
        PdfAsserts.AssertImageFilter(filePath, 0, "DCTDecode");
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportJpegImageToStream(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        var fileStream = File.OpenWrite(filePath);
        using var image = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));

        await _exporter.Export(fileStream, new[] { image });
        fileStream.Close();

        PdfAsserts.AssertImages(filePath, ImageResources.dog);
        PdfAsserts.AssertImageFilter(filePath, 0, "DCTDecode");
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportGrayJpegImage(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        using var image = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog_gray_8bit)
            .PerformTransform(new GrayscaleTransform()));

        await _exporter.Export(filePath, new[] { image });

        PdfAsserts.AssertImages(filePath, ImageResources.dog_gray);
        PdfAsserts.AssertImageFilter(filePath, 0, "DCTDecode");
    }

    [Fact]
    public async Task ExportJpegWithoutEncoding()
    {
        SetUpFileStorage();

        var filePath = Path.Combine(FolderPath, "test.pdf");
        using var image = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));

        await _exporter.Export(filePath, new[] { image });

        var renderer = new PdfiumPdfRenderer();
        var pdfImage = renderer.Render(ImageContext, filePath, PdfRenderSize.Default).Single();
        ImageAsserts.Similar(image.Render(), pdfImage, 0);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportPngImage(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        using var image = ScanningContext.CreateProcessedImage(
            LoadImage(ImageResources.dog_png), BitDepth.Color, true, -1, null);

        await _exporter.Export(filePath, new[] { image });

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
            LoadImage(ImageResources.dog_99w), BitDepth.Color, true, -1, null);

        await _exporter.Export(filePath, new[] { image });

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
            LoadImage(ImageResources.dog_alpha), BitDepth.Color, false, -1, null);

        await _exporter.Export(filePath, new[] { image });

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
            LoadImage(ImageResources.dog_mask), BitDepth.Color, false, -1, null);

        await _exporter.Export(filePath, new[] { image });

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

        await _exporter.Export(filePath, new[] { image });

        PdfAsserts.AssertImages(filePath, ImageResources.dog_bw);
        PdfAsserts.AssertImageFilter(filePath, 0, "CCITTFaxDecode");
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportBlackAndWhiteAlternatingPixels(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        var storageImage = LoadImage(ImageResources.bw_alternating);
        using var image = ScanningContext.CreateProcessedImage(storageImage);

        await _exporter.Export(filePath, new[] { image });

        PdfAsserts.AssertImages(filePath, ImageResources.bw_alternating);
        // Alternating black & white pixels is the worst case for CCITT encoding (i.e. the encoded size is bigger than
        // unencoded), therefore PDFSharp should choose FlateDecode (PNG encoding) instead
        PdfAsserts.AssertImageFilter(filePath, 0, "FlateDecode");
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportGrayImage(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        var storageImage = LoadImage(ImageResources.dog_clustered_gray);
        using var image = ScanningContext.CreateProcessedImage(storageImage);

        await _exporter.Export(filePath, new[] { image });

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
            BitDepth.BlackAndWhite, true, -1, null);

        await _exporter.Export(filePath, new[] { image });

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

        await _exporter.Export(filePath, new[] { image });

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
        SetUpFakeOcr();

        var progressMock = Substitute.For<ProgressCallback>();

        var filePath = Path.Combine(FolderPath, "test.pdf");
        var images = new[] { CreateScannedImage(), CreateScannedImage(), CreateScannedImage() };
        var result = await _exporter.Export(filePath, images, ocrParams: config.OcrParams,
            progress: progressMock);

        Assert.True(result);
        progressMock.Received()(0, 3);
        progressMock.Received()(1, 3);
        progressMock.Received()(2, 3);
        progressMock.Received()(3, 3);
        progressMock.ReceivedCallsCount(4);
    }

    [Theory]
    [ClassData(typeof(OcrTestData))]
    public async Task ExportCancellation(OcrTestConfig config)
    {
        config.StorageConfig.Apply(this);
        SetUpFakeOcr();

        var progressMock = Substitute.For<ProgressCallback>();
        var cts = new CancellationTokenSource();

        void Progress(int current, int total)
        {
            progressMock(current, total);
            if (current == 1) cts.Cancel();
        }

        var filePath = Path.Combine(FolderPath, "test.pdf");
        var images = new[] { CreateScannedImage(), CreateScannedImage(), CreateScannedImage() };
        var result = await _exporter.Export(filePath, images, ocrParams: config.OcrParams,
            progress: new ProgressHandler(Progress, cts.Token));

        Assert.False(result);
        progressMock.Received()(0, 3);
        progressMock.Received()(1, 3);
        progressMock.ReceivedCallsCount(2);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportWithPageSize(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        var sourceImage = ImageContext.Create(850, 1100, ImagePixelFormat.RGB24);
        sourceImage.SetResolution(99.4f, 99.4f);
        using var image = ScanningContext.CreateProcessedImage(sourceImage, BitDepth.Color, false, -1, PageSize.Letter);

        await _exporter.Export(filePath, new[] { image });

        // If the resolution is close to the actual page size, we should correct to that page size with high precision
        PdfAsserts.AssertPageSize(PageSize.Letter, 3, filePath);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ExportWithMismatchedPageSize(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var filePath = Path.Combine(FolderPath, "test.pdf");
        var sourceImage = ImageContext.Create(850, 1100, ImagePixelFormat.RGB24);
        sourceImage.SetResolution(98, 98);
        using var image = ScanningContext.CreateProcessedImage(sourceImage, BitDepth.Color, false, -1, PageSize.Letter);

        await _exporter.Export(filePath, new[] { image });

        // If the page size is too far off, we should ignore it and go by the actual resolution (precision is less important here)
        PdfAsserts.AssertPageSize(new PageSize(8.7m, 11.2m, PageSizeUnit.Inch), 1, filePath);
    }
}