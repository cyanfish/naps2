using System.Drawing;
using System.Drawing.Imaging;
using NAPS2.Util;

namespace NAPS2.Images.Gdi;

#if NET6_0_OR_GREATER
[System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
#endif
internal class GdiTiffWriter : ITiffWriter
{
    public bool SaveTiff(IList<IMemoryImage> images, string path,
        TiffCompressionType compression = TiffCompressionType.Auto, ProgressHandler progress = default)
    {
        return SaveTiffInternal(images,
            (bitmap, codecInfo, encoderParams) => bitmap.Save(path, codecInfo, encoderParams),
            () => File.Delete(path), compression, progress);
    }

    public bool SaveTiff(IList<IMemoryImage> images, Stream stream,
        TiffCompressionType compression = TiffCompressionType.Auto, ProgressHandler progress = default)
    {
        return SaveTiffInternal(images,
            (bitmap, codecInfo, encoderParams) => bitmap.Save(stream, codecInfo, encoderParams),
            () => { }, compression, progress);
    }

    private bool SaveTiffInternal(IList<IMemoryImage> images, Action<Bitmap, ImageCodecInfo, EncoderParameters> save,
        Action cleanup, TiffCompressionType compression, ProgressHandler progress = default)
    {
        ImageCodecInfo codecInfo = GetCodecForString("TIFF");

        progress.Report(0, images.Count);
        if (progress.IsCancellationRequested)
        {
            return false;
        }

        using var image0 = GetImageToSave(images[0], compression);
        if (images.Count == 1)
        {
            save(image0.Bitmap, codecInfo, GetTiffParameters(compression, image0));
        }
        else if (images.Count > 1)
        {
            save(image0.Bitmap, codecInfo,
                GetTiffParameters(compression, images[0], Encoder.SaveFlag, EncoderValue.MultiFrame));

            for (int i = 1; i < images.Count; i++)
            {
                progress.Report(i, images.Count);
                if (progress.IsCancellationRequested)
                {
                    cleanup();
                    return false;
                }

                using var image = GetImageToSave(images[i], compression);
                image0.Bitmap.SaveAdd(image.Bitmap,
                    GetTiffParameters(compression, images[0], Encoder.SaveFlag, EncoderValue.FrameDimensionPage));
            }

            image0.Bitmap.SaveAdd(new EncoderParameters(1)
            {
                Param =
                {
                    [0] = new EncoderParameter(Encoder.SaveFlag, (long) EncoderValue.Flush)
                }
            });
        }
        return true;
    }

    private GdiImage GetImageToSave(IMemoryImage original, TiffCompressionType compression) {
         var image = original.Clone();
         if (compression == TiffCompressionType.Ccitt4)
         {
             image = image.PerformTransform(new BlackWhiteTransform());
         }
         return (GdiImage) image;
    }

    private EncoderParameters GetTiffParameters(TiffCompressionType compression, IMemoryImage image,
        Encoder? secondParam = null, EncoderValue? secondValue = null) =>
        secondParam != null && secondValue != null
            ? new(2)
            {
                Param =
                {
                    [0] = new EncoderParameter(Encoder.Compression,
                        (long) GetTiffCompressionValue(compression, image)),
                    [1] = new EncoderParameter(secondParam, (long) secondValue.Value)
                }
            }
            : new(1)
            {
                Param =
                {
                    [0] = new EncoderParameter(Encoder.Compression,
                        (long) GetTiffCompressionValue(compression, image))
                }
            };

    private EncoderValue GetTiffCompressionValue(TiffCompressionType compression, IMemoryImage image) =>
        compression switch
        {
            TiffCompressionType.None => EncoderValue.CompressionNone,
            TiffCompressionType.Ccitt4 => EncoderValue.CompressionCCITT4,
            TiffCompressionType.Lzw => EncoderValue.CompressionLZW,
            _ => image.PixelFormat == ImagePixelFormat.BW1
                ? EncoderValue.CompressionCCITT4
                : EncoderValue.CompressionLZW
        };

    private ImageCodecInfo GetCodecForString(string type)
    {
        ImageCodecInfo[] info = ImageCodecInfo.GetImageEncoders();
        return info.First(t => t.FormatDescription == type);
    }
}