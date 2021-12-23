using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Eto.Forms;
using Eto.WinForms;
using NAPS2.Images.Gdi;
using NAPS2.ImportExport.Images;

namespace NAPS2.WinForms;

public class ImageClipboard
{
    private readonly ImageTransfer _imageTransfer;

    public ImageClipboard(ImageContext imageContext)
    {
        _imageTransfer = new ImageTransfer(imageContext);
    }

    public ImageClipboard(ImageTransfer imageTransfer)
    {
        _imageTransfer = imageTransfer;
    }

    public async Task Write(IEnumerable<RenderableImage> images, bool includeBitmap)
    {
        var imageList = images.ToList();
        if (imageList.Count == 0)
        {
            return;
        }

        // Fast path for copying within NAPS2
        _imageTransfer.SetClipboard(imageList);

        // Slow path for more full-featured copying
        if (includeBitmap)
        {
            using var firstBitmap = imageList[0].RenderToBitmap();
            Clipboard.Instance.Image = firstBitmap.ToEto();
            Clipboard.Instance.SetString(await RtfEncodeImages(firstBitmap, imageList), "Rich Text Format");
        }
    }

    private async Task<string> RtfEncodeImages(Bitmap firstBitmap, List<RenderableImage> images)
    {
        var sb = new StringBuilder();
        sb.Append("{");
        // TODO: Is this the right format?
        if (!AppendRtfEncodedImage(firstBitmap, firstBitmap.RawFormat, sb, false))
        {
            return null;
        }
        foreach (var img in images.Skip(1))
        {
            using var bitmap = img.RenderToBitmap();
            // TODO: Is this the right format?
            if (!AppendRtfEncodedImage(bitmap, bitmap.RawFormat, sb, true))
            {
                break;
            }
        }
        sb.Append("}");
        return sb.ToString();
    }

    private static bool AppendRtfEncodedImage(Image image, ImageFormat format, StringBuilder sb, bool par)
    {
        const int maxRtfSize = 20 * 1000 * 1000;
        using (var stream = new MemoryStream())
        {
            image.Save(stream, format);
            if (sb.Length + stream.Length * 2 > maxRtfSize)
            {
                return false;
            }

            if (par)
            {
                sb.Append(@"\par");
            }
            sb.Append(@"{\pict\pngblip\picw");
            sb.Append(image.Width);
            sb.Append(@"\pich");
            sb.Append(image.Height);
            sb.Append(@"\picwgoa");
            sb.Append(image.Width);
            sb.Append(@"\pichgoa");
            sb.Append(image.Height);
            sb.Append(@"\hex ");
            // Do a "low-level" conversion to save on memory by avoiding intermediate representations
            stream.Seek(0, SeekOrigin.Begin);
            int value;
            while ((value = stream.ReadByte()) != -1)
            {
                int hi = value / 16, lo = value % 16;
                sb.Append(GetHexChar(hi));
                sb.Append(GetHexChar(lo));
            }
            sb.Append("}");
        }
        return true;
    }

    private static char GetHexChar(int n) => (char)(n < 10 ? '0' + n : 'A' + (n - 10));
}