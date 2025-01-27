using NAPS2.Pdf.Pdfium;

namespace NAPS2.ImportExport;

internal record OutputPathOrStream(string? Path, Stream? Stream)
{
    public void CopyFromStream(MemoryStream inputStream)
    {
        if (Stream != null)
        {
            inputStream.CopyTo(Stream);
        }
        else
        {
            FileSystemHelper.EnsureParentDirExists(Path!);
            using var fileStream = new FileStream(Path!, FileMode.Create);
            inputStream.CopyTo(fileStream);
        }
    }

    public void SavePdfDoc(PdfDocument doc)
    {
        if (Stream != null)
        {
            doc.Save(Stream);
        }
        else
        {
            FileSystemHelper.EnsureParentDirExists(Path!);
            doc.Save(Path!);
        }
    }
}