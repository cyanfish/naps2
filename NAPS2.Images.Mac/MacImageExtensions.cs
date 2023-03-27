
namespace NAPS2.Images.Mac;

public static class MacImageExtensions
{
    public static NSImage RenderToNsImage(this IRenderableImage image)
    {
        var macImageContext = image.ImageContext as MacImageContext ??
                              throw new ArgumentException("The provided image does not have a MacImageContext");
        return macImageContext.RenderToNsImage(image);
    }

    public static NSImage AsNsImage(this IMemoryImage image)
    {
        var macImage = image as MacImage ?? throw new ArgumentException("Expected a MacImage", nameof(image));
        return macImage.NsImage;
    }
}