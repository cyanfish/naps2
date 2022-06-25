namespace NAPS2.Images;

// TODO: This snapshotting also affects a lot more things. Namely selections and UI interactions.
// TODO: So if that's out of date then we have a problem.
public class ImageRenderState : IEquatable<ImageRenderState>
{
    public ImageRenderState(ProcessedImage.WeakReference image, TransformState? thumbnailState, IMemoryImage? thumbnail,
        UiImage source)
    {
        Image = image;
        ThumbnailState = thumbnailState;
        Thumbnail = thumbnail;
        Source = source;
    }

    public ProcessedImage.WeakReference Image { get; init; }

    public TransformState? ThumbnailState { get; init; }

    public IMemoryImage? Thumbnail { get; init; }

    public UiImage Source { get; init; }

    // TODO: Compare source as well?
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ImageRenderState) obj);
    }

    public bool Equals(ImageRenderState? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        // TODO: Should we also compare metadata? 
        return Equals(Image, other.Image) && Equals(ThumbnailState, other.ThumbnailState);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Image.GetHashCode() * 397) ^ (ThumbnailState?.GetHashCode() ?? 0);
        }
    }
}