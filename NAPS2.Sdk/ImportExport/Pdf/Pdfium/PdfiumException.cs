namespace NAPS2.ImportExport.Pdf.Pdfium;

public class PdfiumException : Exception
{
    public PdfiumException(PdfiumErrorCode errorCode)
        : base($"Pdf error: {errorCode}")
    {
        ErrorCode = errorCode;
    }

    public PdfiumException(string message, PdfiumErrorCode errorCode)
        : base($"{message}: {errorCode}")
    {
        ErrorCode = errorCode;
    }

    public PdfiumErrorCode ErrorCode { get; }
}