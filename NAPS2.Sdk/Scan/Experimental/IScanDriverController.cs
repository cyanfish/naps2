using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Util;

namespace NAPS2.Scan.Experimental
{
    /// <summary>
    /// Delegates to an implementation of IScanDriver based on the options and environment.
    /// </summary>
    public interface IScanDriverController
    {
        List<ScanDevice> GetDeviceList(ScanOptions options);

        ScanDevice PromptForDevice(ScanOptions options);

        void Scan(ScanOptions options, ProgressHandler progress, CancellationToken cancelToken, Action<IImage> callback);
    }
}