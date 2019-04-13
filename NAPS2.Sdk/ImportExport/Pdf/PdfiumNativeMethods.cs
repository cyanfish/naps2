using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using NAPS2.Platform;
using NAPS2.Util;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfiumNativeMethods
    {
        public const int FPDFBitmap_BGR = 2;
        public const int FPDFBitmap_BGRA = 4;

        public const int FPDF_PRINTING = 0x800;
        public const int FPDF_REVERSE_BYTE_ORDER = 0x10;

        static PdfiumNativeMethods()
        {
            if (PlatformCompat.System.CanUseWin32)
            {
                string libDir = Environment.Is64BitProcess ? "_win64" : "_win32";
                var location = Assembly.GetExecutingAssembly().Location;
                var coreDllDir = System.IO.Path.GetDirectoryName(location);
                if (coreDllDir != null)
                {
                    Win32.SetDllDirectory(System.IO.Path.Combine(coreDllDir, libDir));
                }
            }
        }

        [DllImport("pdfium.dll")]
        public static extern IntPtr FPDFBitmap_Create(int width, int height, int alpha);

        [DllImport("pdfium.dll")]
        public static extern IntPtr FPDFBitmap_CreateEx(int width, int height, int format, IntPtr firstScan, int stride);

        [DllImport("pdfium.dll")]
        public static extern void FPDFBitmap_Destroy(IntPtr bitmap);

        [DllImport("pdfium.dll")]
        public static extern IntPtr FPDF_LoadDocument(string filePath, string password);

        [DllImport("pdfium.dll")]
        public static extern IntPtr FPDF_LoadMemDocument(IntPtr buffer, int size, string password);

        [DllImport("pdfium.dll")]
        public static extern int FPDF_GetPageCount(IntPtr document);

        [DllImport("pdfium.dll")]
        public static extern IntPtr FPDF_LoadPage(IntPtr document, int pageIndex);

        [DllImport("pdfium.dll")]
        public static extern double FPDF_GetPageWidth(IntPtr page);

        [DllImport("pdfium.dll")]
        public static extern double FPDF_GetPageHeight(IntPtr page);

        [DllImport("pdfium.dll")]
        public static extern void FPDF_RenderPageBitmap(IntPtr bitmap, IntPtr page, int startX, int startY, int sizeX, int sizeY, int rotate, int flags);

    }
}
