using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Dependencies;
using NAPS2.Images.Storage;
using NAPS2.Scan;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfiumPdfRenderer : IPdfRenderer
    {
        private const int RENDER_FLAGS = PdfiumNativeMethods.FPDF_PRINTING | PdfiumNativeMethods.FPDF_REVERSE_BYTE_ORDER;

        public IEnumerable<IImage> Render(string path)
        {
            // TODO: Maybe allow this to be configured
            int dpi = ScanDpi.Dpi300.ToIntDpi();

            var doc = PdfiumNativeMethods.FPDF_LoadDocument(path, null);
            var pageCount = PdfiumNativeMethods.FPDF_GetPageCount(doc);

            for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
                var page = PdfiumNativeMethods.FPDF_LoadPage(doc, pageIndex);
                var widthInInches = PdfiumNativeMethods.FPDF_GetPageWidth(page) * 72;
                var heightInInches = PdfiumNativeMethods.FPDF_GetPageWidth(page) * 72;
                int widthInPx = (int)Math.Round(widthInInches * dpi);
                int heightInPx = (int)Math.Round(heightInInches * dpi);

                // Cap the resolution to 10k pixels in each dimension
                dpi = Math.Min(dpi, (int)(10000 / heightInInches));
                dpi = Math.Min(dpi, (int)(10000 / widthInInches));

                var bitmap = StorageManager.ImageFactory.FromDimensions(widthInPx, heightInPx, StoragePixelFormat.RGB24);
                bitmap.SetResolution(dpi, dpi);
                var bitmapData = bitmap.Lock(LockMode.ReadWrite, out var scan0, out var stride);
                try
                {
                    var pdfiumBitmap = PdfiumNativeMethods.FPDFBitmap_CreateEx(widthInPx, heightInPx, PdfiumNativeMethods.FPDFBitmap_BGR, scan0, stride);
                    PdfiumNativeMethods.FPDF_RenderPageBitmap(pdfiumBitmap, page, 0, 0, widthInPx, heightInPx, 0, RENDER_FLAGS);
                    yield return bitmap;
                }
                finally
                {
                    bitmap.Unlock(bitmapData);
                }
            }
        }

        public void PromptToInstallIfNeeded(IComponentInstallPrompt componentInstallPrompt)
        {
        }

        public void ThrowIfCantRender()
        {
        }
    }
}
