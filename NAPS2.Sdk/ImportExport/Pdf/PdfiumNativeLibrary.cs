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
        var testRoot = Environment.GetEnvironmentVariable("NAPS2_TEST_ROOT");
        var depsFolder = string.IsNullOrEmpty(testRoot) ? assemblyFolder : testRoot;
        var libraryPath = Path.Combine(depsFolder, PlatformCompat.System.PdfiumLibraryPath);
        if (!File.Exists(libraryPath))
        {
            throw new Exception($"Library does not exist: {libraryPath}");
        }
        var nativeLib = new PdfiumNativeLibrary(libraryPath);
        nativeLib.FPDF_InitLibrary();
        return nativeLib;
    });

    public const int FPDFBitmap_BGR = 2;
    public const int FPDFBitmap_BGRA = 4;

    public const int FPDF_PRINTING = 0x800;
    public const int FPDF_REVERSE_BYTE_ORDER = 0x10;

    public const int FPDF_INCREMENTAL = 1;
    public const int FPDF_NOINCREMENTAL = 2;
    public const int FPDF_REMOVE_SECURITY = 3;

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

    public delegate IntPtr FPDF_CreateNewDocument_delegate();

    public delegate IntPtr FPDF_LoadDocument_delegate([MarshalAs(UnmanagedType.LPStr)] string filePath,
        [MarshalAs(UnmanagedType.LPStr)]
        string? password);

    public delegate IntPtr FPDF_LoadMemDocument_delegate(IntPtr buffer, int size,
        [MarshalAs(UnmanagedType.LPStr)]
        string? password);

    public delegate void FPDF_CloseDocument_delegate(IntPtr document);

    public delegate bool FPDF_SaveAsCopy_delegate(IntPtr document, IntPtr fileWrite, int flags);

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

    public delegate IntPtr FPDFPageObj_NewImageObj_delegate(IntPtr document);

    public delegate void FPDFPageObj_Destroy_delegate(IntPtr page_obj);

    public delegate void FPDFPage_InsertObject_delegate(IntPtr page, IntPtr page_obj);

    public delegate bool FPDFImageObj_SetBitmap_delegate(IntPtr pages, int count, IntPtr image_object, IntPtr bitmap);

    public delegate IntPtr FPDFPage_New_delegate(IntPtr document, int page_index, double width, double height);

    public delegate IntPtr FPDFPage_GenerateContent_delegate(IntPtr page);

    public delegate IntPtr FPDFPageObj_Transform_delegate(IntPtr page_object, double a, double b, double c, double d,
        double e, double f);

    public delegate bool FPDFImageObj_LoadJpegFile_delegate(IntPtr pages, int count, IntPtr image_object,
        IntPtr file_access);

    public delegate bool FPDFImageObj_LoadJpegFileInline_delegate(IntPtr pages, int count, IntPtr image_object,
        IntPtr file_access);

    public FPDF_InitLibrary_delegate FPDF_InitLibrary => Load<FPDF_InitLibrary_delegate>();
    public FPDFBitmap_Create_delegate FPDFBitmap_Create => Load<FPDFBitmap_Create_delegate>();
    public FPDFBitmap_CreateEx_delegate FPDFBitmap_CreateEx => Load<FPDFBitmap_CreateEx_delegate>();
    public FPDFBitmap_FillRect_delegate FPDFBitmap_FillRect => Load<FPDFBitmap_FillRect_delegate>();
    public FPDFBitmap_Destroy_delegate FPDFBitmap_Destroy => Load<FPDFBitmap_Destroy_delegate>();
    public FPDF_CreateNewDocument_delegate FPDF_CreateNewDocument => Load<FPDF_CreateNewDocument_delegate>();
    public FPDF_LoadDocument_delegate FPDF_LoadDocument => Load<FPDF_LoadDocument_delegate>();
    public FPDF_LoadMemDocument_delegate FPDF_LoadMemDocument => Load<FPDF_LoadMemDocument_delegate>();
    public FPDF_CloseDocument_delegate FPDF_CloseDocument => Load<FPDF_CloseDocument_delegate>();
    public FPDF_SaveAsCopy_delegate FPDF_SaveAsCopy => Load<FPDF_SaveAsCopy_delegate>();
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
    public FPDFPageObj_NewImageObj_delegate FPDFPageObj_NewImageObj => Load<FPDFPageObj_NewImageObj_delegate>();
    public FPDFPageObj_Destroy_delegate FPDFPageObj_Destroy => Load<FPDFPageObj_Destroy_delegate>();
    public FPDFPage_InsertObject_delegate FPDFPage_InsertObject => Load<FPDFPage_InsertObject_delegate>();
    public FPDFImageObj_SetBitmap_delegate FPDFImageObj_SetBitmap => Load<FPDFImageObj_SetBitmap_delegate>();
    public FPDFPage_New_delegate FPDFPage_New => Load<FPDFPage_New_delegate>();
    public FPDFPage_GenerateContent_delegate FPDFPage_GenerateContent => Load<FPDFPage_GenerateContent_delegate>();
    public FPDFPageObj_Transform_delegate FPDFPageObj_Transform => Load<FPDFPageObj_Transform_delegate>();
    public FPDFImageObj_LoadJpegFile_delegate FPDFImageObj_LoadJpegFile => Load<FPDFImageObj_LoadJpegFile_delegate>();

    public FPDFImageObj_LoadJpegFileInline_delegate FPDFImageObj_LoadJpegFileInline =>
        Load<FPDFImageObj_LoadJpegFileInline_delegate>();

    public struct FPDF_FileWrite
    {
        public int version = 1;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public WriteBlock_delegate WriteBlock;
    }

    public delegate int WriteBlock_delegate(IntPtr self, IntPtr data, ulong size);

    public struct FPDF_FileAccess
    {
        public ulong m_FileLen;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public GetBlock_delegate m_GetBlock;
        public IntPtr m_Param;
    }

    public delegate int GetBlock_delegate(IntPtr param, ulong position, IntPtr buffer, ulong size);
}