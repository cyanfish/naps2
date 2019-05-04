using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NAPS2.Images.Storage;

namespace NAPS2.ImportExport.Pdf
{
    public static class PdfiumPdfRenderer
    {
        private const int RENDER_FLAGS = PdfiumNativeLibrary.FPDF_PRINTING;
        private const uint COLOR_WHITE = uint.MaxValue;

        private static readonly PdfiumNativeLibrary NativeLib;

        static PdfiumPdfRenderer()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyFolder = System.IO.Path.GetDirectoryName(assemblyLocation);
            NativeLib = new PdfiumNativeLibrary(assemblyFolder, "_win32/pdfium.dll", "_win64/pdfium.dll", "_linux/libpdfium.so", "_osx/libpdfium.dylib");
            NativeLib.FPDF_InitLibrary();
        }

        public static IEnumerable<IImage> Render(string path, float dpi)
        {
            // Pdfium is not thread-safe
            lock (NativeLib)
            {
                var doc = NativeLib.FPDF_LoadDocument(path, null);
                try
                {
                    var pageCount = NativeLib.FPDF_GetPageCount(doc);
                    for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
                    {
                        var page = NativeLib.FPDF_LoadPage(doc, pageIndex);
                        try
                        {
                            var widthInInches = NativeLib.FPDF_GetPageWidth(page) / 72;
                            var heightInInches = NativeLib.FPDF_GetPageHeight(page) / 72;

                            // Cap the resolution to 10k pixels in each dimension
                            dpi = Math.Min(dpi, (float) (10000 / heightInInches));
                            dpi = Math.Min(dpi, (float) (10000 / widthInInches));

                            int widthInPx = (int) Math.Round(widthInInches * dpi);
                            int heightInPx = (int) Math.Round(heightInInches * dpi);

                            var bitmap = StorageManager.ImageFactory.FromDimensions(widthInPx, heightInPx, StoragePixelFormat.RGB24);
                            bitmap.SetResolution(dpi, dpi);
                            var bitmapData = bitmap.Lock(LockMode.ReadWrite, out var scan0, out var stride);
                            try
                            {
                                var pdfiumBitmap = NativeLib.FPDFBitmap_CreateEx(widthInPx, heightInPx, PdfiumNativeLibrary.FPDFBitmap_BGR, scan0, stride);
                                try
                                {
                                    NativeLib.FPDFBitmap_FillRect(pdfiumBitmap, 0, 0, widthInPx, heightInPx, COLOR_WHITE);
                                    NativeLib.FPDF_RenderPageBitmap(pdfiumBitmap, page, 0, 0, widthInPx, heightInPx, 0, RENDER_FLAGS);
                                    yield return bitmap;
                                }
                                finally
                                {
                                    NativeLib.FPDFBitmap_Destroy(pdfiumBitmap);
                                }
                            }
                            finally
                            {
                                bitmap.Unlock(bitmapData);
                            }
                        }
                        finally
                        {
                            NativeLib.FPDF_ClosePage(page);
                        }
                    }
                }
                finally
                {
                    NativeLib.FPDF_CloseDocument(doc);
                }
            }
        }
    }
}
