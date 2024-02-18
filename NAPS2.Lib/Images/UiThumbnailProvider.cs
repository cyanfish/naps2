using NAPS2.EtoForms;

namespace NAPS2.Images;

/// <summary>
/// Gets the actual thumbnail image to be shown in the UI. This might be a placeholder hourglass indicator if the
/// real thumbnail isn't available yet, or an overlaid hourglass if it's out of date.
/// </summary>
public class UiThumbnailProvider
{
    private readonly ImageContext _imageContext;
    private readonly ColorScheme _colorScheme;

    public UiThumbnailProvider(ImageContext imageContext, ColorScheme colorScheme)
    {
        _imageContext = imageContext;
        _colorScheme = colorScheme;
    }

    public IMemoryImage GetThumbnail(UiImage img, int thumbnailSize)
    {
        lock (img)
        {
            var thumb = img.GetThumbnailClone();
            if (thumb == null)
            {
                return RenderPlaceholder(thumbnailSize);
            }
            if (img.IsThumbnailDirty)
            {
                thumb = EtoPlatform.Current.DrawHourglass(_imageContext, thumb);
            }
            return thumb;
        }
    }

    private IMemoryImage RenderPlaceholder(int thumbnailSize)
    {
        var placeholder = _imageContext.Create(thumbnailSize, thumbnailSize, ImagePixelFormat.RGB24);
        placeholder.Fill(_colorScheme.BackgroundColor);
        placeholder = EtoPlatform.Current.DrawHourglass(_imageContext, placeholder);
        return placeholder;
    }
}