using NAPS2.Pdf;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Pdf;

public class PdfiumPdfRendererTests : ContextualTests
{
    [Fact]
    public void RenderPdfFromWord()
    {
        var path = CopyResourceToFile(PdfResources.word_generated_pdf, "test.pdf");

        var images = new PdfiumPdfRenderer().Render(ImageContext, path, PdfRenderSize.Default).ToList();

        Assert.Equal(2, images.Count);
        ImageAsserts.Similar(PdfResources.word_p1, images[0], ignoreResolution: true);
        ImageAsserts.Similar(PdfResources.word_p2, images[1], ignoreResolution: true);
    }

    [Fact]
    public void RenderPlainImagePdf()
    {
        var path = CopyResourceToFile(PdfResources.image_pdf, "test.pdf");

        var images = new PdfiumPdfRenderer().Render(ImageContext, path, PdfRenderSize.Default).ToList();
        
        Assert.Single(images);
        // This also verifies that the renderer gets the actual image dpi (72)
        ImageAsserts.Similar(ImageResources.dog, images[0]);
    }

    [Fact]
    public void RenderImageWithTextPdf()
    {
        var path = CopyResourceToFile(PdfResources.image_with_text_pdf, "test.pdf");

        var images = new PdfiumPdfRenderer().Render(ImageContext, path, PdfRenderSize.Default).ToList();

        Assert.Single(images);
        // This also verifies that the renderer gets the actual image dpi (72)
        ImageAsserts.Similar(ImageResources.ocr_test, images[0], ignoreResolution: true);
    }

    [Fact]
    public void RenderCmykImagePdf()
    {
        var path = CopyResourceToFile(PdfResources.image_pdf_cmyk, "test.pdf");

        var images = new PdfiumPdfRenderer().Render(ImageContext, path, PdfRenderSize.Default).ToList();

        Assert.Single(images);
        // This also verifies that the renderer gets the actual image dpi (72)
        ImageAsserts.Similar(ImageResources.dog, images[0]);
    }

    [Fact]
    public void RenderBlackAndWhiteImagePdf()
    {
        var path = CopyResourceToFile(PdfResources.image_pdf_bw, "test.pdf");

        var images = new PdfiumPdfRenderer().Render(ImageContext, path, PdfRenderSize.Default).ToList();

        Assert.Single(images);
        // This also verifies that the renderer gets the actual image dpi (72)
        ImageAsserts.Similar(ImageResources.dog_bw, images[0]);
    }

    [Fact]
    public void RenderFormsAndAnnotations()
    {
        var path = CopyResourceToFile(PdfResources.filled_form_annotated, "test.pdf");

        var images = new PdfiumPdfRenderer().Render(ImageContext, path, PdfRenderSize.Default).ToList();

        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.filled_form_annotated, images[0]);
    }
}