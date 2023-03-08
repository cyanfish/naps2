using NAPS2.Pdf;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;
using Xunit.Abstractions;

namespace NAPS2.Sdk.Tests.ImportExport.Pdf;

public class PdfImportExportTests : ContextualTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    private readonly PdfImporter _importer;
    private readonly PdfExporter _exporter;
    private readonly string _importPath;
    private readonly string _exportPath;

    public PdfImportExportTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _importer = new PdfImporter(ScanningContext);
        _exporter = new PdfExporter(ScanningContext);
        _importPath = CopyResourceToFile(PdfResources.word_generated_pdf, "import.pdf");
        _exportPath = Path.Combine(FolderPath, "export.pdf");
    }

    [Theory]
    [ClassData(typeof(OcrTestData))]
    public async Task ImportExport(OcrTestConfig config)
    {
        config.StorageConfig.Apply(this);

        var images = await _importer.Import(_importPath).ToListAsync();
        Assert.Equal(2, images.Count);
        await _exporter.Export(_exportPath, images, new PdfExportParams(), config.OcrParams);

        PdfAsserts.AssertImages(_exportPath, PdfResources.word_p1, PdfResources.word_p2);
    }

    [Theory]
    [ClassData(typeof(OcrTestData))]
    public async Task ImportInsertExport(OcrTestConfig config)
    {
        config.StorageConfig.Apply(this);

        var images = await _importer.Import(_importPath).ToListAsync();
        Assert.Equal(2, images.Count);

        var toInsert = ScanningContext.CreateProcessedImage(LoadImage(ImageResources.dog));
        var newImages = new List<ProcessedImage>
        {
            images[0],
            toInsert,
            images[1]
        };
        await _exporter.Export(_exportPath, newImages, new PdfExportParams(), config.OcrParams);

        PdfAsserts.AssertImages(_exportPath, PdfResources.word_p1, ImageResources.dog, PdfResources.word_p2);
    }

    [Theory]
    [ClassData(typeof(OcrTestData))]
    public async Task ImportTransformExport(OcrTestConfig config)
    {
        config.StorageConfig.Apply(this);

        var images = await _importer.Import(_importPath).ToListAsync();
        Assert.Equal(2, images.Count);

        var newImages = new List<ProcessedImage>
        {
            images[0].WithTransform(new RotationTransform(90)),
            images[1].WithTransform(new BlackWhiteTransform())
        };
        ImageAsserts.Similar(PdfResources.word_p1_rotated, newImages[0], ignoreResolution: true);
        ImageAsserts.Similar(PdfResources.word_p2_bw, newImages[1], ignoreResolution: true);

        await _exporter.Export(_exportPath, newImages, new PdfExportParams(), config.OcrParams);
        PdfAsserts.AssertImages(_exportPath, PdfResources.word_p1_rotated, PdfResources.word_p2_bw);
    }

    [Theory]
    [ClassData(typeof(OcrTestData))]
    public async Task ImportExportOcrablePdf(OcrTestConfig config)
    {
        config.StorageConfig.Apply(this);
        SetUpFakeOcr(new()
        {
            { LoadImage(PdfResources.word_p1), "Page one."},
            { LoadImage(PdfResources.word_p2), "Page two."},
            { LoadImage(PdfResources.word_patcht_p1), "Sized for printing unscaled"}
        });

        var importPathForOcr = Path.Combine(FolderPath, "import_ocr.pdf");
        File.WriteAllBytes(importPathForOcr, PdfResources.word_patcht_pdf);
        var images = await _importer.Import(_importPath).ToListAsync();
        var imagesForOcr = await _importer.Import(importPathForOcr).ToListAsync();

        Assert.Equal(2, images.Count);
        Assert.Single(imagesForOcr);
        ImageAsserts.Similar(PdfResources.word_patcht_p1, imagesForOcr[0], ignoreResolution: true);

        var allImages = images.Concat(imagesForOcr).ToList();

        await _exporter.Export(_exportPath, allImages, new PdfExportParams(), config.OcrParams);
        PdfAsserts.AssertImages(_exportPath, PdfResources.word_p1, PdfResources.word_p2, PdfResources.word_patcht_p1);
        PdfAsserts.AssertContainsTextOnce("Page one.", _exportPath);
        if (config.OcrParams != null)
        {
            PdfAsserts.AssertContainsTextOnce("Sized for printing unscaled", _exportPath);
        }
        else
        {
            PdfAsserts.AssertDoesNotContainText("Sized for printing unscaled", _exportPath);
        }
    }

    [Theory]
    [ClassData(typeof(OcrTestData))]
    public async Task ImportExportEncrypted(OcrTestConfig config)
    {
        config.StorageConfig.Apply(this);

        var images = await _importer.Import(_importPath).ToListAsync();
        Assert.Equal(2, images.Count);
        await _exporter.Export(_exportPath, images, new PdfExportParams
        {
            Encryption = new()
            {
                EncryptPdf = true,
                OwnerPassword = "hello",
                UserPassword = "world"
            }
        }, config.OcrParams);

        PdfAsserts.AssertEncrypted(_exportPath, "hello", "world");
        PdfAsserts.AssertImages(_exportPath, "world", PdfResources.word_p1, PdfResources.word_p2);
    }

    [Theory]
    [ClassData(typeof(OcrTestData))]
    public async Task ImportVariousAndExport(OcrTestConfig config)
    {
        config.StorageConfig.Apply(this);

        var f1 = CopyResourceToFile(PdfResources.word_generated_pdf, "word.pdf");
        var f2 = CopyResourceToFile(PdfResources.word_patcht_pdf, "patcht.pdf");
        var f3 = CopyResourceToFile(PdfResources.image_pdf, "image.pdf");

        var images = new List<ProcessedImage>();

        images.AddRange(await _importer.Import(f1).ToListAsync());
        images.AddRange(await _importer.Import(f2).ToListAsync());
        images.AddRange(await _importer.Import(f3).ToListAsync());
        images.Add(ScanningContext.CreateProcessedImage(LoadImage(ImageResources.ocr_test)));
        Assert.Equal(5, images.Count);

        SetUpFakeOcr(new()
        {
            { LoadImage(PdfResources.word_p1), "Page one."},
            { LoadImage(PdfResources.word_p2), "Page two."},
            { LoadImage(PdfResources.word_patcht_p1), "Sized for printing unscaled"},
            { LoadImage(ImageResources.dog), ""},
            { LoadImage(ImageResources.ocr_test), "ADVERTISEMENT."},
        });
        await _exporter.Export(_exportPath, images, new PdfExportParams(), config.OcrParams);

        PdfAsserts.AssertImages(_exportPath,
            PdfResources.word_p1,
            PdfResources.word_p2,
            PdfResources.word_patcht_p1,
            ImageResources.dog,
            ImageResources.ocr_test);

        PdfAsserts.AssertContainsTextOnce("Page one.", _exportPath);
        PdfAsserts.AssertContainsTextOnce("Page two.", _exportPath);
        if (config.OcrParams != null)
        {
            PdfAsserts.AssertContainsTextOnce("ADVERTISEMENT.", _exportPath);
            PdfAsserts.AssertContainsTextOnce("Sized for printing unscaled", _exportPath);
        }
        else
        {
            PdfAsserts.AssertDoesNotContainText("ADVERTISEMENT.", _exportPath);
            PdfAsserts.AssertDoesNotContainText("Sized for printing unscaled", _exportPath);
        }
    }
}