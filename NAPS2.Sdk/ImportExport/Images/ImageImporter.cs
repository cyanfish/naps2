using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Logging;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Scan.Images.Storage;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;

namespace NAPS2.ImportExport.Images
{
    public class ImageImporter : IImageImporter
    {
        private readonly ThumbnailRenderer thumbnailRenderer;

        public ImageImporter(ThumbnailRenderer thumbnailRenderer)
        {
            this.thumbnailRenderer = thumbnailRenderer;
        }

        public ScannedImageSource Import(string filePath, ImportParams importParams, ProgressHandler progressCallback, CancellationToken cancelToken)
        {
            var source = new ScannedImageSource.Concrete();
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        source.Done();
                        return;
                    }

                    IEnumerable<IMemoryStorage> toImport;
                    int frameCount;
                    try
                    {
                        toImport = StorageManager.MemoryStorageFactory.DecodeMultiple(filePath, out frameCount);
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
                                source.Done();
                                return;
                            }
                            
                            var image = new ScannedImage(frame, ScanBitDepth.C24Bit, frame.IsOriginalLossless, -1);
                            if (!importParams.NoThumbnails)
                            {
                                image.SetThumbnail(StorageManager.PerformTransform(frame, new ThumbnailTransform()));
                            }

                            if (importParams.DetectPatchCodes)
                            {
                                image.PatchCode = PatchCodeDetector.Detect(frame);
                            }

                            source.Put(image);
                        }

                        progressCallback(frameCount, frameCount);
                    }

                    source.Done();
                }
                catch(Exception e)
                {
                    source.Error(e);
                }
            }, TaskCreationOptions.LongRunning);
            return source;
        }
    }
}
