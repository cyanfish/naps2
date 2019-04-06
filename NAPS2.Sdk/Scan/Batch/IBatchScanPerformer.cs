using System;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Config.Experimental;
using NAPS2.Images;
using NAPS2.WinForms;

namespace NAPS2.Scan.Batch
{
    public interface IBatchScanPerformer
    {
        Task PerformBatchScan(ConfigProvider<BatchSettings> settings, FormBase batchForm, Action<ScannedImage> imageCallback, Action<string> progressCallback, CancellationToken cancelToken);
    }
}