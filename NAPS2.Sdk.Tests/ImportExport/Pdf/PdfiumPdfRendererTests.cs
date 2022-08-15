using NAPS2.ImportExport.Pdf;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport.Pdf;

public class PdfiumPdfRendererTests : ContextualTests
{
    [Fact]
    public void RenderPdfFromWord()
    {
        var path = Path.Combine(FolderPath, "test.pdf");
        File.WriteAllBytes(path, PdfResources.word_generated_pdf);

        var images = new PdfiumPdfRenderer().Render(ImageContext, path, PdfRenderSize.Default).ToList();

        Assert.Equal(2, images.Count);
        ImageAsserts.Similar(PdfResources.word_p1, images[0], ignoreResolution: true);
        ImageAsserts.Similar(PdfResources.word_p2, images[1], ignoreResolution: true);
    }

    [Fact]
    public void RenderPlainImagePdf()
    {
        var path = Path.Combine(FolderPath, "test.pdf");
        File.WriteAllBytes(path, PdfResources.image_pdf);

        var images = new PdfiumPdfRenderer().Render(ImageContext, path, PdfRenderSize.Default).ToList();
        
        Assert.Single(images);
        // This also verifies that the renderer gets the actual image dpi (72) 
        ImageAsserts.Similar(ImageResources.color_image, images[0]);
    }

    [Fact]
    public void RenderImageWithTextPdf()
    {
        var path = Path.Combine(FolderPath, "test.pdf");
        File.WriteAllBytes(path, PdfResources.image_with_text_pdf);

        var images = new PdfiumPdfRenderer().Render(ImageContext, path, PdfRenderSize.Default).ToList();

        Assert.Single(images);
        // This also verifies that the renderer gets the actual image dpi (72)
        ImageAsserts.Similar(ImageResources.ocr_test, images[0], ignoreResolution: true);
    }
}