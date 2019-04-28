using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images.Storage;
using NAPS2.Util;

namespace NAPS2.Scan.Experimental.Internal
{
    internal class LegacyTwainScanDriver : IScanDriver
    {
        public List<ScanDevice> GetDeviceList(ScanOptions options) => throw new NotImplementedException();

        public Task Scan(ScanOptions options, ProgressHandler progress, CancellationToken cancelToken, Action<IImage> callback)
        {
            throw new NotImplementedException();
        }
    }
}