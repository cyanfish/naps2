using System.Text;
using Eto.Forms;
using NAPS2.EtoForms;
using NAPS2.ImportExport.Images;

namespace NAPS2.ImportExport;

public class ImageClipboard
{
    private readonly ImageTransfer _imageTransfer;

    public ImageClipboard() : this(new ImageTransfer())
    {
    }

    public ImageClipboard(ImageTransfer imageTransfer)
    {
        _imageTransfer = imageTransfer;
    }

    public async Task Write(IEnumerable<ProcessedImage> images, bool includeBitmap)
    {
        var imageList = images.ToList();
        if (imageList.Count == 0)
        {
            return;
        }

        // Fast path for copying within NAPS2
        _imageTransfer.SetClipboard(imageList);

        if (includeBitmap)
        {
            // Slow path for more full-featured copying, expensive parts not run on UI thread
            using var firstBitmap = await Task.Run(() => imageList[0].Render());
            EtoPlatform.Current.SetClipboardImage(Clipboard.Instance, firstBitmap.ToEtoImage());
            var encodedRtf = await Task.Run(() => RtfEncodeImages(firstBitmap, imageList));
            if (encodedRtf != null)
            {
                Clipboard.Instance.SetString(encodedRtf, "Rich Text Format");
            }
        }
    }

    private string? RtfEncodeImages(IMemoryImage firstBitmap, List<ProcessedImage> images)
    {
        var sb = new StringBuilder();
        sb.Append("{");
        if (!AppendRtfEncodedImage(firstBitmap, sb, false))
        {
            return null;
        }
        foreach (var img in images.Skip(1))
        {
            using var bitmap = img.Render();
            if (!AppendRtfEncodedImage(bitmap, sb, true))
            {
                break;
            }
        }
        sb.Append("}");
        return sb.ToString();
    }

    private static bool AppendRtfEncodedImage(IMemoryImage image, StringBuilder sb, bool par)
    {
        // TODO: Use a variation of SaveSmallestFormat here
        var format = image.OriginalFileFormat == ImageFileFormat.Jpeg ? ImageFileFormat.Jpeg : ImageFileFormat.Png;
        const int maxRtfSize = 20 * 1000 * 1000;
        using var stream = new MemoryStream();
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
        return true;
    }

    private static char GetHexChar(int n) => (char)(n < 10 ? '0' + n : 'A' + (n - 10));
}