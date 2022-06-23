using System.Threading;
using NAPS2.WinForms;

namespace NAPS2.Scan.Batch;

public interface IBatchScanPerformer
{
    Task PerformBatchScan(BatchSettings settings, FormBase batchForm, Action<ProcessedImage> imageCallback, Action<string> progressCallback, CancellationToken cancelToken);
}