namespace NAPS2.ImportExport;

public interface IScannedImageImporter
{
    AsyncSource<ProcessedImage> Import(string filePath, ImportParams importParams, ProgressHandler progress = default);
}