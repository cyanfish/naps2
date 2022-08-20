using System.Drawing;
using System.Drawing.Imaging;
using NAPS2.Images.Gdi;

namespace NAPS2.Images;

/// <summary>
/// Gets the actual thumbnail image to be shown in the UI. This might be a placeholder hourglass indicator if the
/// real thumbnail isn't available yet, or an overlaid hourglass if it's out of date.
/// </summary>
public class UiThumbnailProvider
{
    private Bitmap? _placeholder;

    public Bitmap GetThumbnail(UiImage img, int thumbnailSize)
    {
        lock (img)
        {
            var thumb = ((GdiImage?) img.GetThumbnailClone())?.Bitmap;
            if (thumb == null)
            {
                return RenderPlaceholder(thumbnailSize);
            }
            if (img.IsThumbnailDirty)
            {
                thumb = DrawHourglass(thumb);
            }
            return thumb;
        }
    }

    private Bitmap RenderPlaceholder(int thumbnailSize)
    {
        lock (this)
        {
            if (_placeholder?.Size.Width == thumbnailSize)
            {
                return _placeholder;
            }
            _placeholder?.Dispose();
            _placeholder = new Bitmap(thumbnailSize, thumbnailSize);
            _placeholder = DrawHourglass(_placeholder);
            return _placeholder;
        }
    }

    private Bitmap DrawHourglass(Image image)
    {
        var bitmap = new Bitmap(image.Width, image.Height);
        using (var g = Graphics.FromImage(bitmap))
        {
            var attrs = new ImageAttributes();
            attrs.SetColorMatrix(new ColorMatrix
            {
                Matrix33 = 0.3f
            });
            g.DrawImage(image,
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                0,
                0,
                image.Width,
                image.Height,
                GraphicsUnit.Pixel,
                attrs);
            g.DrawImage(new Bitmap(new MemoryStream(Icons.hourglass_grey)), new Rectangle((bitmap.Width - 32) / 2, (bitmap.Height - 32) / 2, 32, 32));
        }
        image.Dispose();
        return bitmap;
    }
}