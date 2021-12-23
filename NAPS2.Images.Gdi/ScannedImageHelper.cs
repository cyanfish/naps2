using System.Drawing;
using System.Drawing.Imaging;
namespace NAPS2.Images.Gdi;

// TODO: Generalize as much as possible
public class ScannedImageHelper
{
    public static string SaveSmallestBitmap(Bitmap sourceImage, string pathWithoutExt, BitDepth bitDepth, bool highQuality, int quality, out ImageFormat imageFormat)
    {
        // Store the image in as little space as possible
        if (sourceImage.PixelFormat == PixelFormat.Format1bppIndexed)
        {
            // Already encoded as 1-bit
            imageFormat = ImageFormat.Png;
            return EncodePng(sourceImage, pathWithoutExt);
        }
        else if (bitDepth == BitDepth.BlackAndWhite)
        {
            // Convert to a 1-bit bitmap before saving to help compression
            // This is lossless and takes up minimal storage (best of both worlds), so highQuality is irrelevant
            using var bitmap = BitmapHelper.CopyToBpp(sourceImage, 1);
            imageFormat = ImageFormat.Png;
            return EncodePng(bitmap, pathWithoutExt);
            // Note that if a black and white image comes from native WIA, bitDepth is unknown,
            // so the image will be png-encoded below instead of using a 1-bit bitmap
        }
        else if (highQuality)
        {
            // Store as PNG
            // Lossless, but some images (color/grayscale) take up lots of storage
            imageFormat = ImageFormat.Png;
            return EncodePng(sourceImage, pathWithoutExt);
        }
        else if (Equals(sourceImage.RawFormat, ImageFormat.Jpeg))
        {
            // Store as JPEG
            // Since the image was originally in JPEG format, PNG is unlikely to have size benefits
            imageFormat = ImageFormat.Jpeg;
            return EncodeJpeg(sourceImage, pathWithoutExt, quality);
        }
        else
        {
            // Store as PNG/JPEG depending on which is smaller
            var pngEncoded = EncodePng(sourceImage, pathWithoutExt);
            var jpegEncoded = EncodeJpeg(sourceImage, pathWithoutExt, quality);
            if (new FileInfo(pngEncoded).Length <= new FileInfo(jpegEncoded).Length)
            {
                // Probably a black and white image (from native WIA, so bitDepth is unknown), which PNG compresses well vs. JPEG
                File.Delete(jpegEncoded);
                imageFormat = ImageFormat.Png;
                return pngEncoded;
            }
            else
            {
                // Probably a color or grayscale image, which JPEG compresses well vs. PNG
                File.Delete(pngEncoded);
                imageFormat = ImageFormat.Jpeg;
                return jpegEncoded;
            }
        }
    }

    private static string EncodePng(Bitmap bitmap, string pathWithoutExt)
    {
        var path = pathWithoutExt + ".png";
        bitmap.Save(path, ImageFormat.Png);
        return path;
    }

    private static string EncodeJpeg(Bitmap bitmap, string pathWithoutExt, int quality)
    {
        var path = pathWithoutExt + ".jpg";
        if (quality == -1)
        {
            bitmap.Save(path, ImageFormat.Jpeg);
        }
        else
        {
            quality = Math.Max(Math.Min(quality, 100), 0);
            var encoder = ImageCodecInfo.GetImageEncoders().First(x => x.FormatID == ImageFormat.Jpeg.Guid);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            bitmap.Save(path, encoder, encoderParams);
        }
        return path;
    }
}