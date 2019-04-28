using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Remoting.Worker;
using NAPS2.Util;

namespace NAPS2.Scan.Experimental.Internal
{
    /// <summary>
    /// Represents scanning in a worker process on the same machine.
    /// </summary>
    internal class WorkerScanBridge : IScanBridge
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