using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan.Images.Transforms;

namespace NAPS2.Scan.Images
{
    public class ScannedImageRenderer
    {
        private readonly IPdfRenderer pdfRenderer;

        public ScannedImageRenderer(IPdfRenderer pdfRenderer)
        {
            this.pdfRenderer = pdfRenderer;
        }

        public async Task<Bitmap> Render(ScannedImage image, int outputSize = 0)
        {
            using (var snapshot = image.Preserve())
            {
                return await Render(snapshot, outputSize);
            }
        }

        public async Task<Bitmap> Render(ScannedImage.Snapshot snapshot, int outputSize = 0)
        {
            return await Task.Factory.StartNew(() =>
            {
                var bitmap = snapshot.Source.FileFormat == null
                    ? pdfRenderer.Render(snapshot.Source.RecoveryFilePath).Single()
                    : new Bitmap(snapshot.Source.RecoveryFilePath);
                if (outputSize > 0)
                {
                    bitmap = ShrinkBitmap(bitmap, outputSize);
                }
                return Transform.PerformAll(bitmap, snapshot.TransformList);
            });
        }

        private Bitmap ShrinkBitmap(Bitmap bitmap, int outputSize)
        {
            double scaleFactor = Math.Min(outputSize / (double)bitmap.Height, outputSize / (double)bitmap.Width);
            if (scaleFactor >= 1)
            {
                return bitmap;
            }
            var bitmap2 = new Bitmap((int)Math.Round(bitmap.Width * scaleFactor), (int)Math.Round(bitmap.Height * scaleFactor));
            using (var g = Graphics.FromImage(bitmap2))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(bitmap, new Rectangle(Point.Empty, bitmap2.Size), new Rectangle(Point.Empty, bitmap.Size), GraphicsUnit.Pixel);
            }
            bitmap.Dispose();
            return bitmap2;
        }

        public async Task<Stream> RenderToStream(ScannedImage image)
        {
            using (var snapshot = image.Preserve())
            {
                return await RenderToStream(snapshot);
            }
        }

        public async Task<Stream> RenderToStream(ScannedImage.Snapshot snapshot)
        {
            using (var transformed = await Render(snapshot))
            {
                var stream = new MemoryStream();
                var format = transformed.PixelFormat == PixelFormat.Format1bppIndexed
                    ? ImageFormat.Png
                    : snapshot.Source.FileFormat ?? (snapshot.Source.RecoveryIndexImage.HighQuality ? ImageFormat.Png : ImageFormat.Jpeg);
                transformed.Save(stream, format);
                return stream;
            }
        }
    }
}
