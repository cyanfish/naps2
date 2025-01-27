using Microsoft.Extensions.Logging;
using NAPS2.Scan;

namespace NAPS2.ImportExport;

/// <summary>
/// Imports image files.
/// </summary>
public class ImageImporter
{
    private readonly ScanningContext _scanningContext;

    public ImageImporter(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public IAsyncEnumerable<ProcessedImage> Import(string filePath, ImportParams? importParams = null,
        ProgressHandler progress = default) =>
        Import(new InputPathOrStream(filePath, null, null), importParams, progress);

    public IAsyncEnumerable<ProcessedImage> Import(Stream stream, ImportParams? importParams = null,
        ProgressHandler progress = default) =>
        Import(new InputPathOrStream(null, stream, null), importParams, progress);

    internal IAsyncEnumerable<ProcessedImage> Import(InputPathOrStream input, ImportParams? importParams = null,
        ProgressHandler progress = default)
    {
        importParams ??= new ImportParams();
        return AsyncProducers.RunProducer<ProcessedImage>(async produceImage =>
        {
            if (progress.IsCancellationRequested) return;

            int frameCount = 0;
            try
            {
                var callback = new ProgressCallback((current, max) =>
                {
                    frameCount = max;
                    if (current == 0)
                    {
                        progress.Report(0, frameCount);
                    }
                });
                var toImport = input.Stream != null
                    ? _scanningContext.ImageContext.LoadFrames(input.Stream, callback)
                    : _scanningContext.ImageContext.LoadFrames(input.FilePath!, callback);

                int i = 0;
                await foreach (var frame in toImport)
                {
                    using (frame)
                    {
                        if (progress.IsCancellationRequested) return;

                        bool lossless = frame.OriginalFileFormat is ImageFileFormat.Bmp or ImageFileFormat.Png;
                        var image = _scanningContext.CreateProcessedImage(
                            frame.OriginalFileFormat == ImageFileFormat.Jpeg &&
                            (input.Stream == null || input.Stream.CanSeek)
                                ? CreateJpegStorageWithoutReEncoding(input, frame)
                                : frame,
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
                if (input.FilePath != null)
                {
                    _scanningContext.Logger.LogError(e, "Error importing image: {FilePath}", input.FilePath);
                }
                else
                {
                    _scanningContext.Logger.LogError(e, "Error importing image");
                }
                // Handle and notify the user outside the method so that errors importing multiple files can be aggregated
                throw;
            }
        });
    }

    private IImageStorage CreateJpegStorageWithoutReEncoding(InputPathOrStream input, IMemoryImage loadedImage)
    {
        if (_scanningContext.FileStorageManager == null)
        {
            return loadedImage;
        }
        var storagePath = _scanningContext.FileStorageManager.NextFilePath() + ".jpg";
        if (input.Stream != null)
        {
            // TODO: Technically we don't know if the stream we were given started at 0
            input.Stream.Position = 0;
        }
        input.CopyToFile(storagePath);
        return new ImageFileStorage(storagePath);
    }
}