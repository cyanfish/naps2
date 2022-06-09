using NAPS2.Remoting.Worker;

namespace NAPS2.ImportExport.Pdf;

public class PdfiumWorkerCoordinator : IPdfRenderer
{
    private readonly ImageContext _imageContext;
    private readonly WorkerPool _workerPool;

    public PdfiumWorkerCoordinator(ImageContext imageContext, WorkerPool workerPool)
    {
        _imageContext = imageContext;
        _workerPool = workerPool;
    }

    public IEnumerable<IMemoryImage> Render(string path, float dpi)
    {
        // TODO: Only use worker on windows? Or what... 
        var image = _workerPool.Use(worker =>
        {
            var imageStream = new MemoryStream(worker.Service.RenderPdf(path, dpi));
            return _imageContext.Load(imageStream);
        });
        return new[] { image };
    }
}