using NAPS2.Images.Gdi;
using NAPS2.ImportExport.Pdf;
using NAPS2.Ocr;
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
        File.WriteAllBytes(_importPath, PdfResources.word_generated_pdf);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task Import(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var images = await _importer.Import(_importPath).ToList();

        Assert.Equal(2, images.Count);
        ImageAsserts.Similar(PdfResources.word_p1, ImageContext.Render(images[0]));
        ImageAsserts.Similar(PdfResources.word_p2, ImageContext.Render(images[1]));
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ImportInsertExport(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var images = await _importer.Import(_importPath).ToList();
        Assert.Equal(2, images.Count);

        var toInsert = ScanningContext.CreateProcessedImage(new GdiImage(ImageResources.color_image));
        var newImages = new List<ProcessedImage>
        {
            images[0],
            toInsert,
            images[1]
        };
        await _exporter.Export(_exportPath, newImages, new PdfExportParams());

        PdfAsserts.AssertImages(_exportPath, PdfResources.word_p1, ImageResources.color_image, PdfResources.word_p2);
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
        ImageAsserts.Similar(PdfResources.word_p1_rotated, ImageContext.Render(newImages[0]));
        ImageAsserts.Similar(PdfResources.word_p2_bw, ImageContext.Render(newImages[1]));

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
        ImageAsserts.Similar(PdfResources.word_patcht_p1, ImageContext.Render(imagesForOcr[0]));

        var allImages = images.Concat(imagesForOcr).ToList();

        await _exporter.Export(_exportPath, allImages, new PdfExportParams(), new OcrParams("eng", OcrMode.Fast, 0));
        PdfAsserts.AssertImages(_exportPath, PdfResources.word_p1, PdfResources.word_p2, PdfResources.word_patcht_p1);
        PdfAsserts.AssertContainsTextOnce("Page one.", _exportPath);
        PdfAsserts.AssertContainsTextOnce("Sized for printing unscaled", _exportPath);
    }
    
    // TODO: Add test coverage for NAPS2-generated pdf import (either in another file or reorg import/export test classes), as well as for exporting various image types.
    // PdfSharpImporterTests, PdfSharpExporterTests, PdfImportExportTests?
    // Or maybe the ImportExport tests can just be part of PdfSharpExporterTests.
    // Or maybe forget that. Just create a "Pdf" subfolder with more descriptive test classes. e.g. PdfATests, PdfImportTests, PdfExportTests, PdfImportExportTests, PdfiumExportTests.
    // Also PdfiumPdfRendererTests should move.
}