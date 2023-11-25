using NAPS2.ImportExport.Images;
using NAPS2.Pdf;

namespace NAPS2.ImportExport;

public class FileImporter : IFileImporter
{
    private readonly IFileImporter _pdfImporter;
    private readonly IFileImporter _imageImporter;

    public FileImporter(IPdfImporter pdfImporter, IImageImporter imageImporter)
    {
        _pdfImporter = pdfImporter;
        _imageImporter = imageImporter;
    }

    public IAsyncEnumerable<ProcessedImage> Import(string filePath, ImportParams? importParams = null,
        ProgressHandler progress = default)
    {
        if (filePath == null)
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (Path.GetExtension(filePath).ToLowerInvariant() == ".pdf")
        {
            return _pdfImporter.Import(filePath, importParams, progress);
        }
        if (ImageContext.GetFileFormatFromExtension(filePath) != ImageFileFormat.Unspecified)
        {
            return _imageImporter.Import(filePath, importParams, progress);
        }

        // If we couldn't infer if it's a PDF from the extension, we will try and read the file itself
        var firstBytes = new byte[8];
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(firstBytes, 0, 8);
        stream.Seek(0, SeekOrigin.Begin);
        
        // PDFs begin with "%PDF", possibly with a UTF-8 BOM first
        if (firstBytes is [0x25, 0x50, 0x44, 0x46, ..] or [0xEF, 0xBB, 0xBF, 0x25, 0x50, 0x44, 0x46, ..])
        {
            return _pdfImporter.Import(filePath, importParams, progress);
        }
        
        return _imageImporter.Import(filePath, importParams, progress);
    }
}