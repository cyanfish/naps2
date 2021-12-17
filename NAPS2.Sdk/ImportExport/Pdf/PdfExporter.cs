using System.Threading;
using NAPS2.Ocr;

namespace NAPS2.ImportExport.Pdf;

public abstract class PdfExporter
{
    public async Task<bool> Export(string path, IEnumerable<ScannedImage> images, PdfSettings settings)
    {
        var snapshots = images.Select(x => x.Preserve()).ToList();
        try
        {
            return await Export(path, snapshots, new StubConfigProvider<PdfSettings>(settings));
        }
        finally
        {
            snapshots.ForEach(s => s.Dispose());
        }
    }
        
    public abstract Task<bool> Export(string path, ICollection<ScannedImage.Snapshot> snapshots, IConfigProvider<PdfSettings> settings,
        OcrContext? ocrContext = null, ProgressHandler? progressCallback = null, CancellationToken cancelToken = default);
}