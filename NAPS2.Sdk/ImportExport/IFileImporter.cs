namespace NAPS2.ImportExport;

public interface IFileImporter
{
    IAsyncEnumerable<ProcessedImage> Import(string filePath, ImportParams? importParams = null, ProgressHandler progress = default);
}