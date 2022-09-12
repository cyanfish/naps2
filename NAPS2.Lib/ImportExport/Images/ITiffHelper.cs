using System.Threading;

namespace NAPS2.ImportExport.Images;

// TODO: Merge this functionality into ImageContext or something
public interface ITiffHelper
{
    bool SaveMultipage(IList<ProcessedImage> images, string location, TiffCompression compression, ProgressHandler progressCallback, CancellationToken cancelToken);
}