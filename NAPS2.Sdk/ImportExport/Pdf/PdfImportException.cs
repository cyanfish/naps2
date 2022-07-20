namespace NAPS2.ImportExport.Pdf;

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