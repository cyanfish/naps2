using System.IO.Compression;
using NAPS2.Pdf;
using NAPS2.Scan;

namespace NAPS2.ImportExport;

/// <summary>
/// Imports PDF or image files.
/// </summary>
public class FileImporter
{
    private readonly PdfImporter _pdfImporter;
    private readonly ImageImporter _imageImporter;

    public FileImporter(ScanningContext scanningContext)
        : this(new PdfImporter(scanningContext), new ImageImporter(scanningContext))
    {
    }

    public FileImporter(PdfImporter pdfImporter, ImageImporter imageImporter)
    {
        _pdfImporter = pdfImporter;
        _imageImporter = imageImporter;
    }


    public IAsyncEnumerable<ProcessedImage> Import(string filePath, ImportParams? importParams = null,
        ProgressHandler progress = default) =>
        Import(new InputPathOrStream(filePath, null, null), importParams, progress);

    public IAsyncEnumerable<ProcessedImage> Import(Stream stream, ImportParams? importParams = null,
        ProgressHandler progress = default) =>
        Import(new InputPathOrStream(null, stream, null), importParams, progress);

    internal IAsyncEnumerable<ProcessedImage> Import(InputPathOrStream input, ImportParams? importParams = null,
        ProgressHandler progress = default, bool skipUnsupported = false)
    {
        if (Path.GetExtension(input.FileName).ToLowerInvariant() == ".pdf")
        {
            return _pdfImporter.Import(input, importParams, progress);
        }
        if (Path.GetExtension(input.FileName).ToLowerInvariant() == ".zip")
        {
            return ImportZip(input, importParams, progress);
        }
        if (ImageContext.GetFileFormatFromExtension(input.FileName) != ImageFileFormat.Unknown)
        {
            return _imageImporter.Import(input, importParams, progress);
        }

        // If we couldn't infer if it's a PDF from the extension, we will try and read the file itself
        var firstBytes = new byte[8];
        if (input.Stream != null)
        {
            input.Stream.Seek(0, SeekOrigin.Begin);
            input.Stream.Read(firstBytes, 0, 8);
            input.Stream.Seek(0, SeekOrigin.Begin);
        }
        else
        {
            using var stream = new FileStream(input.FilePath!, FileMode.Open, FileAccess.Read);
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(firstBytes, 0, 8);
            stream.Seek(0, SeekOrigin.Begin);
        }

        // PDFs begin with "%PDF", possibly with a UTF-8 BOM first
        if (firstBytes is [0x25, 0x50, 0x44, 0x46, ..] or [0xEF, 0xBB, 0xBF, 0x25, 0x50, 0x44, 0x46, ..])
        {
            return _pdfImporter.Import(input, importParams, progress);
        }
        if (firstBytes is [0x50, 0x4b, 0x03, 0x04, ..])
        {
            return ImportZip(input, importParams, progress);
        }

        // If we're recursively importing a zip file, we should ignore any entries that aren't supported formats
        // rather than trying to import and throwing an exception instead.
        if (skipUnsupported && ImageContext.GetFileFormatFromFirstBytes(firstBytes) == ImageFileFormat.Unknown)
        {
            return AsyncProducers.Empty<ProcessedImage>();
        }

        return _imageImporter.Import(input, importParams, progress);
    }

    private async IAsyncEnumerable<ProcessedImage> ImportZip(InputPathOrStream input, ImportParams? importParams,
        ProgressHandler progress)
    {
        using var zip = input.Stream != null ? new ZipArchive(input.Stream) : ZipFile.OpenRead(input.FilePath!);
        int n = 0;
        var fileEntries = zip.Entries.Where(entry => entry.Length > 0).ToList();
        progress.Report(n++, fileEntries.Count);
        foreach (var entry in fileEntries)
        {
            using var entryStream = entry.Open();
            var memoryStream = new MemoryStream();
            entryStream.CopyTo(memoryStream);
            await foreach (var image in Import(new InputPathOrStream(null, memoryStream, entry.Name), importParams,
                               progress.CancelToken, skipUnsupported: true))
            {
                yield return image;
            }
            progress.Report(n++, fileEntries.Count);
        }
    }
}