using NAPS2.ImportExport.Pdf.Pdfium;

namespace NAPS2.ImportExport.Pdf;

public class PdfiumPdfRenderer : IPdfRenderer
{
    public IEnumerable<IMemoryImage> Render(ImageContext imageContext, string path, float defaultDpi)
    {
        var nativeLib = PdfiumNativeLibrary.LazyInstance.Value;

        // Pdfium is not thread-safe
        lock (nativeLib)
        {
            using var doc = PdfDocument.Load(path);
            var pageCount = doc.PageCount;
            for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
                using var page = doc.GetPage(pageIndex);
                var widthInInches = page.Width / 72;
                var heightInInches = page.Height / 72;

                var objectCount = page.ObjectCount;
                if (objectCount == 1)
                {
                    using var pageObj = page.GetObject(0);
                    if (pageObj.IsImage && pageObj.Matrix == PdfMatrix.FillPage(page.Width, page.Height))
                    {
                        // TODO: This should probably use GetRenderedBitmap, but that does add an alpha channel, and might be slower...
                        using var pdfBitmap = pageObj.GetBitmap();
                        if (pdfBitmap.Format is ImagePixelFormat.RGB24 or ImagePixelFormat.ARGB32)
                        {
                            yield return CopyPdfBitmapToNewImage(imageContext, pdfBitmap, page);
                            continue;
                        }
                    }
                }

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
                yield return bitmap;
            }
        }
    }

    private static unsafe IMemoryImage CopyPdfBitmapToNewImage(ImageContext imageContext, PdfBitmap pdfBitmap, PdfPage page)
    {
        var dstImage = imageContext.Create(pdfBitmap.Width, pdfBitmap.Height, pdfBitmap.Format);
        dstImage.SetResolution(dstImage.Width / page.Width * 72, dstImage.Height / page.Height * 72);
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