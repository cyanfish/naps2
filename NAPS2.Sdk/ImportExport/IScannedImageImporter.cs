using System.Threading;

namespace NAPS2.ImportExport;

public interface IScannedImageImporter
{
    ScannedImageSource Import(string filePath, ImportParams importParams, ProgressHandler progressCallback, CancellationToken cancelToken);
}