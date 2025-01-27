using NAPS2.ImportExport;
using NAPS2.Pdf.Pdfium;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport;

public class FileImporterTests : ContextualTests
{
    private readonly FileImporter _fileImporter;

    public FileImporterTests()
    {
        _fileImporter = new FileImporter(ScanningContext);
        SetUpFileStorage();
    }

    [Fact]
    public async Task ImportPngImage()
    {
        var filePath = CopyResourceToFile(ImageResources.skewed_bw, "image.png");

        var source = _fileImporter.Import(filePath, new ImportParams());
        var result = await source.ToListAsync();

        Assert.Single(result);
        var storage = Assert.IsType<ImageFileStorage>(result[0].Storage);
        Assert.Equal(".png", Path.GetExtension(storage.FullPath));
    }

    [Fact]
    public async Task ImportPngStream()
    {
        var fileStream = new MemoryStream(ImageResources.skewed_bw);

        var source = _fileImporter.Import(fileStream, new ImportParams());
        var result = await source.ToListAsync();

        Assert.Single(result);
        var storage = Assert.IsType<ImageFileStorage>(result[0].Storage);
        Assert.Equal(".png", Path.GetExtension(storage.FullPath));
    }

    [Fact]
    public async Task ImportJpegImage()
    {
        var filePath = CopyResourceToFile(ImageResources.dog, "image.jpg");

        var source = _fileImporter.Import(filePath, new ImportParams());
        var result = await source.ToListAsync();

        Assert.Single(result);
        var storage = Assert.IsType<ImageFileStorage>(result[0].Storage);
        Assert.Equal(".jpg", Path.GetExtension(storage.FullPath));
    }

    [Fact]
    public async Task ImportJpegStream()
    {
        var fileStream = new MemoryStream(ImageResources.dog);

        var source = _fileImporter.Import(fileStream, new ImportParams());
        var result = await source.ToListAsync();

        Assert.Single(result);
        var storage = Assert.IsType<ImageFileStorage>(result[0].Storage);
        Assert.Equal(".jpg", Path.GetExtension(storage.FullPath));
    }

    [Fact]
    public async Task ImportPdfFile()
    {
        var filePath = CopyResourceToFile(PdfResources.word_patcht_pdf, "word.pdf");

        var source = _fileImporter.Import(filePath, new ImportParams());
        var result = await source.ToListAsync();

        Assert.Single(result);
        var storage = Assert.IsType<ImageFileStorage>(result[0].Storage);
        Assert.Equal(".pdf", Path.GetExtension(storage.FullPath));
    }

    [Fact]
    public async Task ImportPdfStream()
    {
        var fileStream = new MemoryStream(PdfResources.word_patcht_pdf);

        var source = _fileImporter.Import(fileStream, new ImportParams());
        var result = await source.ToListAsync();

        Assert.Single(result);
        var storage = Assert.IsType<ImageFileStorage>(result[0].Storage);
        Assert.Equal(".pdf", Path.GetExtension(storage.FullPath));
    }

    [Fact]
    public async Task ImportZipFile()
    {
        var filePath = CopyResourceToFile(BinaryResources.animals, "animals.zip");

        var source = _fileImporter.Import(filePath, new ImportParams());
        var result = await source.ToListAsync();

        Assert.Equal(2, result.Count);
        var storage = Assert.IsType<ImageFileStorage>(result[0].Storage);
        Assert.Equal(".jpg", Path.GetExtension(storage.FullPath));
    }

    [Fact]
    public async Task ImportZipStream()
    {
        var fileStream = new MemoryStream(BinaryResources.animals);

        var source = _fileImporter.Import(fileStream, new ImportParams());
        var result = await source.ToListAsync();

        Assert.Equal(2, result.Count);
        var storage = Assert.IsType<ImageFileStorage>(result[0].Storage);
        Assert.Equal(".jpg", Path.GetExtension(storage.FullPath));
    }

    [Fact]
    public async Task ImportUnsupportedFile()
    {
        var filePath = CopyResourceToFile(BinaryResources.testcert, "something.crt");

        await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await _fileImporter.Import(filePath, new ImportParams()).ToListAsync());
    }

    [Fact]
    public async Task ImportUnsupportedStream()
    {
        var fileStream = new MemoryStream(BinaryResources.testcert);

        await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await _fileImporter.Import(fileStream, new ImportParams()).ToListAsync());
    }

    [Fact]
    public async Task ImportImageWithPdfExtension()
    {
        var filePath = CopyResourceToFile(ImageResources.dog, "image.pdf");

        await Assert.ThrowsAsync<PdfiumException>(async () =>
            await _fileImporter.Import(filePath, new ImportParams()).ToListAsync());
    }

    [Fact]
    public async Task ImportPdfWithImageExtension()
    {
        var filePath = CopyResourceToFile(PdfResources.word_patcht_pdf, "pdf.jpg");

        await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await _fileImporter.Import(filePath, new ImportParams()).ToListAsync());
    }
}