using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Util;

namespace NAPS2.Scan.Experimental
{
    /// <summary>
    /// Delegates to an implementation of IScanDriver based on the options and environment.
    /// </summary>
    public interface IRemoteScanController
    {
        List<ScanDevice> GetDeviceList(ScanOptions options);

        ScanDevice PromptForDevice(ScanOptions options);

        Task Scan(ScanOptions options, ProgressHandler progress, CancellationToken cancelToken, Action<ScannedImage, PostProcessingContext> callback);
    }
}