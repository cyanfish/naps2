using Moq;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Pdf;
using NAPS2.ImportExport.Pdf.Pdfium;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport.Pdf;

// TODO: MemoryStorage tests are a lot slower than FileStorage, why?
// TODO: Add an import test for 1bit png (not ccitt)
public class PdfImportTests : ContextualTests
{
    private readonly PdfImporter _importer;

    public PdfImportTests()
    {
        _importer = new PdfImporter(ScanningContext);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ImportNonNaps2Pdf(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var importPath = CopyResourceToFile(PdfResources.word_generated_pdf, "import.pdf");
        var images = await _importer.Import(importPath).ToList();

        Assert.Equal(2, images.Count);
        storageConfig.AssertPdfStorage(images[0].Storage);
        storageConfig.AssertPdfStorage(images[1].Storage);
        // TODO: Why is the expected resolution weird?
        ImageAsserts.Similar(PdfResources.word_p1, images[0].Render(), ignoreResolution: true);
        ImageAsserts.Similar(PdfResources.word_p2, images[1].Render(), ignoreResolution: true);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ImportNaps2Pdf(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var importPath = CopyResourceToFile(PdfResources.image_pdf, "import.pdf");
        var images = await _importer.Import(importPath).ToList();

        Assert.Single(images);
        storageConfig.AssertJpegStorage(images[0].Storage);
        ImageAsserts.Similar(ImageResources.dog, images[0].Render());
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ImportNaps2PngPdf(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var importPath = CopyResourceToFile(PdfResources.image_pdf_png, "import.pdf");
        var images = await _importer.Import(importPath).ToList();

        Assert.Single(images);
        storageConfig.AssertPngStorage(images[0].Storage);
        ImageAsserts.Similar(ImageResources.dog, images[0].Render());
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ImportNaps2BlackWhitePdf(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var importPath = CopyResourceToFile(PdfResources.image_pdf_bw, "import.pdf");
        var images = await _importer.Import(importPath).ToList();

        Assert.Single(images);
        storageConfig.AssertPngStorage(images[0].Storage);
        ImageAsserts.Similar(ImageResources.dog_bw, images[0].Render());
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ImportEncrypted(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var importPath = CopyResourceToFile(PdfResources.encrypted_pdf, "import.pdf");
        var images = await _importer.Import(importPath, new ImportParams { Password = "hello" }).ToList();

        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog, images[0].Render());
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task ImportEncryptedWithPasswordProvider(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var passwordProvider = new Mock<IPdfPasswordProvider>();
        var password = "hello";
        passwordProvider.Setup(x => x.ProvidePassword(It.IsAny<string>(), It.IsAny<int>(), out password)).Returns(true);
        var importer = new PdfImporter(ScanningContext, passwordProvider.Object);

        var importPath = CopyResourceToFile(PdfResources.encrypted_pdf, "import.pdf");
        var images = await importer.Import(importPath).ToList();

        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog, images[0].Render());
    }

    [Fact]
    public async Task ImportMissingFile()
    {
        var source = _importer.Import(Path.Combine(FolderPath, "missing.pdf"));
        await Assert.ThrowsAsync<FileNotFoundException>(async () => await source.ToList());
    }

    [Fact]
    public async Task ImportInUseFile()
    {
        var path = Path.Combine(FolderPath, "inuse.pdf");
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
        var source = _importer.Import(path);
        await Assert.ThrowsAsync<IOException>(async () => await source.ToList());
    }

    [Fact]
    public async Task ImportInvalidFile()
    {
        var path = CopyResourceToFile(BinaryResources.stock_dog, "notapdf.pdf");
        var source = _importer.Import(path);
        var ex = await Assert.ThrowsAsync<PdfiumException>(async () => await source.ToList());
        Assert.Equal(PdfiumErrorCode.InvalidFileFormat, ex.ErrorCode);
    }
}