using NAPS2.ImportExport.Pdf.Pdfium;

namespace NAPS2.ImportExport.Pdf;

public class PdfiumPdfRenderer : IPdfRenderer
{
    private readonly ImageContext _imageContext;

    public PdfiumPdfRenderer(ImageContext imageContext)
    {
        _imageContext = imageContext;
    }

    public IEnumerable<IMemoryImage> Render(string path, float dpi)
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

                // Cap the resolution to 10k pixels in each dimension
                dpi = Math.Min(dpi, (float) (10000 / heightInInches));
                dpi = Math.Min(dpi, (float) (10000 / widthInInches));

                int widthInPx = (int) Math.Round(widthInInches * dpi);
                int heightInPx = (int) Math.Round(heightInInches * dpi);

                var bitmap = _imageContext.Create(widthInPx, heightInPx, ImagePixelFormat.RGB24);
                bitmap.SetResolution(dpi, dpi);
                using var bitmapData = bitmap.Lock(LockMode.ReadWrite, out var scan0, out var stride);
                using var pdfiumBitmap = PdfBitmap.CreateFromPointerBgr(widthInPx, heightInPx, scan0, stride);
                pdfiumBitmap.FillRect(0, 0, widthInPx, heightInPx, PdfBitmap.WHITE);
                pdfiumBitmap.RenderPage(page, 0, 0, widthInPx, heightInPx);
                yield return bitmap;
            }
        }
    }
}