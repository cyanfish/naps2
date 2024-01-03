using System.Windows.Media.Imaging;
using NAPS2.Util;

namespace NAPS2.Images.Wpf;

internal class WpfTiffWriter : ITiffWriter
{
    public bool SaveTiff(IList<IMemoryImage> images, string path,
        TiffCompressionType compression = TiffCompressionType.Auto, ProgressHandler progress = default)
    {
        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
        return SaveTiff(images, fileStream, compression, progress);
    }

    public bool SaveTiff(IList<IMemoryImage> images, Stream stream,
        TiffCompressionType compression = TiffCompressionType.Auto, ProgressHandler progress = default)
    {
        if (progress.IsCancellationRequested) return false;
        var tiffEncoder = new TiffBitmapEncoder
        {
            Compression = compression switch
            {
                TiffCompressionType.Ccitt4 => TiffCompressOption.Ccitt4,
                TiffCompressionType.Lzw => TiffCompressOption.Lzw,
                TiffCompressionType.None => TiffCompressOption.None,
                _ => TiffCompressOption.Default
            }
        };
        int i = 0;
        progress.Report(i, images.Count);
        foreach (var image in images)
        {
            image.UpdateLogicalPixelFormat();
            if (compression == TiffCompressionType.Ccitt4 && image.LogicalPixelFormat != ImagePixelFormat.BW1)
            {
                using var bwCopy = image.Clone().PerformTransform(new BlackWhiteTransform());
                tiffEncoder.Frames.Add(BitmapFrame.Create(((WpfImage) bwCopy).Bitmap));
            }
            else
            {
                tiffEncoder.Frames.Add(BitmapFrame.Create(((WpfImage) image).Bitmap));
            }
            progress.Report(++i, images.Count);
            if (progress.IsCancellationRequested) return false;
        }
        tiffEncoder.Save(stream);
        return true;
    }
}