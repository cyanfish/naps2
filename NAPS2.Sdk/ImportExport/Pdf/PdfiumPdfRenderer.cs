using System.Runtime.InteropServices;
using NAPS2.Images.Bitwise;
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

    public unsafe IMemoryImage RenderPageToNewImage(ImageContext imageContext, PdfPage page, float defaultDpi)
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
        bitmap.SetResolution((int) Math.Round(dpi), (int) Math.Round(dpi));

        // As Pdfium only supports BGR, to be general we need to store it in an intermediate buffer,
        // then use a copy operation to get the data to our output image (which might be BGR or RGB).
        // TODO: Consider bypassing this by supporting BGR on mac etc.
        var pixelInfo = new PixelInfo(widthInPx, heightInPx, SubPixelType.Bgr);
        var buffer = new byte[pixelInfo.Length];
        fixed (byte* ptr = buffer)
        {
            using var pdfiumBitmap =
                PdfBitmap.CreateFromPointerBgr(widthInPx, heightInPx, (IntPtr) ptr, pixelInfo.Stride);
            pdfiumBitmap.FillRect(0, 0, widthInPx, heightInPx, PdfBitmap.WHITE);
            pdfiumBitmap.RenderPage(page, 0, 0, widthInPx, heightInPx);
            
            new CopyBitwiseImageOp().Perform(buffer, pixelInfo, bitmap);
                
            return bitmap;
        }
    }
}