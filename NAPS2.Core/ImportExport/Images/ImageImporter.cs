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

                    Bitmap toImport;
                    try
                    {
                        toImport = new Bitmap(filePath);
                    }
                    catch (Exception e)
                    {
                        Log.ErrorException("Error importing image: " + filePath, e);
                        // Handle and notify the user outside the method so that errors importing multiple files can be aggregated
                        throw;
                    }

                    using (toImport)
                    {
                        int frameCount = toImport.GetFrameCount(FrameDimension.Page);
                        int i = 0;
                        foreach (var frameIndex in importParams.Slice.Indices(frameCount))
                        {
                            progressCallback(i++, frameCount);
                            if (cancelToken.IsCancellationRequested)
                            {
                                source.Done();
                                return;
                            }

                            toImport.SelectActiveFrame(FrameDimension.Page, frameIndex);
                            var image = new ScannedImage(toImport, ScanBitDepth.C24Bit, IsLossless(toImport.RawFormat), -1);
                            if (!importParams.NoThumbnails)
                            {
                                image.SetThumbnail(thumbnailRenderer.RenderThumbnail(toImport));
                            }
                            if (importParams.DetectPatchCodes)
                            {
                                image.PatchCode = PatchCodeDetector.Detect(toImport);
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

        private bool IsLossless(ImageFormat format)
        {
            return Equals(format, ImageFormat.Bmp) || Equals(format, ImageFormat.Png);
        }
    }
}
