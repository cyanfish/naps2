using System.Threading;
using NAPS2.Scan;

namespace NAPS2.ImportExport.Images;

public class ImageImporter : IImageImporter
{
    private readonly ImageContext _imageContext;
    private readonly ThumbnailRenderer _thumbnailRenderer;

    public ImageImporter(ImageContext imageContext, ThumbnailRenderer thumbnailRenderer)
    {
        _imageContext = imageContext;
        _thumbnailRenderer = thumbnailRenderer;
    }

    public ScannedImageSource Import(string filePath, ImportParams importParams, ProgressHandler progressCallback, CancellationToken cancelToken)
    {
        var sink = new ScannedImageSink();
        Task.Run(() =>
        {
            try
            {
                if (cancelToken.IsCancellationRequested)
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

                foreach (var frame in toImport)
                {
                    using (frame)
                    {
                        int i = 0;
                        progressCallback(i++, frameCount);
                        if (cancelToken.IsCancellationRequested)
                        {
                            sink.SetCompleted();
                            return;
                        }

                        bool lossless = frame.OriginalFileFormat is ImageFileFormat.Bmp or ImageFileFormat.Png;
                        // TODO: This is a similar pattern as the Pdf importer, consider abstracting
                        var image = new RenderableImage(frame, new ImageMetadata(BitDepth.Color, lossless), TransformState.Empty);
                        if (importParams.ThumbnailSize.HasValue)
                        {
                            image.PostProcessingData.Thumbnail = _imageContext.PerformTransform(frame, new ThumbnailTransform(importParams.ThumbnailSize.Value));
                        }
                        image.PostProcessingData.BarcodeDetection = BarcodeDetector.Detect(frame, importParams.BarcodeDetectionOptions);

                        sink.PutImage(image);
                    }

                    progressCallback(frameCount, frameCount);
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