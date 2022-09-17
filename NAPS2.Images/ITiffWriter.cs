using System.Threading;

namespace NAPS2.Images;

public interface ITiffWriter
{
    bool SaveTiff(IList<IMemoryImage> images, string path,
        TiffCompressionType compression = TiffCompressionType.Auto, Action<int, int>? progressCallback = null,
        CancellationToken cancelToken = default);

    bool SaveTiff(IList<IMemoryImage> images, Stream stream,
        TiffCompressionType compression = TiffCompressionType.Auto, Action<int, int>? progressCallback = null,
        CancellationToken cancelToken = default);
}