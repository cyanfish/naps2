using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Config;
using NAPS2.Ocr;
using NAPS2.Images;
using NAPS2.Util;

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