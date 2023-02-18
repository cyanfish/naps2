using NAPS2.Remoting.Worker;

namespace NAPS2.Pdf;

internal class PdfiumWorkerCoordinator : IPdfRenderer
{
    private readonly WorkerPool _workerPool;

    public PdfiumWorkerCoordinator(WorkerPool workerPool)
    {
        _workerPool = workerPool;
    }

    public IEnumerable<IMemoryImage> Render(ImageContext imageContext, string path, PdfRenderSize renderSize,
        string? password = null)
    {
        if (password != null)
        {
            // TODO: Do we want to implement this?
            throw new InvalidOperationException();
        }
        var image = _workerPool.Use(
            WorkerType.Native,
            worker =>
            {
                // TODO: Transmit render size
                var imageStream = new MemoryStream(worker.Service.RenderPdf(path, renderSize.Dpi ?? 300));
                return imageContext.Load(imageStream);
            });
        return new[] { image };
    }

    public IEnumerable<IMemoryImage> Render(ImageContext imageContext, byte[] buffer, int length,
        PdfRenderSize renderSize, string? password = null)
    {
        throw new NotImplementedException();
    }
}