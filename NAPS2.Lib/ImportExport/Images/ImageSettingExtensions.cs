namespace NAPS2.ImportExport.Images;

public static class ImageSettingExtensions
{
    // TODO: Use the sdk tiffcompression in settings (need localization on it)
    public static TiffCompressionType ToTiffCompressionType(this TiffCompression tiffCompression) =>
        tiffCompression switch
        {
            TiffCompression.Auto => TiffCompressionType.Auto,
            TiffCompression.Ccitt4 => TiffCompressionType.Ccitt4,
            TiffCompression.Lzw => TiffCompressionType.Lzw,
            TiffCompression.None => TiffCompressionType.None,
        };
}