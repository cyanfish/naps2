using NAPS2.ImportExport.Pdf;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport;

public class PdfiumTests : ContextualTests
{
    [Fact]
    public void RenderPdfFromWord()
    {
        var path = Path.Combine(FolderPath, "test.pdf");
        File.WriteAllBytes(path, PdfData.word_generated_pdf);

        var images = new PdfiumPdfRenderer().Render(ImageContext, path, 300).ToList();

        Assert.Equal(2, images.Count);
        ImageAsserts.Similar(PdfData.word_p1, images[0]);
        ImageAsserts.Similar(PdfData.word_p2, images[1]);
    }

    [Fact]
    public void RenderPlainImagePdf()
    {
        var path = Path.Combine(FolderPath, "test.pdf");
        File.WriteAllBytes(path, PdfData.image_pdf);

        var images = new PdfiumPdfRenderer().Render(ImageContext, path, 300).ToList();
        
        Assert.Single(images);
        // This also verifies that the renderer gets the actual image dpi (72) 
        ImageAsserts.Similar(SharedData.color_image, images[0]);
    }

    // TODO: Implement
    [Fact(Skip="Not implemented")]
    public void RenderImageWithTextPdf()
    {
        var path = Path.Combine(FolderPath, "test.pdf");
        File.WriteAllBytes(path, PdfData.image_pdf);

        var images = new PdfiumPdfRenderer().Render(ImageContext, path, 300).ToList();

        Assert.Single(images);
        // This also verifies that the renderer gets the actual image dpi (72)
        ImageAsserts.Similar(SharedData.ocr_test, images[0]);
    }
}