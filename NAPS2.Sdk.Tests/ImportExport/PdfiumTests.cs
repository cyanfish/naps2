using NAPS2.ImportExport.Pdf;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport;

public class PdfiumTests : ContextualTests
{
    [Fact]
    public void RenderPdfFromWord()
    {
        var path = Path.Combine(FolderPath, "word.pdf");
        File.WriteAllBytes(path, PdfiumTestsData.word);

        var images = new PdfiumPdfRenderer().Render(ImageContext, path, 300).ToList();

        Assert.Equal(2, images.Count);
        ImageAsserts.Similar(PdfiumTestsData.word_p1, images[0]);
        ImageAsserts.Similar(PdfiumTestsData.word_p2, images[1]);
    }
}