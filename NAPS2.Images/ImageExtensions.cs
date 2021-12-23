namespace NAPS2.Images;

public static class ImageExtensions
{
    public static MemoryStream SaveToMemoryStream(this IImage image, ImageFileFormat imageFormat)
    {
        var stream = new MemoryStream();
        image.Save(stream, imageFormat);
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }
}
