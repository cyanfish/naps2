using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using NAPS2.ImportExport.Pdf;
using NAPS2.Recovery;
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

        public Bitmap Render(ScannedImage image)
        {
            var bitmap = image.FileFormat == null
                ? pdfRenderer.Render(image.RecoveryFilePath).Single()
                : new Bitmap(image.RecoveryFilePath);
            lock (image.RecoveryIndexImage.TransformList)
            {
                return Transform.PerformAll(bitmap, image.RecoveryIndexImage.TransformList);
            }
        }

        public Stream RenderToStream(ScannedImage image)
        {
            using (var transformed = Render(image))
            {
                var stream = new MemoryStream();
                var format = transformed.PixelFormat == PixelFormat.Format1bppIndexed
                    ? ImageFormat.Png
                    : image.FileFormat ?? (image.RecoveryIndexImage.HighQuality ? ImageFormat.Png : ImageFormat.Jpeg);
                transformed.Save(stream, format);
                return stream;
            }
        }
    }
}
