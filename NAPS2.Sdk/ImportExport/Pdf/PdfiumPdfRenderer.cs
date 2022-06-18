namespace NAPS2.ImportExport.Pdf;

public class PdfiumPdfRenderer : IPdfRenderer
{
    private const int RENDER_FLAGS = PdfiumNativeLibrary.FPDF_PRINTING;
    private const uint COLOR_WHITE = uint.MaxValue;

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
            var doc = nativeLib.FPDF_LoadDocument(path, null);
            try
            {
                var pageCount = nativeLib.FPDF_GetPageCount(doc);
                for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
                {
                    var page = nativeLib.FPDF_LoadPage(doc, pageIndex);
                    try
                    {
                        var widthInInches = nativeLib.FPDF_GetPageWidth(page) / 72;
                        var heightInInches = nativeLib.FPDF_GetPageHeight(page) / 72;

                        // Cap the resolution to 10k pixels in each dimension
                        dpi = Math.Min(dpi, (float) (10000 / heightInInches));
                        dpi = Math.Min(dpi, (float) (10000 / widthInInches));

                        int widthInPx = (int) Math.Round(widthInInches * dpi);
                        int heightInPx = (int) Math.Round(heightInInches * dpi);

                        var bitmap = _imageContext.Create(widthInPx, heightInPx, ImagePixelFormat.RGB24);
                        bitmap.SetResolution(dpi, dpi);
                        var bitmapData = bitmap.Lock(LockMode.ReadWrite, out var scan0, out var stride);
                        try
                        {
                            var pdfiumBitmap = nativeLib.FPDFBitmap_CreateEx(widthInPx, heightInPx,
                                PdfiumNativeLibrary.FPDFBitmap_BGR, scan0, stride);
                            try
                            {
                                nativeLib.FPDFBitmap_FillRect(pdfiumBitmap, 0, 0, widthInPx, heightInPx, COLOR_WHITE);
                                nativeLib.FPDF_RenderPageBitmap(pdfiumBitmap, page, 0, 0, widthInPx, heightInPx, 0,
                                    RENDER_FLAGS);
                                yield return bitmap;
                            }
                            finally
                            {
                                nativeLib.FPDFBitmap_Destroy(pdfiumBitmap);
                            }
                        }
                        finally
                        {
                            bitmap.Unlock(bitmapData);
                        }
                    }
                    finally
                    {
                        nativeLib.FPDF_ClosePage(page);
                    }
                }
            }
            finally
            {
                nativeLib.FPDF_CloseDocument(doc);
            }
        }
    }
}