using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NAPS2.Scan;

namespace NAPS2.Images;

public class ScannedImageHelper
{
    public static string SaveSmallestBitmap(Bitmap sourceImage, BitDepth bitDepth, bool highQuality, int quality, out ImageFormat imageFormat)
    {
        // Store the image in as little space as possible
        if (sourceImage.PixelFormat == PixelFormat.Format1bppIndexed)
        {
            // Already encoded as 1-bit
            imageFormat = ImageFormat.Png;
            return EncodePng(sourceImage);
        }
        else if (bitDepth == BitDepth.BlackAndWhite)
        {
            // Convert to a 1-bit bitmap before saving to help compression
            // This is lossless and takes up minimal storage (best of both worlds), so highQuality is irrelevant
            using var bitmap = BitmapHelper.CopyToBpp(sourceImage, 1);
            imageFormat = ImageFormat.Png;
            return EncodePng(bitmap);
            // Note that if a black and white image comes from native WIA, bitDepth is unknown,
            // so the image will be png-encoded below instead of using a 1-bit bitmap
        }
        else if (highQuality)
        {
            // Store as PNG
            // Lossless, but some images (color/grayscale) take up lots of storage
            imageFormat = ImageFormat.Png;
            return EncodePng(sourceImage);
        }
        else if (Equals(sourceImage.RawFormat, ImageFormat.Jpeg))
        {
            // Store as JPEG
            // Since the image was originally in JPEG format, PNG is unlikely to have size benefits
            imageFormat = ImageFormat.Jpeg;
            return EncodeJpeg(sourceImage, quality);
        }
        else
        {
            // Store as PNG/JPEG depending on which is smaller
            var pngEncoded = EncodePng(sourceImage);
            var jpegEncoded = EncodeJpeg(sourceImage, quality);
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

    private static string GetTempFilePath()
    {
        return Path.Combine(Paths.Temp, Path.GetRandomFileName());
    }

    private static string EncodePng(Bitmap bitmap)
    {
        var tempFilePath = GetTempFilePath();
        bitmap.Save(tempFilePath, ImageFormat.Png);
        return tempFilePath;
    }

    private static string EncodeJpeg(Bitmap bitmap, int quality)
    {
        var tempFilePath = GetTempFilePath();
        if (quality == -1)
        {
            bitmap.Save(tempFilePath, ImageFormat.Jpeg);
        }
        else
        {
            quality = Math.Max(Math.Min(quality, 100), 0);
            var encoder = ImageCodecInfo.GetImageEncoders().First(x => x.FormatID == ImageFormat.Jpeg.Guid);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            bitmap.Save(tempFilePath, encoder, encoderParams);
        }
        return tempFilePath;
    }
}