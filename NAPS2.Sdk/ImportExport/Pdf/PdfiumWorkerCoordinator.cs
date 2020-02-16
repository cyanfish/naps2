using System.Collections.Generic;
using System.IO;
using NAPS2.Images.Storage;
using NAPS2.Remoting.Worker;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfiumWorkerCoordinator : IPdfRenderer
    {
        private readonly ImageContext _imageContext;
        private readonly WorkerPool _workerPool;

        public PdfiumWorkerCoordinator(ImageContext imageContext, WorkerPool workerPool)
        {
            _imageContext = imageContext;
            _workerPool = workerPool;
        }

        public IEnumerable<IImage> Render(string path, float dpi)
        {
            // TODO: Only use worker on windows? Or what... 
            var image = _workerPool.Use(worker =>
            {
                var imageStream = new MemoryStream(worker.Service.RenderPdf(path, dpi));
                return _imageContext.ImageFactory.Decode(imageStream, "");
            });
            return new[] { image };
        }
    }
}