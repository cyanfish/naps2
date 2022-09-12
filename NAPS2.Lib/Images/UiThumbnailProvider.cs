using NAPS2.EtoForms;

namespace NAPS2.Images;

/// <summary>
/// Gets the actual thumbnail image to be shown in the UI. This might be a placeholder hourglass indicator if the
/// real thumbnail isn't available yet, or an overlaid hourglass if it's out of date.
/// </summary>
public class UiThumbnailProvider
{
    private readonly ImageContext _imageContext;
    private IMemoryImage? _placeholder;

    public UiThumbnailProvider(ImageContext imageContext)
    {
        _imageContext = imageContext;
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
        lock (this)
        {
            if (_placeholder?.Width == thumbnailSize)
            {
                return _placeholder;
            }
            _placeholder?.Dispose();
            _placeholder = _imageContext.Create(thumbnailSize, thumbnailSize, ImagePixelFormat.RGB24);
            _placeholder = EtoPlatform.Current.DrawHourglass(_imageContext, _placeholder);
            return _placeholder;
        }
    }
}