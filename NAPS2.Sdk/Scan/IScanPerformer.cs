using System;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;

namespace NAPS2.Scan
{
    /// <summary>
    /// A high-level interface used for scanning.
    /// This abstracts away the logic of obtaining and using an instance of IScanDriver.
    /// </summary>
    public interface IScanPerformer
    {
        Task<ScanDevice> PromptForDevice(ScanProfile scanProfile, IntPtr dialogParent = default);
        
        Task<ScannedImageSource> PerformScan(ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent = default, CancellationToken cancelToken = default);
    }
}
