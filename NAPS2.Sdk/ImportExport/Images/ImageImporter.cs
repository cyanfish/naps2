using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Logging;
using NAPS2.Scan;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;
using NAPS2.Scan.Experimental;
using NAPS2.Util;

namespace NAPS2.ImportExport.Images
{
    public class ImageImporter : IImageImporter
    {
        private readonly ImageContext imageContext;
        private readonly ThumbnailRenderer thumbnailRenderer;

        public ImageImporter(ImageContext imageContext, ThumbnailRenderer thumbnailRenderer)
        {
            this.imageContext = imageContext;
            this.thumbnailRenderer = thumbnailRenderer;
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
                        toImport = imageContext.ImageFactory.DecodeMultiple(filePath, out frameCount);
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
                            
                            var image = imageContext.CreateScannedImage(frame, BitDepth.Color, frame.IsOriginalLossless, -1);
                            if (importParams.ThumbnailSize.HasValue)
                            {
                                image.SetThumbnail(imageContext.PerformTransform(frame, new ThumbnailTransform(importParams.ThumbnailSize.Value)));
                            }

                            if (importParams.DetectPatchCodes)
                            {
                                image.PatchCode = PatchCodeDetector.Detect(frame);
                            }

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
