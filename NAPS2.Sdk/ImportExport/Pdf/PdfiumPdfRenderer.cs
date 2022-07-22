using System.Runtime.InteropServices;
using NAPS2.ImportExport.Pdf.Pdfium;

namespace NAPS2.ImportExport.Pdf;

public class PdfiumPdfRenderer : IPdfRenderer
{
    public IEnumerable<IMemoryImage> Render(ImageContext imageContext, string path, float defaultDpi)
    {
        // Pdfium is not thread-safe
        lock (PdfiumNativeLibrary.LazyInstance.Value)
        {
            using var doc = PdfDocument.Load(path);
            foreach (var memoryImage in RenderDocument(imageContext, defaultDpi, doc))
            {
                yield return memoryImage;
            }
        }
    }

    public IEnumerable<IMemoryImage> Render(ImageContext imageContext, byte[] buffer, int length, float defaultDpi)
    {
        // Pdfium is not thread-safe
        lock (PdfiumNativeLibrary.LazyInstance.Value)
        {
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                using var doc = PdfDocument.Load(handle.AddrOfPinnedObject(), length);
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

            using var imageObj = GetSingleImageObject(page);
            if (imageObj != null)
            {
                // TODO: This could be wrong if the image object has a mask, but GetRenderedBitmap does a re-encode which we don't really want
                // Ideally we would be do this conditionally based on the presence of a mask, if pdfium could provide us that info
                using var pdfBitmap = imageObj.GetBitmap();
                if (pdfBitmap.Format is ImagePixelFormat.RGB24 or ImagePixelFormat.ARGB32)
                {
                    yield return CopyPdfBitmapToNewImage(imageContext, pdfBitmap, page);
                    continue;
                }
            }


            yield return RenderPageToNewImage(imageContext, page, defaultDpi);
        }
    }

    private PdfPageObject? GetSingleImageObject(PdfPage page)
    {
        using var pageText = page.GetText();
        PdfPageObject? imageObject = null;
        var objectCount = page.ObjectCount;
        for (int i = 0; i < objectCount; i++)
        {
            // Note that objects from PdfPage.GetObject don't need to be disposed
            var pageObj = page.GetObject(i);
            // TODO: We could consider, even in cases where we don't have an exact matrix match etc., getting a smarter dpi estimate.
            // TODO: But it's not clear how well that will render if there's a subpixel offset.
            if (pageObj.IsImage && pageObj.Matrix == PdfMatrix.FillPage(page.Width, page.Height) && imageObject == null)
            {
                imageObject = pageObj;
            }
            else if (pageObj.IsText && (imageObject == null || IsInvisibleText(pageObj)))
            {
                // Skip invisible text or text that's underneath the image
                // TODO: This could be wrong if the image object has transparency
            }
            else
            {
                // We have other visible objects than a single image
                return null;
            }
        }
        return imageObject;
    }

    private bool IsInvisibleText(PdfPageObject pageObj)
    {
        return pageObj.TextRenderMode == TextRenderMode.Invisible
               || pageObj.TextRenderMode == TextRenderMode.Fill && pageObj.GetFillColor().a == 0
               || pageObj.TextRenderMode == TextRenderMode.Stroke && pageObj.GetStrokeColor().a == 0;
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

    private static unsafe IMemoryImage CopyPdfBitmapToNewImage(ImageContext imageContext, PdfBitmap pdfBitmap,
        PdfPage page)
    {
        var dstImage = imageContext.Create(pdfBitmap.Width, pdfBitmap.Height, pdfBitmap.Format);
        // TODO: We can get dpi from FPDFImageObj_GetImageMetadata?
        // TODO: Also, is there any world where we want to use GetImageData instead of GetBitmap?
        dstImage.SetResolution((int) Math.Round(dstImage.Width / page.Width * 72),
            (int) Math.Round(dstImage.Height / page.Height * 72));
        using var imageLock = dstImage.Lock(LockMode.ReadWrite, out var dstBuffer, out var dstStride);
        var srcBuffer = pdfBitmap.Buffer;
        var srcStride = pdfBitmap.Stride;
        for (int y = 0; y < dstImage.Height; y++)
        {
            IntPtr srcRow = srcBuffer + srcStride * y;
            IntPtr dstRow = dstBuffer + dstStride * y;
            Buffer.MemoryCopy(srcRow.ToPointer(), dstRow.ToPointer(), dstStride, Math.Min(srcStride, dstStride));
        }

        return dstImage;
    }
}