using System.Threading;

namespace NAPS2.ImportExport.Images;

public class StubTiffHelper : ITiffHelper
{
    public bool SaveMultipage(IList<ProcessedImage> images, string location, TiffCompression compression, ProgressHandler progressCallback,
        CancellationToken cancelToken)
    {
        throw new NotImplementedException();
    }
}