using System.Reflection;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace NAPS2.ImportExport.Pdf.Pdfium;

public class PdfiumNativeLibrary : Unmanaged.NativeLibrary
{
    // TODO: Consider using Pdfium as a full replacement for PdfSharp.
    // The benefits would be more import compatibility + a solution to PdfSharp xplat. But there may be some
    // limitations, e.g. I don't see how you encrypt a document with Pdfium.
    //
    // API reference: https://pdfium.googlesource.com/pdfium/+/main/public/

    private static readonly Lazy<PdfiumNativeLibrary> LazyInstance = new(() =>
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyFolder = Path.GetDirectoryName(assemblyLocation);
        var testRoot = Environment.GetEnvironmentVariable("NAPS2_TEST_ROOT");
        var depsFolder = string.IsNullOrEmpty(testRoot) ? assemblyFolder : testRoot;
        var libraryPath = Path.Combine(depsFolder!, PlatformCompat.System.PdfiumLibraryPath);
        if (!File.Exists(libraryPath))
        {
            throw new Exception($"Library does not exist: {libraryPath}");
        }
        var nativeLib = new PdfiumNativeLibrary(libraryPath);
        nativeLib.FPDF_InitLibrary();
        return nativeLib;
    });

    public static PdfiumNativeLibrary Instance => LazyInstance.Value;

    public const int FPDFBitmap_BGR = 2;
    public const int FPDFBitmap_BGRA = 4;

    public const int FPDF_PRINTING = 0x800;
    public const int FPDF_REVERSE_BYTE_ORDER = 0x10;

    public const int FPDF_INCREMENTAL = 1;
    public const int FPDF_NOINCREMENTAL = 2;
    public const int FPDF_REMOVE_SECURITY = 3;

    public const int FPDF_PAGEOBJ_TEXT = 1;
    public const int FPDF_PAGEOBJ_IMAGE = 3;

    public PdfiumNativeLibrary(string libraryPath)
        : base(libraryPath)
    {
    }

    public delegate IntPtr FPDF_InitLibrary_delegate();

    public delegate IntPtr FPDF_GetLastError_delegate();

    public delegate IntPtr FPDFBitmap_Create_delegate(int width, int height, int alpha);

    public delegate IntPtr
        FPDFBitmap_CreateEx_delegate(int width, int height, int format, IntPtr firstScan, int stride);

    public delegate void FPDFBitmap_FillRect_delegate(IntPtr bitmap, int left, int top, int width, int height,
        uint color);

    public delegate void FPDFBitmap_Destroy_delegate(IntPtr bitmap);

    public delegate int FPDFBitmap_GetWidth_delegate(IntPtr bitmap);

    public delegate int FPDFBitmap_GetHeight_delegate(IntPtr bitmap);

    public delegate IntPtr FPDFBitmap_GetBuffer_delegate(IntPtr bitmap);

    public delegate int FPDFBitmap_GetStride_delegate(IntPtr bitmap);

    public delegate int FPDFBitmap_GetFormat_delegate(IntPtr bitmap);

    public delegate IntPtr FPDF_CreateNewDocument_delegate();

    public delegate IntPtr FPDF_LoadDocument_delegate([MarshalAs(UnmanagedType.LPStr)] string filePath,
        [MarshalAs(UnmanagedType.LPStr)]
        string? password);

    public delegate IntPtr FPDF_LoadMemDocument_delegate(IntPtr buffer, int size,
        [MarshalAs(UnmanagedType.LPStr)]
        string? password);

    public delegate void FPDF_CloseDocument_delegate(IntPtr document);

    public delegate bool FPDF_SaveAsCopy_delegate(IntPtr document, ref FPDF_FileWrite fileWrite, int flags);

    public delegate IntPtr FPDF_GetMetaText_delegate(IntPtr document, [MarshalAs(UnmanagedType.LPStr)] string tag,
        byte[]? buffer, IntPtr buflen);

    public delegate int FPDF_GetPageCount_delegate(IntPtr document);

    public delegate IntPtr FPDF_LoadPage_delegate(IntPtr document, int pageIndex);

    public delegate void FPDF_ClosePage_delegate(IntPtr page);

    public delegate float FPDF_GetPageWidthF_delegate(IntPtr page);

    public delegate float FPDF_GetPageHeightF_delegate(IntPtr page);

    public delegate void FPDF_RenderPageBitmap_delegate(IntPtr bitmap, IntPtr page, int startX, int startY, int sizeX,
        int sizeY, int rotate, int flags);

    public delegate IntPtr FPDFText_LoadPage_delegate(IntPtr page);

    public delegate IntPtr FPDFText_ClosePage_delegate(IntPtr text_page);

    public delegate bool FPDF_ImportPages_delegate(IntPtr dest_doc, IntPtr src_doc,
        [MarshalAs(UnmanagedType.LPStr)] string? pagerange, int index);

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

    public delegate bool FPDFPageObj_SetMatrix_delegate(IntPtr page_object, ref PdfMatrix matrix);

    public delegate bool FPDFImageObj_LoadJpegFile_delegate(IntPtr pages, int count, IntPtr image_object,
        ref FPDF_FileAccess file_access);

    public delegate bool FPDFImageObj_LoadJpegFileInline_delegate(IntPtr pages, int count, IntPtr image_object,
        ref FPDF_FileAccess file_access);

    public delegate int FPDFPage_CountObjects_delegate(IntPtr page);

    public delegate IntPtr FPDFPage_GetObject_delegate(IntPtr page, int index);

    public delegate bool FPDFPage_RemoveObject_delegate(IntPtr page, IntPtr page_obj);

    public delegate IntPtr FPDFImageObj_GetBitmap_delegate(IntPtr image_object);

    public delegate IntPtr
        FPDFImageObj_GetRenderedBitmap_delegate(IntPtr document, IntPtr page, IntPtr image_object);

    public delegate int FPDFImageObj_GetImageFilterCount_delegate(IntPtr image_object);

    public delegate IntPtr FPDFImageObj_GetImageFilter_delegate(IntPtr image_object, int index, byte[]? buffer,
        IntPtr buflen);

    public delegate bool FPDFImageObj_GetImageMetadata_delegate(IntPtr image_object, IntPtr page,
        ref FPDF_ImageObj_Metadata metadata);

    public delegate bool FPDFPageObj_GetMatrix_delegate(IntPtr page_object, out PdfMatrix matrix);

    public delegate bool FPDFPageObj_GetStrokeColor_delegate(IntPtr page_object, out uint r, out uint g, out uint b,
        out uint a);

    public delegate bool FPDFPageObj_GetFillColor_delegate(IntPtr page_object, out uint r, out uint g, out uint b,
        out uint a);

    public delegate int FPDFPageObj_GetType_delegate(IntPtr page_object);

    public delegate IntPtr
        FPDFTextObj_GetText_delegate(IntPtr text_object, IntPtr text_page, byte[]? buffer, IntPtr length);

    public delegate int FPDFTextObj_GetTextRenderMode_delegate(IntPtr text);

    public FPDF_InitLibrary_delegate FPDF_InitLibrary => Load<FPDF_InitLibrary_delegate>();
    public FPDF_GetLastError_delegate FPDF_GetLastError => Load<FPDF_GetLastError_delegate>();
    public FPDFBitmap_Create_delegate FPDFBitmap_Create => Load<FPDFBitmap_Create_delegate>();
    public FPDFBitmap_CreateEx_delegate FPDFBitmap_CreateEx => Load<FPDFBitmap_CreateEx_delegate>();
    public FPDFBitmap_FillRect_delegate FPDFBitmap_FillRect => Load<FPDFBitmap_FillRect_delegate>();
    public FPDFBitmap_GetWidth_delegate FPDFBitmap_GetWidth => Load<FPDFBitmap_GetWidth_delegate>();
    public FPDFBitmap_GetHeight_delegate FPDFBitmap_GetHeight => Load<FPDFBitmap_GetHeight_delegate>();
    public FPDFBitmap_GetBuffer_delegate FPDFBitmap_GetBuffer => Load<FPDFBitmap_GetBuffer_delegate>();
    public FPDFBitmap_GetStride_delegate FPDFBitmap_GetStride => Load<FPDFBitmap_GetStride_delegate>();
    public FPDFBitmap_GetFormat_delegate FPDFBitmap_GetFormat => Load<FPDFBitmap_GetFormat_delegate>();
    public FPDFBitmap_Destroy_delegate FPDFBitmap_Destroy => Load<FPDFBitmap_Destroy_delegate>();
    public FPDF_CreateNewDocument_delegate FPDF_CreateNewDocument => Load<FPDF_CreateNewDocument_delegate>();
    public FPDF_LoadDocument_delegate FPDF_LoadDocument => Load<FPDF_LoadDocument_delegate>();
    public FPDF_LoadMemDocument_delegate FPDF_LoadMemDocument => Load<FPDF_LoadMemDocument_delegate>();
    public FPDF_CloseDocument_delegate FPDF_CloseDocument => Load<FPDF_CloseDocument_delegate>();
    public FPDF_SaveAsCopy_delegate FPDF_SaveAsCopy => Load<FPDF_SaveAsCopy_delegate>();
    public FPDF_GetMetaText_delegate FPDF_GetMetaText => Load<FPDF_GetMetaText_delegate>();
    public FPDF_GetPageCount_delegate FPDF_GetPageCount => Load<FPDF_GetPageCount_delegate>();
    public FPDF_LoadPage_delegate FPDF_LoadPage => Load<FPDF_LoadPage_delegate>();
    public FPDF_ClosePage_delegate FPDF_ClosePage => Load<FPDF_ClosePage_delegate>();
    public FPDF_GetPageWidthF_delegate FPDF_GetPageWidthF => Load<FPDF_GetPageWidthF_delegate>();
    public FPDF_GetPageHeightF_delegate FPDF_GetPageHeightF => Load<FPDF_GetPageHeightF_delegate>();
    public FPDF_RenderPageBitmap_delegate FPDF_RenderPageBitmap => Load<FPDF_RenderPageBitmap_delegate>();
    public FPDFText_LoadPage_delegate FPDFText_LoadPage => Load<FPDFText_LoadPage_delegate>();
    public FPDFText_ClosePage_delegate FPDFText_ClosePage => Load<FPDFText_ClosePage_delegate>();
    public FPDF_ImportPages_delegate FPDF_ImportPages => Load<FPDF_ImportPages_delegate>();
    public FPDFText_CountChars_delegate FPDFText_CountChars => Load<FPDFText_CountChars_delegate>();
    public FPDFText_GetUnicode_delegate FPDFText_GetUnicode => Load<FPDFText_GetUnicode_delegate>();
    public FPDFText_GetCharBox_delegate FPDFText_GetCharBox => Load<FPDFText_GetCharBox_delegate>();
    public FPDFPageObj_NewImageObj_delegate FPDFPageObj_NewImageObj => Load<FPDFPageObj_NewImageObj_delegate>();
    public FPDFPageObj_Destroy_delegate FPDFPageObj_Destroy => Load<FPDFPageObj_Destroy_delegate>();
    public FPDFPage_InsertObject_delegate FPDFPage_InsertObject => Load<FPDFPage_InsertObject_delegate>();
    public FPDFImageObj_SetBitmap_delegate FPDFImageObj_SetBitmap => Load<FPDFImageObj_SetBitmap_delegate>();
    public FPDFPage_New_delegate FPDFPage_New => Load<FPDFPage_New_delegate>();
    public FPDFPage_GenerateContent_delegate FPDFPage_GenerateContent => Load<FPDFPage_GenerateContent_delegate>();
    public FPDFPageObj_SetMatrix_delegate FPDFPageObj_SetMatrix => Load<FPDFPageObj_SetMatrix_delegate>();
    public FPDFImageObj_LoadJpegFile_delegate FPDFImageObj_LoadJpegFile => Load<FPDFImageObj_LoadJpegFile_delegate>();

    public FPDFImageObj_LoadJpegFileInline_delegate FPDFImageObj_LoadJpegFileInline =>
        Load<FPDFImageObj_LoadJpegFileInline_delegate>();

    public FPDFPage_CountObjects_delegate FPDFPage_CountObjects => Load<FPDFPage_CountObjects_delegate>();
    public FPDFPage_GetObject_delegate FPDFPage_GetObject => Load<FPDFPage_GetObject_delegate>();
    public FPDFPage_RemoveObject_delegate FPDFPage_RemoveObject => Load<FPDFPage_RemoveObject_delegate>();
    public FPDFImageObj_GetBitmap_delegate FPDFImageObj_GetBitmap => Load<FPDFImageObj_GetBitmap_delegate>();

    public FPDFImageObj_GetRenderedBitmap_delegate FPDFImageObj_GetRenderedBitmap =>
        Load<FPDFImageObj_GetRenderedBitmap_delegate>();

    public FPDFImageObj_GetImageFilterCount_delegate FPDFImageObj_GetImageFilterCount =>
        Load<FPDFImageObj_GetImageFilterCount_delegate>();

    public FPDFImageObj_GetImageFilter_delegate FPDFImageObj_GetImageFilter =>
        Load<FPDFImageObj_GetImageFilter_delegate>();

    public FPDFImageObj_GetImageMetadata_delegate FPDFImageObj_GetImageMetadata =>
        Load<FPDFImageObj_GetImageMetadata_delegate>();

    public FPDFPageObj_GetMatrix_delegate FPDFPageObj_GetMatrix => Load<FPDFPageObj_GetMatrix_delegate>();

    public FPDFPageObj_GetStrokeColor_delegate FPDFPageObj_GetStrokeColor =>
        Load<FPDFPageObj_GetStrokeColor_delegate>();

    public FPDFPageObj_GetFillColor_delegate FPDFPageObj_GetFillColor =>
        Load<FPDFPageObj_GetFillColor_delegate>();

    public FPDFPageObj_GetType_delegate FPDFPageObj_GetType => Load<FPDFPageObj_GetType_delegate>();
    public FPDFTextObj_GetText_delegate FPDFTextObj_GetText => Load<FPDFTextObj_GetText_delegate>();

    public FPDFTextObj_GetTextRenderMode_delegate FPDFTextObj_GetTextRenderMode =>
        Load<FPDFTextObj_GetTextRenderMode_delegate>();

    public struct FPDF_FileWrite
    {
        public int version;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public WriteBlock_delegate WriteBlock;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int WriteBlock_delegate(IntPtr self, IntPtr data, IntPtr size);

    public struct FPDF_FileAccess
    {
        public IntPtr m_FileLen;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public GetBlock_delegate m_GetBlock;
        public IntPtr m_Param;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int GetBlock_delegate(IntPtr param, IntPtr position, IntPtr buffer, IntPtr size);

    public struct FPDF_ImageObj_Metadata
    {
        public uint width;
        public uint height;
        public float horizontal_dpi;
        public float vertical_dpi;
        public int bits_per_pixel;
        [MarshalAs(UnmanagedType.I4)]
        public Colorspace colorspace;
        public int marked_content_id;
    }
}