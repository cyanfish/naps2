namespace NAPS2.Images;

public class ImageExportHelper
{
    public string SaveSmallestFormat(string pathWithoutExtension, IMemoryImage image, BitDepth bitDepth,
        bool lossless, int quality, out ImageFileFormat imageFileFormat)
    {
        // TODO: Should we save directly to the file?
        var memoryStream = SaveSmallestFormatToMemoryStream(image, bitDepth, lossless, quality, out imageFileFormat);
        var ext = imageFileFormat == ImageFileFormat.Png ? ".png" : ".jpg";
        var path = pathWithoutExtension + ext;
        using var fileStream = new FileStream(path, FileMode.Create);
        memoryStream.CopyTo(fileStream);
        return path;
    }

    public MemoryStream SaveSmallestFormatToMemoryStream(IMemoryImage image, BitDepth bitDepth, bool lossless,
        int quality, out ImageFileFormat imageFileFormat)
    {
        var exportFormat = GetExportFormat(image, bitDepth, lossless);
        if (exportFormat.FileFormat == ImageFileFormat.Png)
        {
            imageFileFormat = ImageFileFormat.Png;
            if (exportFormat.PixelFormat == ImagePixelFormat.BW1 && image.PixelFormat != ImagePixelFormat.BW1)
            {
                using var bwImage = image.Clone().PerformTransform(new BlackWhiteTransform());
                return bwImage.SaveToMemoryStream(ImageFileFormat.Png);
            }
            return image.SaveToMemoryStream(ImageFileFormat.Png);
        }
        if (exportFormat.FileFormat == ImageFileFormat.Jpeg)
        {
            imageFileFormat = ImageFileFormat.Jpeg;
            return image.SaveToMemoryStream(ImageFileFormat.Jpeg, quality);
        }
        // Save as PNG/JPEG depending on which is smaller
        var pngEncoded = image.SaveToMemoryStream(ImageFileFormat.Png);
        var jpegEncoded = image.SaveToMemoryStream(ImageFileFormat.Jpeg, quality);
        if (pngEncoded.Length <= jpegEncoded.Length)
        {
            // Probably a black and white image (e.g. from native WIA, where bitDepth is unknown), which PNG compresses well vs. JPEG
            imageFileFormat = ImageFileFormat.Png;
            return pngEncoded;
        }
        // Probably a color or grayscale image, which JPEG compresses well vs. PNG
        imageFileFormat = ImageFileFormat.Jpeg;
        return jpegEncoded;
    }

    public ImageExportFormat GetExportFormat(IMemoryImage image, BitDepth bitDepth, bool lossless)
    {
        // Store the image in as little space as possible
        if (image.PixelFormat == ImagePixelFormat.BW1)
        {
            // Already encoded as 1-bit
            return new ImageExportFormat(ImageFileFormat.Png, ImagePixelFormat.BW1);
        }
        if (bitDepth == BitDepth.BlackAndWhite)
        {
            // Convert to a 1-bit bitmap before saving to help compression
            // This is lossless and takes up minimal storage (best of both worlds), so highQuality is irrelevant
            // Note that if a black and white image comes from native WIA, bitDepth is unknown,
            // so the image will be png-encoded below instead of using a 1-bit bitmap
            return new ImageExportFormat(ImageFileFormat.Png, ImagePixelFormat.BW1);
        }
        // TODO: Also for ARGB32? Or is OriginalFileFormat enough if we populate that more consistently?
        if (lossless || image.OriginalFileFormat == ImageFileFormat.Png)
        {
            // Store as PNG
            // Lossless, but some images (color/grayscale) take up lots of storage
            return new ImageExportFormat(ImageFileFormat.Png, image.PixelFormat);
        }
        if (image.OriginalFileFormat == ImageFileFormat.Jpeg)
        {
            // Store as JPEG
            // Since the image was originally in JPEG format, PNG is unlikely to have size benefits
            return new ImageExportFormat(ImageFileFormat.Jpeg, ImagePixelFormat.RGB24);
        }
        // No inherent preference for Jpeg or Png, the caller can decide
        return new ImageExportFormat(ImageFileFormat.Unspecified, ImagePixelFormat.RGB24);
    }
}