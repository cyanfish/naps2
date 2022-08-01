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
            yield return new PdfiumBitmapFactory(imageContext).RenderPageToNewImage(page, defaultDpi);
        }
    }
}