using System;
using System.Collections.Generic;
using System.Reflection;
using NAPS2.Images.Storage;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfiumPdfRenderer : IPdfRenderer
    {
        private const int RENDER_FLAGS = PdfiumNativeLibrary.FPDF_PRINTING;
        private const uint COLOR_WHITE = uint.MaxValue;

        private static readonly Lazy<PdfiumNativeLibrary> LazyNativeLib = new Lazy<PdfiumNativeLibrary>(() =>
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyFolder = System.IO.Path.GetDirectoryName(assemblyLocation);
            var nativeLib = new PdfiumNativeLibrary(assemblyFolder, "_win32/pdfium.dll", "_win64/pdfium.dll", "_linux/libpdfium.so", "_osx/libpdfium.dylib");
            nativeLib.FPDF_InitLibrary();
            return nativeLib;
        });

        private readonly ImageContext imageContext;

        public PdfiumPdfRenderer(ImageContext imageContext)
        {
            this.imageContext = imageContext;
        }

        public IEnumerable<IImage> Render(string path, float dpi)
        {
            var nativeLib = LazyNativeLib.Value;
            // Pdfium is not thread-safe
            lock (nativeLib)
            {
                var doc = nativeLib.FPDF_LoadDocument(path, null);
                try
                {
                    return ProcessDoc(doc, dpi, nativeLib);
                }
                finally
                {
                    nativeLib.FPDF_CloseDocument(doc);
                }
            }
        }

        private IEnumerable<IImage> ProcessDoc(IntPtr doc, float dpi, PdfiumNativeLibrary nativeLib)
        {
            var pageCount = nativeLib.FPDF_GetPageCount(doc);
            for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
                var page = nativeLib.FPDF_LoadPage(doc, pageIndex);
                try
                {
                    yield return ProcessPage(page, dpi, nativeLib);
                }
                finally
                {
                    nativeLib.FPDF_ClosePage(page);
                }
            }
        }

        private IImage ProcessPage(IntPtr page, float dpi, PdfiumNativeLibrary nativeLib)
        {
            var widthInInches = nativeLib.FPDF_GetPageWidth(page) / 72;
            var heightInInches = nativeLib.FPDF_GetPageHeight(page) / 72;

            // Cap the resolution to 10k pixels in each dimension
            dpi = Math.Min(dpi, (float) (10000 / heightInInches));
            dpi = Math.Min(dpi, (float) (10000 / widthInInches));

            int widthInPx = (int) Math.Round(widthInInches * dpi);
            int heightInPx = (int) Math.Round(heightInInches * dpi);

            var bitmap = imageContext.ImageFactory.FromDimensions(widthInPx, heightInPx, StoragePixelFormat.RGB24);
            bitmap.SetResolution(dpi, dpi);
            var bitmapData = bitmap.Lock(LockMode.ReadWrite, out var scan0, out var stride);
            try
            {
                var pdfiumBitmap = nativeLib.FPDFBitmap_CreateEx(widthInPx, heightInPx,
                    PdfiumNativeLibrary.FPDFBitmap_BGR, scan0, stride);
                try
                {
                    nativeLib.FPDFBitmap_FillRect(pdfiumBitmap, 0, 0, widthInPx, heightInPx, COLOR_WHITE);
                    nativeLib.FPDF_RenderPageBitmap(pdfiumBitmap, page, 0, 0, widthInPx, heightInPx, 0, RENDER_FLAGS);
                    return bitmap;
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
    }
}
