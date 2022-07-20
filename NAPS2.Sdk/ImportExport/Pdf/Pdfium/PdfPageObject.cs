using System.Runtime.InteropServices;

namespace NAPS2.ImportExport.Pdf.Pdfium;

public class PdfPageObject : NativePdfiumObject
{
    internal PdfPageObject(IntPtr handle) : base(handle)
    {
    }

    public void SetBitmap(PdfBitmap bitmap)
    {
        if (!Native.FPDFImageObj_SetBitmap(IntPtr.Zero, 0, Handle, bitmap.Handle))
        {
            throw new Exception("Could not set bitmap");
        }
    }

    public void LoadJpegFileInline(Stream stream)
    {
        int GetBlock(IntPtr param, ulong position, IntPtr buffer, ulong size)
        {
            stream.Seek((long) position, SeekOrigin.Begin);
            // TODO: Can we skip the intermediate buffer somehow?
            var intermediateBuffer = new byte[size];
            stream.Read(intermediateBuffer, 0, (int) size);
            Marshal.Copy(intermediateBuffer, 0, buffer, (int) size);
            return 1;
        }
        
        PdfiumNativeLibrary.FPDF_FileAccess fileAccess = new()
        {
            m_FileLen = (ulong) stream.Length,
            m_GetBlock = GetBlock
        };
        IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(fileAccess));
        Marshal.StructureToPtr(fileAccess, p, false);
        try
        {
            if (!Native.FPDFImageObj_LoadJpegFileInline(IntPtr.Zero, 0, Handle, p))
            {
                throw new Exception("Could not load jpeg");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(p);
        }
    }

    public void Transform(double a, double b, double c, double d, double e, double f)
    {
        Native.FPDFPageObj_Transform(Handle, a, b, c, d, e, f);
    }

    protected override void DisposeHandle()
    {
        Native.FPDFPageObj_Destroy(Handle);
    }
}