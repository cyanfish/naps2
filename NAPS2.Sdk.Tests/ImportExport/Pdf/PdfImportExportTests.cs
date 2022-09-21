using NAPS2.ImportExport.Pdf;
using NAPS2.Ocr;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport.Pdf;

public class PdfImportExportTests : ContextualTests
{
    private readonly PdfImporter _importer;
    private readonly PdfExporter _exporter;
    private readonly string _importPath;
    private readonly string _exportPath;

    public PdfImportExportTests()
    {
        _importer = new PdfImporter(ScanningContext);
        _exporter = new PdfExporter(ScanningContext);
        _importPath = CopyResourceToFile(PdfResources.word_generated_pdf, "import.pdf");
        _exportPath = Path.Combine(FolderPath, "export.pdf");
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ImportExport(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var images = await _importer.Import(_importPath).ToList();
        Assert.Equal(2, images.Count);
        await _exporter.Export(_exportPath, images, new PdfExportParams());

        PdfAsserts.AssertImages(_exportPath, PdfResources.word_p1, PdfResources.word_p2);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ImportInsertExport(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var images = await _importer.Import(_importPath).ToList();
        Assert.Equal(2, images.Count);

        var toInsert = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));
        var newImages = new List<ProcessedImage>
        {
            images[0],
            toInsert,
            images[1]
        };
        await _exporter.Export(_exportPath, newImages, new PdfExportParams());

        PdfAsserts.AssertImages(_exportPath, PdfResources.word_p1, ImageResources.dog, PdfResources.word_p2);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ImportTransformExport(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var images = await _importer.Import(_importPath).ToList();
        Assert.Equal(2, images.Count);

        var newImages = new List<ProcessedImage>
        {
            images[0].WithTransform(new RotationTransform(90)),
            images[1].WithTransform(new BlackWhiteTransform())
        };
        ImageAsserts.Similar(PdfResources.word_p1_rotated, newImages[0].Render(), ignoreResolution: true);
        ImageAsserts.Similar(PdfResources.word_p2_bw, newImages[1].Render(), ignoreResolution: true);

        await _exporter.Export(_exportPath, newImages, new PdfExportParams());
        PdfAsserts.AssertImages(_exportPath, PdfResources.word_p1_rotated, PdfResources.word_p2_bw);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ImportExportWithOcr(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);
        SetUpOcr();

        var importPathForOcr = Path.Combine(FolderPath, "import_ocr.pdf");
        File.WriteAllBytes(importPathForOcr, PdfResources.word_patcht_pdf);
        var images = await _importer.Import(_importPath).ToList();
        var imagesForOcr = await _importer.Import(importPathForOcr).ToList();

        Assert.Equal(2, images.Count);
        Assert.Single(imagesForOcr);
        ImageAsserts.Similar(PdfResources.word_patcht_p1, imagesForOcr[0].Render(), ignoreResolution: true);

        var allImages = images.Concat(imagesForOcr).ToList();

        await _exporter.Export(_exportPath, allImages, new PdfExportParams(), new OcrParams("eng", OcrMode.Fast, 0));
        PdfAsserts.AssertImages(_exportPath, PdfResources.word_p1, PdfResources.word_p2, PdfResources.word_patcht_p1);
        PdfAsserts.AssertContainsTextOnce("Page one.", _exportPath);
        PdfAsserts.AssertContainsTextOnce("Sized for printing unscaled", _exportPath);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ImportExportEncrypted(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var images = await _importer.Import(_importPath).ToList();
        Assert.Equal(2, images.Count);
        await _exporter.Export(_exportPath, images, new PdfExportParams
        {
            Encryption = new()
            {
                EncryptPdf = true,
                OwnerPassword = "hello",
                UserPassword = "world"
            }
        });

        PdfAsserts.AssertEncrypted(_exportPath, "hello", "world");
        PdfAsserts.AssertImages(_exportPath, "world", PdfResources.word_p1, PdfResources.word_p2);
    }
}