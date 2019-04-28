using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Images;

namespace NAPS2.Scan.Experimental
{
    /// <summary>
    /// Abstracts communication with the scanner. This enables scanning over a network or in a worker process.
    /// </summary>
    public interface IScanAdapter
    {
        List<ScanDevice> GetDeviceList(ScanOptions options);

        ScanDevice PromptForDevice(ScanOptions options);

        ScannedImageSource Scan(ScanOptions options, CancellationToken cancelToken);
    }

    /// <summary>
    /// Represents scanning in a worker process on the same machine.
    /// </summary>
    public class WorkerScanAdapter : IScanAdapter
    {
        public List<ScanDevice> GetDeviceList(ScanOptions options) => throw new NotImplementedException();

        public ScanDevice PromptForDevice(ScanOptions options) => throw new NotImplementedException();

        public ScannedImageSource Scan(ScanOptions options, CancellationToken cancelToken) => throw new NotImplementedException();
    }

    /// <summary>
    /// Represents scanning across a network on a different machine.
    /// </summary>
    public class NetworkScanAdapter : IScanAdapter
    {
        public List<ScanDevice> GetDeviceList(ScanOptions options) => throw new NotImplementedException();

        public ScanDevice PromptForDevice(ScanOptions options) => throw new NotImplementedException();

        public ScannedImageSource Scan(ScanOptions options, CancellationToken cancelToken) => throw new NotImplementedException();
    }

    /// <summary>
    /// Represents scanning in the local process.
    /// </summary>
    public class LocalScanAdapter : IScanAdapter
    {
        public List<ScanDevice> GetDeviceList(ScanOptions options) => throw new NotImplementedException();

        public ScanDevice PromptForDevice(ScanOptions options) => throw new NotImplementedException();

        public ScannedImageSource Scan(ScanOptions options, CancellationToken cancelToken) => throw new NotImplementedException();
    }
}
