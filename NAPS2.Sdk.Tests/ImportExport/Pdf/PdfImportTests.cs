using NAPS2.ImportExport.Pdf;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport.Pdf;

public class PdfImportTests : ContextualTests
{
    private readonly PdfSharpImporter _importer;

    public PdfImportTests()
    {
        _importer = new PdfSharpImporter(ScanningContext);
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task Import(StorageConfig storageConfig)
    {
        storageConfig.Apply(this);

        var importPath = CopyResourceToFile(PdfResources.word_generated_pdf, "import.pdf");
        var images = await _importer.Import(importPath).ToList();

        Assert.Equal(2, images.Count);
        ImageAsserts.Similar(PdfResources.word_p1, ImageContext.Render(images[0]));
        ImageAsserts.Similar(PdfResources.word_p2, ImageContext.Render(images[1]));
    }
}