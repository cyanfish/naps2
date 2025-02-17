using NAPS2.Pdf.Pdfium;

namespace NAPS2.ImportExport;

internal record InputPathOrStream(string? FilePath, Stream? Stream, string? StreamFileName)
{
    public string FileName => Stream != null
        ? StreamFileName ?? ""
        : Path.GetFileName(FilePath)!;

    public void CopyToFile(string outputPath)
    {
        if (Stream != null)
        {
            using var outputStream = new FileStream(outputPath, FileMode.Create);
            Stream.CopyTo(outputStream);
        }
        else
        {
            File.Copy(FilePath!, outputPath);
        }
    }

    public void CopyToStream(Stream outputStream)
    {
        if (Stream != null)
        {
            Stream.CopyTo(outputStream);
        }
        else
        {
            using var inputStream = new FileStream(FilePath!, FileMode.Open);
            inputStream.CopyTo(outputStream);
        }
    }

    public PdfDocument LoadPdfDoc(string? password)
    {
        return Stream != null
            ? PdfDocument.Load(Stream, password)
            : PdfDocument.Load(FilePath!, password);
    }
}