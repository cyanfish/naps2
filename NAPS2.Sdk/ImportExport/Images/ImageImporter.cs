using NAPS2.Scan;

namespace NAPS2.ImportExport.Images;

public class ImageImporter : IImageImporter
{
    private readonly ScanningContext _scanningContext;
    private readonly ImageContext _imageContext;
    private readonly ImportPostProcessor _importPostProcessor;

    public ImageImporter(ScanningContext scanningContext, ImageContext imageContext, ImportPostProcessor importPostProcessor)
    {
        _scanningContext = scanningContext;
        _imageContext = imageContext;
        _importPostProcessor = importPostProcessor;
    }

    public AsyncSource<ProcessedImage> Import(string filePath, ImportParams importParams, ProgressHandler progress = default)
    {
        var sink = new AsyncSink<ProcessedImage>();
        Task.Run(() =>
        {
            try
            {
                if (progress.IsCancellationRequested)
                {
                    sink.SetCompleted();
                    return;
                }

                IEnumerable<IMemoryImage> toImport;
                int frameCount;
                try
                {
                    toImport = _imageContext.LoadFrames(filePath, out frameCount);
                }
                catch (Exception e)
                {
                    Log.ErrorException("Error importing image: " + filePath, e);
                    // Handle and notify the user outside the method so that errors importing multiple files can be aggregated
                    throw;
                }
                
                progress.Report(0, frameCount);

                int i = 0;
                foreach (var frame in toImport)
                {
                    using (frame)
                    {
                        if (progress.IsCancellationRequested)
                        {
                            sink.SetCompleted();
                            return;
                        }

                        bool lossless = frame.OriginalFileFormat is ImageFileFormat.Bmp or ImageFileFormat.Png;
                        var image = _scanningContext.CreateProcessedImage(
                            frame,
                            BitDepth.Color,
                            lossless,
                            -1);
                        image = _importPostProcessor.AddPostProcessingData(
                            image,
                            frame,
                            importParams.ThumbnailSize,
                            importParams.BarcodeDetectionOptions,
                            true);

                        progress.Report(++i, frameCount);
                        sink.PutItem(image);
                    }
                }

                sink.SetCompleted();
            }
            catch(Exception e)
            {
                sink.SetError(e);
            }
        });
        return sink.AsSource();
    }
}