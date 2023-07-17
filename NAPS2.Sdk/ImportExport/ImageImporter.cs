using Microsoft.Extensions.Logging;
using NAPS2.Scan;

namespace NAPS2.ImportExport;

public class ImageImporter : IImageImporter
{
    private readonly ScanningContext _scanningContext;

    public ImageImporter(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public IAsyncEnumerable<ProcessedImage> Import(string filePath, ImportParams importParams,
        ProgressHandler progress = default)
    {
        return AsyncProducers.RunProducer<ProcessedImage>(async produceImage =>
        {
            if (progress.IsCancellationRequested) return;

            int frameCount = 0;
            try
            {
                var toImport =
                    _scanningContext.ImageContext.LoadFrames(filePath, new ProgressCallback((current, max) =>
                    {
                        frameCount = max;
                        if (current == 0)
                        {
                            progress.Report(0, frameCount);
                        }
                    }));

                int i = 0;
                await foreach (var frame in toImport)
                {
                    using (frame)
                    {
                        if (progress.IsCancellationRequested) return;

                        bool lossless = frame.OriginalFileFormat is ImageFileFormat.Bmp or ImageFileFormat.Png;
                        var image = _scanningContext.CreateProcessedImage(
                            frame.OriginalFileFormat == ImageFileFormat.Jpeg
                                ? CreateJpegStorageWithoutReEncoding(filePath, frame)
                                : frame,
                            BitDepth.Color,
                            lossless,
                            -1,
                            null);
                        image = ImportPostProcessor.AddPostProcessingData(
                            image,
                            frame,
                            importParams.ThumbnailSize,
                            importParams.BarcodeDetectionOptions,
                            true);

                        progress.Report(++i, frameCount);
                        produceImage(image);
                    }
                }
            }
            catch (Exception e)
            {
                _scanningContext.Logger.LogError(e, "Error importing image: {FilePath}", filePath);
                // Handle and notify the user outside the method so that errors importing multiple files can be aggregated
                throw;
            }
        });
    }

    private IImageStorage CreateJpegStorageWithoutReEncoding(string originalPath, IMemoryImage loadedImage)
    {
        if (_scanningContext.FileStorageManager == null)
        {
            return loadedImage;
        }
        var storagePath = _scanningContext.FileStorageManager.NextFilePath() + ".jpg";
        File.Copy(originalPath, storagePath);
        return new ImageFileStorage(storagePath);
    }
}