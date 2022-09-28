namespace NAPS2.ImportExport;

public interface IScannedImageImporter
{
    IAsyncEnumerable<ProcessedImage> Import(string filePath, ImportParams importParams, ProgressHandler progress = default);
}