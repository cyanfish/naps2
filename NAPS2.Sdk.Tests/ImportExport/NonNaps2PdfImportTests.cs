using NAPS2.Images.Gdi;
using NAPS2.ImportExport.Pdf;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport;

public class NonNaps2PdfImportTests : ContextualTests
{
    private readonly PdfSharpImporter _importer;
    private readonly PdfSharpExporter _exporter;
    private readonly string _importPath;
    private readonly string _exportPath;

    public NonNaps2PdfImportTests()
    {
        _importer = new PdfSharpImporter(ScanningContext);
        _exporter = new PdfSharpExporter(ScanningContext);
        _importPath = Path.Combine(FolderPath, "import.pdf");
        _exportPath = Path.Combine(FolderPath, "export.pdf");
        File.WriteAllBytes(_importPath, PdfData.word_generated_pdf);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task Import(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var images = await _importer.Import(_importPath).ToList();

        Assert.Equal(2, images.Count);
        ImageAsserts.Similar(PdfData.word_p1, ImageContext.Render(images[0]));
        ImageAsserts.Similar(PdfData.word_p2, ImageContext.Render(images[1]));
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ImportInsertExport(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var images = await _importer.Import(_importPath).ToList();
        Assert.Equal(2, images.Count);

        var toInsert = ScanningContext.CreateProcessedImage(new GdiImage(SharedData.color_image));
        var newImages = new List<ProcessedImage>
        {
            images[0],
            toInsert,
            images[1]
        };
        await _exporter.Export(_exportPath, newImages, new PdfExportParams());

        PdfAsserts.AssertImages(_exportPath, PdfData.word_p1, SharedData.color_image, PdfData.word_p2);
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
        ImageAsserts.Similar(PdfData.word_p1_rotated, ImageContext.Render(newImages[0]));
        ImageAsserts.Similar(PdfData.word_p2_bw, ImageContext.Render(newImages[1]));
        
        await _exporter.Export(_exportPath, newImages, new PdfExportParams());
        PdfAsserts.AssertImages(_exportPath, PdfData.word_p1_rotated, PdfData.word_p2_bw);
    }
}