using System;
using System.Collections.Generic;
using System.Drawing;
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

        public async Task<Bitmap> Render(ScannedImage image)
        {
            using (var snapshot = image.Preserve())
            {
                return await Render(snapshot);
            }
        }

        public async Task<Bitmap> Render(ScannedImage.Snapshot snapshot)
        {
            return await Task.Factory.StartNew(() =>
            {
                var bitmap = snapshot.Source.FileFormat == null
                    ? pdfRenderer.Render(snapshot.Source.RecoveryFilePath).Single()
                    : new Bitmap(snapshot.Source.RecoveryFilePath);
                return Transform.PerformAll(bitmap, snapshot.TransformList);
            });
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
