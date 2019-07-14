using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;

namespace NAPS2.Scan.Experimental.Internal
{
    /// <summary>
    /// Delegates to an implementation of IScanDriver based on the options and environment.
    /// </summary>
    internal interface IRemoteScanController
    {
        List<ScanDevice> GetDeviceList(ScanOptions options);

        Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<ScannedImage, PostProcessingContext> callback);
    }
}