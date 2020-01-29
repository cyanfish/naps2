using System.Collections.Generic;
using System.IO;
using NAPS2.Images.Storage;
using NAPS2.Remoting.Worker;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfiumWorkerCoordinator : IPdfRenderer
    {
        private readonly ImageContext imageContext;
        private readonly WorkerPool workerPool;

        public PdfiumWorkerCoordinator(ImageContext imageContext, WorkerPool workerPool)
        {
            this.imageContext = imageContext;
            this.workerPool = workerPool;
        }

        public IEnumerable<IImage> Render(string path, float dpi)
        {
            // TODO: Only use worker on windows? Or what... 
            var image = workerPool.Use(worker =>
            {
                var imageStream = new MemoryStream(worker.Service.RenderPdf(path, dpi));
                return imageContext.ImageFactory.Decode(imageStream, "");
            });
            return new[] { image };
        }
    }
}