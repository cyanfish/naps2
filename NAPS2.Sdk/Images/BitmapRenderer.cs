using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Images.Storage;

namespace NAPS2.Images
{
    // TODO: Move this and all GDI stuff to a GDI namespace.
    // TODO: Also maybe this should be called GdiRenderer. Or GdiBitmapRenderer.
    public class BitmapRenderer : IScannedImageRenderer<Bitmap>
    {
        private readonly IScannedImageRenderer<IImage> imageRenderer;

        public BitmapRenderer()
        {
            imageRenderer = new ImageRenderer();
        }

        public BitmapRenderer(IScannedImageRenderer<IImage> imageRenderer)
        {
            this.imageRenderer = imageRenderer;
        }

        public async Task<Bitmap> Render(ScannedImage image, int outputSize = 0)
        {
            return StorageManager.Convert<GdiImage>(await imageRenderer.Render(image, outputSize)).Bitmap;
        }

        public async Task<Bitmap> Render(ScannedImage.Snapshot snapshot, int outputSize = 0)
        {
            return StorageManager.Convert<GdiImage>(await imageRenderer.Render(snapshot, outputSize)).Bitmap;
        }
    }
}
