using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Remoting.Worker;

namespace NAPS2.Scan.Internal
{
    /// <summary>
    /// Represents scanning in a worker process on the same machine.
    /// </summary>
    internal class WorkerScanBridge : IScanBridge
    {
        private readonly ImageContext imageContext;
        private readonly IWorkerFactory workerFactory;

        public WorkerScanBridge() : this(ImageContext.Default, WorkerFactory.Default)
        {
        }

        public WorkerScanBridge(ImageContext imageContext, IWorkerFactory workerFactory)
        {
            this.imageContext = imageContext;
            this.workerFactory = workerFactory;
        }

        public async Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
        {
            using (var ctx = workerFactory.Create())
            {
                return await ctx.Service.GetDeviceList(options);
            }
        }

        public async Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<ScannedImage, PostProcessingContext> callback)
        {
            using (var ctx = workerFactory.Create())
            {
                await ctx.Service.Scan(imageContext, options, cancelToken, scanEvents, (image, tempPath) =>
                {
                    callback(image, new PostProcessingContext { TempPath = tempPath });
                });
            }
        }
    }
}