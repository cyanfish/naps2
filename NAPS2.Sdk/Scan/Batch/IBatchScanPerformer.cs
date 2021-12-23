using System.Threading;
using NAPS2.WinForms;

namespace NAPS2.Scan.Batch;

public interface IBatchScanPerformer
{
    Task PerformBatchScan(IConfigProvider<BatchSettings> settings, FormBase batchForm, Action<RenderableImage> imageCallback, Action<string> progressCallback, CancellationToken cancelToken);
}