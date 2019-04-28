using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Images.Storage;
using NAPS2.Util;

namespace NAPS2.Scan.Experimental
{
    public interface IScanDriver
    {
        List<ScanDevice> GetDeviceList(ScanOptions options);

        ScanDevice PromptForDevice(ScanOptions options);

        void Scan(ScanOptions options, ProgressHandler progress, CancellationToken cancelToken, Action<IImage> callback);
    }

    public class WiaScanDriver : IScanDriver
    {
        public List<ScanDevice> GetDeviceList(ScanOptions options) => throw new NotImplementedException();

        public ScanDevice PromptForDevice(ScanOptions options) => throw new NotImplementedException();

        public void Scan(ScanOptions options, ProgressHandler progress, CancellationToken cancelToken, Action<IImage> callback)
        {
            throw new NotImplementedException();
        }
    }

    public class LegacyTwainScanDriver : IScanDriver
    {
        public List<ScanDevice> GetDeviceList(ScanOptions options) => throw new NotImplementedException();

        public ScanDevice PromptForDevice(ScanOptions options) => throw new NotImplementedException();

        public void Scan(ScanOptions options, ProgressHandler progress, CancellationToken cancelToken, Action<IImage> callback)
        {
            throw new NotImplementedException();
        }
    }

    public class TwainScanDriver : IScanDriver
    {
        public List<ScanDevice> GetDeviceList(ScanOptions options) => throw new NotImplementedException();

        public ScanDevice PromptForDevice(ScanOptions options) => throw new NotImplementedException();

        public void Scan(ScanOptions options, ProgressHandler progress, CancellationToken cancelToken, Action<IImage> callback)
        {
            throw new NotImplementedException();
        }
    }

    public class SaneScanDriver : IScanDriver
    {
        public List<ScanDevice> GetDeviceList(ScanOptions options) => throw new NotImplementedException();

        public ScanDevice PromptForDevice(ScanOptions options) => throw new NotImplementedException();

        public void Scan(ScanOptions options, ProgressHandler progress, CancellationToken cancelToken, Action<IImage> callback)
        {
            throw new NotImplementedException();
        }
    }
}
