using System.Runtime.InteropServices;
using NAPS2.ImportExport.Pdf.Pdfium;

namespace NAPS2.ImportExport.Pdf;

public class PdfiumPdfRenderer : IPdfRenderer
{
    public IEnumerable<IMemoryImage> Render(ImageContext imageContext, string path, float defaultDpi,
        string? password = null)
    {
        // Pdfium is not thread-safe
        lock (PdfiumNativeLibrary.Instance)
        {
            using var doc = PdfDocument.Load(path, password);
            foreach (var memoryImage in RenderDocument(imageContext, defaultDpi, doc))
            {
                yield return memoryImage;
            }
        }
    }

    public IEnumerable<IMemoryImage> Render(ImageContext imageContext, byte[] buffer, int length, float defaultDpi,
        string? password = null)
    {
        // Pdfium is not thread-safe
        lock (PdfiumNativeLibrary.Instance)
        {
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                using var doc = PdfDocument.Load(handle.AddrOfPinnedObject(), length, password);
                foreach (var memoryImage in RenderDocument(imageContext, defaultDpi, doc))
                {
                    yield return memoryImage;
                }
            }
            finally
            {
                handle.Free();
            }
        }
    }

    private IEnumerable<IMemoryImage> RenderDocument(ImageContext imageContext, float defaultDpi, PdfDocument doc)
    {
        var pageCount = doc.PageCount;
        for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            using var page = doc.GetPage(pageIndex);

            var image = PdfiumImageExtractor.GetSingleImage(imageContext, page);
            if (image != null)
            {
                yield return image;
                continue;
            }
            yield return RenderPageToNewImage(imageContext, page, defaultDpi);
        }
    }

    private static IMemoryImage RenderPageToNewImage(ImageContext imageContext, PdfPage page, float defaultDpi)
    {
        var widthInInches = page.Width / 72;
        var heightInInches = page.Height / 72;

        // Cap the resolution to 10k pixels in each dimension
        var dpi = defaultDpi;
        dpi = Math.Min(dpi, 10000 / heightInInches);
        dpi = Math.Min(dpi, 10000 / widthInInches);

        int widthInPx = (int) Math.Round(widthInInches * dpi);
        int heightInPx = (int) Math.Round(heightInInches * dpi);

        var bitmap = imageContext.Create(widthInPx, heightInPx, ImagePixelFormat.RGB24);
        bitmap.SetResolution(dpi, dpi);
        using var bitmapData = bitmap.Lock(LockMode.ReadWrite, out var scan0, out var stride);
        using var pdfiumBitmap = PdfBitmap.CreateFromPointerBgr(widthInPx, heightInPx, scan0, stride);
        pdfiumBitmap.FillRect(0, 0, widthInPx, heightInPx, PdfBitmap.WHITE);
        pdfiumBitmap.RenderPage(page, 0, 0, widthInPx, heightInPx);
        return bitmap;
    }
}