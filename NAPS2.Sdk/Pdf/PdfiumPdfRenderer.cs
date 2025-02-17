using NAPS2.Images.Bitwise;
using NAPS2.Pdf.Pdfium;

namespace NAPS2.Pdf;

internal class PdfiumPdfRenderer : IPdfRenderer
{
    public IEnumerable<IMemoryImage> Render(ImageContext imageContext, string path, PdfRenderSize renderSize,
        string? password = null)
    {
        // Pdfium is not thread-safe
        lock (PdfiumNativeLibrary.Instance)
        {
            using var doc = PdfDocument.Load(path, password);
            foreach (var memoryImage in RenderDocument(imageContext, renderSize, doc))
            {
                yield return memoryImage;
            }
        }
    }

    public IEnumerable<IMemoryImage> Render(ImageContext imageContext, byte[] buffer, int length,
        PdfRenderSize renderSize, string? password = null)
    {
        // Pdfium is not thread-safe
        lock (PdfiumNativeLibrary.Instance)
        {
            using var doc = PdfDocument.Load(buffer, length, password);
            foreach (var memoryImage in RenderDocument(imageContext, renderSize, doc))
            {
                yield return memoryImage;
            }
        }
    }

    public IMemoryImage RenderPage(ImageContext imageContext, string path, PdfRenderSize renderSize,
        int pageIndex = 0, string? password = null)
    {
        // Pdfium is not thread-safe
        lock (PdfiumNativeLibrary.Instance)
        {
            using var doc = PdfDocument.Load(path, password);
            return RenderDocument(imageContext, renderSize, doc, pageIndex).Single();
        }
    }

    public IMemoryImage RenderPage(ImageContext imageContext, byte[] buffer, int length, PdfRenderSize renderSize,
        int pageIndex, string? password = null)
    {
        // Pdfium is not thread-safe
        lock (PdfiumNativeLibrary.Instance)
        {
            using var doc = PdfDocument.Load(buffer, length, password);
            return RenderDocument(imageContext, renderSize, doc, pageIndex).Single();
        }
    }

    private IEnumerable<IMemoryImage> RenderDocument(ImageContext imageContext, PdfRenderSize renderSize,
        PdfDocument doc, int? pageIndex = null)
    {
        var pageCount = doc.PageCount;
        int start = pageIndex ?? 0;
        int end = pageIndex ?? pageCount - 1;
        for (int i = start; i <= end; i++)
        {
            using var page = doc.GetPage(i);

            if (!NoExtraction)
            {
                var image = PdfiumImageExtractor.GetSingleImage(imageContext, page, true);
                if (image != null)
                {
                    yield return image;
                    continue;
                }
            }
            yield return RenderPageToNewImage(imageContext, page, i, renderSize);
        }
    }

    public IMemoryImage RenderPageToNewImage(ImageContext imageContext, PdfPage page, int pageIndex,
        PdfRenderSize renderSize)
    {
        var widthInInches = page.Width / 72;
        var heightInInches = page.Height / 72;

        var (widthInPx, heightInPx, xDpi, yDpi) = renderSize.GetDimensions(widthInInches, heightInInches, pageIndex);

        var bitmap = imageContext.Create(widthInPx, heightInPx, ImagePixelFormat.RGB24);
        bitmap.SetResolution(xDpi, yDpi);

        using var pdfiumBitmap =
            PdfBitmap.Create(widthInPx, heightInPx, PdfiumNativeLibrary.FPDFBitmap_BGR);
        pdfiumBitmap.FillRect(0, 0, widthInPx, heightInPx, PdfBitmap.WHITE);
        pdfiumBitmap.RenderPage(page, 0, 0, widthInPx, heightInPx);

        // We need to draw forms so that filled forms and signatures are visible
        using var formEnv = page.Document.CreateFormEnv();
        formEnv.DrawForms(pdfiumBitmap, page);

        var pixelInfo = new PixelInfo(pdfiumBitmap.Width, pdfiumBitmap.Height, SubPixelType.Bgr, pdfiumBitmap.Stride);
        new CopyBitwiseImageOp().Perform(pdfiumBitmap.Buffer, pixelInfo, bitmap);

        return bitmap;
    }

    /// <summary>
    /// If true, full Pdfium rendering will always be used instead of the more efficient (and resolution-preserving)
    /// direct image extraction. This can be set for tests to ensure that any incompatibilities with the encoded image
    /// are identified.
    /// </summary>
    public bool NoExtraction { get; set; }
}