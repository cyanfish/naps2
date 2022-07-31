namespace NAPS2.Images;

public static class ImageExtensions
{
    public static MemoryStream SaveToMemoryStream(this IMemoryImage image, ImageFileFormat imageFormat,
        int quality = -1)
    {
        var stream = new MemoryStream();
        image.Save(stream, imageFormat, quality);
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    public static string AsTypeHint(this ImageFileFormat imageFormat)
    {
        return imageFormat switch
        {
            ImageFileFormat.Bmp => ".bmp",
            ImageFileFormat.Jpeg => ".jpg",
            ImageFileFormat.Png => ".png",
            _ => ""
        };
    }
}