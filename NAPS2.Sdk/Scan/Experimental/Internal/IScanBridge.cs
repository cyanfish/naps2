using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;

namespace NAPS2.Scan.Experimental.Internal
{
    /// <summary>
    /// Abstracts communication with the scanner. This enables scanning over a network or in a worker process.
    /// </summary>
    internal interface IScanBridge
    {
        Task<List<ScanDevice>> GetDeviceList(ScanOptions options);

        Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<ScannedImage, PostProcessingContext> callback);
    }
}
