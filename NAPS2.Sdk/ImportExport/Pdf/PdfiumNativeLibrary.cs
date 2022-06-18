using System.Runtime.InteropServices;
using NAPS2.Unmanaged;

// ReSharper disable InconsistentNaming

namespace NAPS2.ImportExport.Pdf;

public class PdfiumNativeLibrary : NativeLibrary
{
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

    public delegate IntPtr FPDFBitmap_CreateEx_delegate(int width, int height, int format, IntPtr firstScan, int stride);

    public delegate void FPDFBitmap_FillRect_delegate(IntPtr bitmap, int left, int top, int width, int height, uint color);

    public delegate void FPDFBitmap_Destroy_delegate(IntPtr bitmap);

    public delegate IntPtr FPDF_LoadDocument_delegate([MarshalAs(UnmanagedType.LPStr)] string filePath, [MarshalAs(UnmanagedType.LPStr)] string password);

    public delegate IntPtr FPDF_LoadMemDocument_delegate(IntPtr buffer, int size, [MarshalAs(UnmanagedType.LPStr)] string password);

    public delegate void FPDF_CloseDocument_delegate(IntPtr document);

    public delegate int FPDF_GetPageCount_delegate(IntPtr document);

    public delegate IntPtr FPDF_LoadPage_delegate(IntPtr document, int pageIndex);

    public delegate void FPDF_ClosePage_delegate(IntPtr page);

    public delegate double FPDF_GetPageWidth_delegate(IntPtr page);

    public delegate double FPDF_GetPageHeight_delegate(IntPtr page);

    public delegate void FPDF_RenderPageBitmap_delegate(IntPtr bitmap, IntPtr page, int startX, int startY, int sizeX, int sizeY, int rotate, int flags);

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
}