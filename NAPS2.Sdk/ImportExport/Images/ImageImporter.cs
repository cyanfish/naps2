using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Logging;
using NAPS2.Scan;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;
using NAPS2.Util;

namespace NAPS2.ImportExport.Images
{
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

                    IEnumerable<IImage> toImport;
                    int frameCount;
                    try
                    {
                        toImport = _imageContext.ImageFactory.DecodeMultiple(filePath, out frameCount);
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
                            
                            var image = _imageContext.CreateScannedImage(frame, BitDepth.Color, frame.IsOriginalLossless, -1);
                            if (importParams.ThumbnailSize.HasValue)
                            {
                                image.SetThumbnail(_imageContext.PerformTransform(frame, new ThumbnailTransform(importParams.ThumbnailSize.Value)));
                            }
                            image.BarcodeDetection = BarcodeDetection.Detect(frame, importParams.BarcodeDetectionOptions);

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
}
