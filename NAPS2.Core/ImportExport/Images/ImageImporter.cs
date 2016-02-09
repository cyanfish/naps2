using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
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

        public IEnumerable<ScannedImage> Import(string filePath, Func<int, int, bool> progressCallback)
        {
            if (!progressCallback(0, 1))
            {
                yield break;
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
                for (int i = 0; i < frameCount; ++i)
                {
                    if (!progressCallback(i, frameCount))
                    {
                        yield break;
                    }
                    toImport.SelectActiveFrame(FrameDimension.Page, i);
                    var image = new ScannedImage(toImport, ScanBitDepth.C24Bit, IsLossless(toImport.RawFormat), -1);
                    image.SetThumbnail(thumbnailRenderer.RenderThumbnail(toImport));
                    yield return image;
                }
                progressCallback(frameCount, frameCount);
            }
        }

        private bool IsLossless(ImageFormat format)
        {
            return Equals(format, ImageFormat.Bmp) || Equals(format, ImageFormat.Png);
        }
    }
}
