using System.Reflection;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace NAPS2.ImportExport.Pdf;

public class PdfiumNativeLibrary : Unmanaged.NativeLibrary
{
    // TODO: Consider using Pdfium as a full replacement for PdfSharp.
    // The benefits would be more import compatibility + a solution to PdfSharp xplat. But there may be some
    // limitations, e.g. I don't see how you encrypt a document with Pdfium.
    //
    // API reference: https://pdfium.googlesource.com/pdfium/+/main/public/

    public static readonly Lazy<PdfiumNativeLibrary> LazyInstance = new(() =>
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyFolder = Path.GetDirectoryName(assemblyLocation);
        var libraryPath = Path.Combine(assemblyFolder, PlatformCompat.System.PdfiumLibraryPath);
        var nativeLib = new PdfiumNativeLibrary(libraryPath);
        nativeLib.FPDF_InitLibrary();
        return nativeLib;
    });

    public const int FPDFBitmap_BGR = 2;
    public const int FPDFBitmap_BGRA = 4;

    public const int FPDF_PRINTING = 0x800;
    public const int FPDF_REVERSE_BYTE_ORDER = 0x10;

    public PdfiumNativeLibrary(string libraryPath)
        : base(libraryPath)
    {
    }

    public delegate IntPtr FPDF_InitLibrary_delegate();

    public delegate IntPtr FPDFBitmap_Create_delegate(int width, int height, int alpha);

    public delegate IntPtr
        FPDFBitmap_CreateEx_delegate(int width, int height, int format, IntPtr firstScan, int stride);

    public delegate void FPDFBitmap_FillRect_delegate(IntPtr bitmap, int left, int top, int width, int height,
        uint color);

    public delegate void FPDFBitmap_Destroy_delegate(IntPtr bitmap);

    public delegate IntPtr FPDF_LoadDocument_delegate([MarshalAs(UnmanagedType.LPStr)] string filePath,
        [MarshalAs(UnmanagedType.LPStr)] string password);

    public delegate IntPtr FPDF_LoadMemDocument_delegate(IntPtr buffer, int size,
        [MarshalAs(UnmanagedType.LPStr)] string password);

    public delegate void FPDF_CloseDocument_delegate(IntPtr document);

    public delegate int FPDF_GetPageCount_delegate(IntPtr document);

    public delegate IntPtr FPDF_LoadPage_delegate(IntPtr document, int pageIndex);

    public delegate void FPDF_ClosePage_delegate(IntPtr page);

    public delegate double FPDF_GetPageWidth_delegate(IntPtr page);

    public delegate double FPDF_GetPageHeight_delegate(IntPtr page);

    public delegate void FPDF_RenderPageBitmap_delegate(IntPtr bitmap, IntPtr page, int startX, int startY, int sizeX,
        int sizeY, int rotate, int flags);

    public delegate IntPtr FPDFText_LoadPage_delegate(IntPtr page);

    public delegate IntPtr FPDFText_ClosePage_delegate(IntPtr text_page);

    public delegate int FPDFText_CountChars_delegate(IntPtr text_page);

    public delegate int FPDFText_GetUnicode_delegate(IntPtr text_page, int index);

    public delegate bool FPDFText_GetCharBox_delegate(IntPtr text_page, int index, out double left, out double right,
        out double bottom, out double top);

    public FPDF_InitLibrary_delegate FPDF_InitLibrary => Load<FPDF_InitLibrary_delegate>();
    public FPDFBitmap_Create_delegate FPDFBitmap_Create => Load<FPDFBitmap_Create_delegate>();
    public FPDFBitmap_CreateEx_delegate FPDFBitmap_CreateEx => Load<FPDFBitmap_CreateEx_delegate>();
    public FPDFBitmap_FillRect_delegate FPDFBitmap_FillRect => Load<FPDFBitmap_FillRect_delegate>();
    public FPDFBitmap_Destroy_delegate FPDFBitmap_Destroy => Load<FPDFBitmap_Destroy_delegate>();
    public FPDF_LoadDocument_delegate FPDF_LoadDocument => Load<FPDF_LoadDocument_delegate>();
    public FPDF_LoadMemDocument_delegate FPDF_LoadMemDocument => Load<FPDF_LoadMemDocument_delegate>();
    public FPDF_CloseDocument_delegate FPDF_CloseDocument => Load<FPDF_CloseDocument_delegate>();
    public FPDF_GetPageCount_delegate FPDF_GetPageCount => Load<FPDF_GetPageCount_delegate>();
    public FPDF_LoadPage_delegate FPDF_LoadPage => Load<FPDF_LoadPage_delegate>();
    public FPDF_ClosePage_delegate FPDF_ClosePage => Load<FPDF_ClosePage_delegate>();
    public FPDF_GetPageWidth_delegate FPDF_GetPageWidth => Load<FPDF_GetPageWidth_delegate>();
    public FPDF_GetPageHeight_delegate FPDF_GetPageHeight => Load<FPDF_GetPageHeight_delegate>();
    public FPDF_RenderPageBitmap_delegate FPDF_RenderPageBitmap => Load<FPDF_RenderPageBitmap_delegate>();
    public FPDFText_LoadPage_delegate FPDFText_LoadPage => Load<FPDFText_LoadPage_delegate>();
    public FPDFText_ClosePage_delegate FPDFText_ClosePage => Load<FPDFText_ClosePage_delegate>();
    public FPDFText_CountChars_delegate FPDFText_CountChars => Load<FPDFText_CountChars_delegate>();
    public FPDFText_GetUnicode_delegate FPDFText_GetUnicode => Load<FPDFText_GetUnicode_delegate>();
    public FPDFText_GetCharBox_delegate FPDFText_GetCharBox => Load<FPDFText_GetCharBox_delegate>();
}