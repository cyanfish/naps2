namespace NAPS2.Images;

internal static class ImageExportHelper
{
    public static string SaveSmallestFormat(string pathWithoutExtension, IMemoryImage image,
        bool lossless, int quality, out ImageFileFormat imageFileFormat)
    {
        // TODO: Should we save directly to the file?
        var memoryStream = SaveSmallestFormatToMemoryStream(image, lossless, quality, out imageFileFormat);
        var ext = imageFileFormat == ImageFileFormat.Png ? ".png" : ".jpg";
        var path = pathWithoutExtension + ext;
        using var fileStream = new FileStream(path, FileMode.Create);
        memoryStream.CopyTo(fileStream);
        return path;
    }

    public static MemoryStream SaveSmallestFormatToMemoryStream(IMemoryImage image, bool lossless,
        int quality, out ImageFileFormat imageFileFormat)
    {
        var exportFormat = GetExportFormat(image, lossless);
        if (exportFormat.FileFormat == ImageFileFormat.Png)
        {
            imageFileFormat = ImageFileFormat.Png;
            return image.SaveToMemoryStream(ImageFileFormat.Png);
        }
        if (exportFormat.FileFormat == ImageFileFormat.Jpeg)
        {
            imageFileFormat = ImageFileFormat.Jpeg;
            return image.SaveToMemoryStream(ImageFileFormat.Jpeg, new ImageSaveOptions { Quality = quality });
        }
        // Save as PNG/JPEG depending on which is smaller
        var pngEncoded = image.SaveToMemoryStream(ImageFileFormat.Png);
        var jpegEncoded = image.SaveToMemoryStream(ImageFileFormat.Jpeg, new ImageSaveOptions { Quality = quality });
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

    public static ImageExportFormat GetExportFormat(IMemoryImage image, bool lossless)
    {
        image.UpdateLogicalPixelFormat();
        // Store the image in as little space as possible
        if (image.LogicalPixelFormat == ImagePixelFormat.BW1)
        {
            // Already 1-bit
            return new ImageExportFormat(ImageFileFormat.Png, ImagePixelFormat.BW1);
        }
        if (lossless || image.LogicalPixelFormat == ImagePixelFormat.ARGB32 ||
            image.OriginalFileFormat == ImageFileFormat.Png)
        {
            // Store as PNG
            // Lossless, but some images (color/grayscale) take up lots of storage
            return new ImageExportFormat(ImageFileFormat.Png, image.LogicalPixelFormat);
        }
        if (image.OriginalFileFormat == ImageFileFormat.Jpeg)
        {
            // Store as JPEG
            // Since the image was originally in JPEG format, PNG is unlikely to have size benefits
            return new ImageExportFormat(ImageFileFormat.Jpeg, ImagePixelFormat.RGB24);
        }
        // No inherent preference for Jpeg or Png, the caller can decide
        return new ImageExportFormat(ImageFileFormat.Unknown, ImagePixelFormat.RGB24);
    }
}