using NAPS2.Remoting.Worker;

namespace NAPS2.ImportExport.Pdf;

public class PdfiumWorkerCoordinator : IPdfRenderer
{
    private readonly WorkerPool _workerPool;

    public PdfiumWorkerCoordinator(WorkerPool workerPool)
    {
        _workerPool = workerPool;
    }

    public IEnumerable<IMemoryImage> Render(ImageContext imageContext, string path, float defaultDpi)
    {
        // TODO: Only use worker on windows? Or what... 
        var image = _workerPool.Use(worker =>
        {
            var imageStream = new MemoryStream(worker.Service.RenderPdf(path, defaultDpi));
            return imageContext.Load(imageStream);
        });
        return new[] { image };
    }
}