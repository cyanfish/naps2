using Eto.Drawing;
using Eto.Forms;
using NAPS2.Images.Bitwise;

namespace NAPS2.EtoForms;

public static class EtoExtensions
{
    public static Icon ToEtoIcon(this byte[] bytes) => new(new MemoryStream(bytes));

    public static Bitmap ToEtoImage(this byte[] bytes) => new(bytes);
    public static Bitmap ToEtoImage(this IMemoryImage image) => EtoPlatform.Current.ToBitmap(image);

    public static Bitmap Clone(this Image image) => new(image);

    public static MessageBoxType ToEto(this MessageBoxIcon icon)
    {
        return icon switch
        {
            MessageBoxIcon.Information => MessageBoxType.Information,
            MessageBoxIcon.Warning => MessageBoxType.Warning,
            _ => MessageBoxType.Information // TODO: Default type with no icon?
        };
    }

    public static DockPosition ToEto(this DockStyle dock)
    {
        return dock switch
        {
            DockStyle.Bottom => DockPosition.Bottom,
            DockStyle.Left => DockPosition.Left,
            DockStyle.Right => DockPosition.Right,
            _ => DockPosition.Top
        };
    }

    public static void AddItems(this ContextMenu contextMenu, params MenuItem[] menuItems)
    {
        contextMenu.Items.AddRange(menuItems);
    }

    public static bool IsChecked(this CheckBox checkBox)
    {
        return checkBox.Checked == true;
    }

    public static void Fill(this IMemoryImage image, Color color)
    {
        new FillColorImageOp((byte) color.Rb, (byte) color.Gb, (byte) color.Bb, (byte) color.Ab).Perform(image);
    }

    public static Image PadTo(this Image image, Size size)
    {
        bool fits = image.Width <= size.Width && image.Height <= size.Height;
        bool needsPadding = image.Height < size.Height || image.Width < size.Width;
        if (fits && needsPadding)
        {
            var newImage = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppRgba);
            using var graphics = new Graphics(newImage);
            graphics.Clear(Colors.Transparent);
            graphics.DrawImage(image, (size.Width - image.Width) / 2f, (size.Height - image.Height) / 2f);
            image.Dispose();
            return newImage;
        }
        return image;
    }

    public static Bitmap ResizeTo(this Bitmap image, int size) => ResizeTo(image, new Size(size, size));

    public static Bitmap ResizeTo(this Bitmap image, int width, int height) => ResizeTo(image, new Size(width, height));

    public static Bitmap ResizeTo(this Bitmap image, Size size)
    {
        if (image.Width != size.Width || image.Height != size.Height)
        {
            var newImage = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppRgba);
            using var graphics = new Graphics(newImage);
            graphics.Clear(Colors.Transparent);
            graphics.DrawImage(image, 0, 0, size.Width, size.Height);
            image.Dispose();
            return newImage;
        }
        return image;
    }
}