using System.Drawing;
using System.Threading.Tasks;
using NAPS2.Images.Storage;

namespace NAPS2.Images
{
    // TODO: Move this and all GDI stuff to a GDI namespace.
    // TODO: Also maybe this should be called GdiRenderer. Or GdiBitmapRenderer.
    public class BitmapRenderer : IScannedImageRenderer<Bitmap>
    {
        private readonly ImageContext _imageContext;
        private readonly IScannedImageRenderer<IImage> _imageRenderer;

        public BitmapRenderer(ImageContext imageContext)
        {
            _imageContext = imageContext;
            _imageRenderer = new ImageRenderer(imageContext);
        }

        public BitmapRenderer(ImageContext imageContext, IScannedImageRenderer<IImage> imageRenderer)
        {
            _imageContext = imageContext;
            _imageRenderer = imageRenderer;
        }

        public async Task<Bitmap> Render(ScannedImage image, int outputSize = 0)
        {
            return _imageContext.Convert<GdiImage>(await _imageRenderer.Render(image, outputSize)).Bitmap;
        }

        public async Task<Bitmap> Render(ScannedImage.Snapshot snapshot, int outputSize = 0)
        {
            return _imageContext.Convert<GdiImage>(await _imageRenderer.Render(snapshot, outputSize)).Bitmap;
        }
    }
}
