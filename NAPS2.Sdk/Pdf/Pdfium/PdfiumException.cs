namespace NAPS2.Pdf.Pdfium;

internal class PdfiumException : Exception
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