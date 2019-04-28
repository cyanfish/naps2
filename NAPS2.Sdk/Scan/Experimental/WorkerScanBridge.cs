using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Remoting.Worker;
using NAPS2.Util;

namespace NAPS2.Scan.Experimental
{
    /// <summary>
    /// Represents scanning in a worker process on the same machine.
    /// </summary>
    public class WorkerScanBridge : IScanBridge
    {
        private readonly IWorkerServiceFactory workerServiceFactory;

        public WorkerScanBridge()
         : this(WorkerManager.Factory)
        {
        }

        public WorkerScanBridge(IWorkerServiceFactory workerServiceFactory)
        {
            this.workerServiceFactory = workerServiceFactory;
        }

        public List<ScanDevice> GetDeviceList(ScanOptions options)
        {
            using (var ctx = workerServiceFactory.Create())
            {
                return ctx.Service.GetDeviceList(options);
            }
        }

        public ScanDevice PromptForDevice(ScanOptions options)
        {
            throw new NotSupportedException("PromptForDevice is not supported for worker scans.");
        }

        public async Task Scan(ScanOptions options, ProgressHandler progress, CancellationToken cancelToken, Action<ScannedImage, PostProcessingContext> callback)
        {
            using (var ctx = workerServiceFactory.Create())
            {
                await ctx.Service.Scan(options, progress, cancelToken, (image, tempPath) =>
                {
                    callback(image, new PostProcessingContext { TempPath = tempPath });
                });
            }
        }
    }
}