namespace NAPS2.Pdf;

public class PdfImportException : Exception
{
    public PdfImportException(string message)
        : base(message)
    {
    }

    public PdfImportException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}