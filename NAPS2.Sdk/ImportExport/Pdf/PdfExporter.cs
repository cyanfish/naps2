using System.Threading;
using NAPS2.Ocr;

namespace NAPS2.ImportExport.Pdf;

public abstract class PdfExporter
{
    public async Task<bool> Export(string path, ICollection<ProcessedImage> images, PdfSettings settings)
    {
        try
        {
            return await Export(path, images, new StubConfigProvider<PdfSettings>(settings));
        }
        finally
        {
            foreach (var image in images)
            {
                image.Dispose();
            }
        }
    }

    // TODO: Unify lifetime management
    // TODO: Also does it make sense to have some kind of custom renderable image collection that encapsulates whether it should dispose on completion?
    public abstract Task<bool> Export(string path, ICollection<ProcessedImage> images, IConfigProvider<PdfSettings> settings,
        OcrContext? ocrContext = null, ProgressHandler? progressCallback = null, CancellationToken cancelToken = default);
}