namespace NAPS2.Images;

// TODO: Implement & use this class in the NAPS2 UI
public class UiImage
{
    private ProcessedImage ProcessedImage { get; set; }



    public void Dispose()
    {
        // lock (this)
        // {
        //     _disposed = true;
        //     // TODO: Delete the recovery entry (if recovery is being used)
        //     RenderableImage.Dispose();
        //
        //     if (_thumbnail != null)
        //     {
        //         _thumbnail.Dispose();
        //         _thumbnail = null;
        //     }
        //
        //     FullyDisposed?.Invoke(this, new EventArgs());
        // }
    }

    public void AddTransform(Transform transform)
    {
        // if (transform.IsNull)
        // {
        //     return;
        // }
        // lock (this)
        // {
        //     // Also updates the recovery index since they reference the same list
        //     Transform.AddOrSimplify(Metadata.TransformList, transform);
        //     Metadata.TransformState++;
        // }
        // Metadata.Commit();
        // ThumbnailInvalidated?.Invoke(this, new EventArgs());
    }

    public void ResetTransforms()
    {
        // lock (this)
        // {
        //     if (Metadata.TransformList.Count == 0)
        //     {
        //         return;
        //     }
        //     Metadata.TransformList.Clear();
        //     Metadata.TransformState++;
        // }
        // Metadata.Commit();
        // ThumbnailInvalidated?.Invoke(this, new EventArgs());
    }

    public IMemoryImage? GetThumbnail()
    {
        // lock (this)
        // {
        //     return _thumbnail?.Clone();
        // }
        return null;
    }

    public void SetThumbnail(IMemoryImage image, int? state = null)
    {
        // lock (this)
        // {
        //     _thumbnail?.Dispose();
        //     _thumbnail = image;
        //     _thumbnailState = state ?? Metadata.TransformState;
        // }
        // ThumbnailChanged?.Invoke(this, new EventArgs());
    }

    public bool IsThumbnailDirty => false; // _thumbnailState != Metadata.TransformState;

    public EventHandler? ThumbnailChanged;

    public EventHandler? ThumbnailInvalidated;

    public EventHandler? FullyDisposed;
}
