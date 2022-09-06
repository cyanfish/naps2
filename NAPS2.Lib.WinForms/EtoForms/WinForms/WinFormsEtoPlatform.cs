using System.Drawing.Imaging;
using System.Windows.Forms;
using Eto.Drawing;
using Eto.Forms;
using Eto.WinForms;
using Eto.WinForms.Forms.Controls;
using NAPS2.Images.Gdi;
using sd = System.Drawing;

namespace NAPS2.EtoForms.WinForms;

public class WinFormsEtoPlatform : EtoPlatform
{
    private static readonly Size MinImageButtonSize = new(75, 32);
    private const int IMAGE_PADDING = 5;

    public override IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) =>
        new WinFormsListView<T>(behavior);

    public override void ConfigureImageButton(Eto.Forms.Button button)
    {
        button.MinimumSize = MinImageButtonSize;
        if (button.ImagePosition == ButtonImagePosition.Left)
        {
            var native = (System.Windows.Forms.Button) button.ToNative();
            native.TextImageRelation = TextImageRelation.Overlay;
            native.ImageAlign = sd.ContentAlignment.MiddleLeft;
            native.TextAlign = sd.ContentAlignment.MiddleRight;

            var imageWidth = native.Image.Width;
            using var g = native.CreateGraphics();
            var textWidth = (int) g.MeasureString(native.Text, native.Font).Width;
            native.AutoSize = false;

            var widthWithoutRightPadding = imageWidth + textWidth + IMAGE_PADDING + 15;
            native.Width = Math.Max(widthWithoutRightPadding + IMAGE_PADDING, ButtonHandler.DefaultMinimumSize.Width);
            var rightPadding = IMAGE_PADDING + (native.Width - widthWithoutRightPadding - IMAGE_PADDING) / 2;
            native.Padding = native.Padding with { Left = IMAGE_PADDING, Right = rightPadding };
        }
    }

    public override Bitmap ToBitmap(IMemoryImage image)
    {
        return ((GdiImage) image).Bitmap.ToEto();
    }

    public override IMemoryImage DrawHourglass(ImageContext imageContext, IMemoryImage image)
    {
        var bitmap = new System.Drawing.Bitmap(image.Width, image.Height);
        using (var g = sd.Graphics.FromImage(bitmap))
        {
            var attrs = new ImageAttributes();
            attrs.SetColorMatrix(new ColorMatrix
            {
                Matrix33 = 0.3f
            });
            g.DrawImage(image.AsBitmap(),
                new sd.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                0,
                0,
                image.Width,
                image.Height,
                sd.GraphicsUnit.Pixel,
                attrs);
            using var hourglass = new sd.Bitmap(new MemoryStream(Icons.hourglass_grey));
            g.DrawImage(hourglass, new sd.Rectangle((bitmap.Width - 32) / 2, (bitmap.Height - 32) / 2, 32, 32));
        }
        image.Dispose();
        return new GdiImage(imageContext, bitmap);
    }
}