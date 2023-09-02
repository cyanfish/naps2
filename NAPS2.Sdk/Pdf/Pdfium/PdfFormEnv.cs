using System.Runtime.InteropServices;

namespace NAPS2.Pdf.Pdfium;

internal class PdfFormEnv : NativePdfiumObject
{
    private readonly GCHandle _formInfoHandle;

    public PdfFormEnv(IntPtr handle, GCHandle formInfoHandle) : base(handle)
    {
        _formInfoHandle = formInfoHandle;
    }

    public void DrawForms(PdfBitmap bitmap, PdfPage page)
    {
        Native.FPDF_FFLDraw(Handle, bitmap.Handle, page.Handle, 0, 0, bitmap.Width, bitmap.Height, 0,
            PdfiumNativeLibrary.FPDF_PRINTING);
    }

    protected override void DisposeHandle()
    {
        Native.FPDFDOC_ExitFormFillEnvironment(Handle);
        _formInfoHandle.Free();
    }
}