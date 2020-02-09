using System.Drawing;
using System.Threading.Tasks;
using NAPS2.Images.Storage;

namespace NAPS2.Images
{
    // TODO: Move this and all GDI stuff to a GDI namespace.
    // TODO: Also maybe this should be called GdiRenderer. Or GdiBitmapRenderer.
    public class BitmapRenderer : IScannedImageRenderer<Bitmap>
    {
        private readonly ImageContext imageContext;
        private readonly IScannedImageRenderer<IImage> imageRenderer;

        public BitmapRenderer(ImageContext imageContext)
        {
            this.imageContext = imageContext;
            imageRenderer = new ImageRenderer(imageContext);
        }

        public BitmapRenderer(ImageContext imageContext, IScannedImageRenderer<IImage> imageRenderer)
        {
            this.imageContext = imageContext;
            this.imageRenderer = imageRenderer;
        }

        public async Task<Bitmap> Render(ScannedImage image, int outputSize = 0)
        {
            return imageContext.Convert<GdiImage>(await imageRenderer.Render(image, outputSize)).Bitmap;
        }

        public async Task<Bitmap> Render(ScannedImage.Snapshot snapshot, int outputSize = 0)
        {
            return imageContext.Convert<GdiImage>(await imageRenderer.Render(snapshot, outputSize)).Bitmap;
        }
    }
}
