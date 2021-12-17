using System.IO;
using System.Linq;
using NAPS2.Images.Storage;
using NAPS2.ImportExport.Pdf;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport;

public class PdfiumTests : ContextualTexts
{
    [Fact]
    public void RenderPdfFromWord()
    {
        var path = Path.Combine(FolderPath, "word.pdf");
        File.WriteAllBytes(path, PdfiumTestsData.word);

        IImage expectedPage1 = new GdiImage(PdfiumTestsData.word_p1);
        IImage expectedPage2 = new GdiImage(PdfiumTestsData.word_p2);

        var images = new PdfiumPdfRenderer(ImageContext).Render(path, 300).ToList();

        Assert.Equal(2, images.Count);
        ImageAsserts.Similar(images[0], expectedPage1, 3);
        ImageAsserts.Similar(images[1], expectedPage2, 3);
    }
}