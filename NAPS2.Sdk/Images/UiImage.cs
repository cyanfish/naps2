namespace NAPS2.Images;

// TODO: We need to implement recovery here.
// TODO: Write tests for this class
/// <summary>
/// A mutable container for an image in the NAPS2 UI that can be edited, has a thumbnail, etc.
/// </summary>
public class UiImage
{
    private ProcessedImage _processedImage;
    private IMemoryImage? _thumbnail;
    private TransformState? _thumbnailTransformState;
    private bool _disposed;
    
    public UiImage(ProcessedImage image)
    {
        _processedImage = image;
    }

    /// <summary>
    /// Gets a clone of the current underlying ProcessedImage that must be later disposed.
    /// </summary>
    /// <returns></returns>
    public ProcessedImage GetClonedImage()
    {
        lock (this)
        {
            return _processedImage.Clone();
        }
    }

    /// <summary>
    /// Gets a weak reference of the current underlying ProcessedImage that doesn't need to be disposed.
    /// </summary>
    /// <returns></returns>
    public ProcessedImage.WeakReference GetImageWeakReference()
    {
        lock (this)
        {
            return _processedImage.GetWeakReference();
        }
    }
    
    public void Dispose()
    {
        lock (this)
        {
            _disposed = true;
            _processedImage.Dispose();
        
            if (_thumbnail != null)
            {
                _thumbnail.Dispose();
                _thumbnail = null;
            }
        
            // TODO: This shouldn't be here, OCR cancellation needs to be figured out
            FullyDisposed?.Invoke(this, EventArgs.Empty);
        }
    }

    public void AddTransform(Transform transform)
    {
        if (transform.IsNull)
        {
            return;
        }
        lock (this)
        {
            var newImage = _processedImage.WithTransform(transform);
            _processedImage.Dispose();
            _processedImage = newImage;
        }
        ThumbnailInvalidated?.Invoke(this, EventArgs.Empty);
    }

    public void ResetTransforms()
    {
        lock (this)
        {
            if (_processedImage.TransformState.IsEmpty)
            {
                return;
            }
            var newImage = _processedImage.WithNoTransforms();
            _processedImage.Dispose();
            _processedImage = newImage;
        }
        ThumbnailInvalidated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Returns a clone of the thumbnail (if present) that must be disposed by the caller.
    /// </summary>
    /// <returns></returns>
    public IMemoryImage? GetThumbnailClone()
    {
        lock (this)
        {
            return _thumbnail?.Clone();
        }
    }

    public void SetThumbnail(IMemoryImage image, TransformState transformState = null)
    {
        lock (this)
        {
            _thumbnail?.Dispose();
            _thumbnail = image;
            _thumbnailTransformState = transformState;
        }
        ThumbnailChanged?.Invoke(this, new EventArgs());
    }

    public bool IsThumbnailDirty => _thumbnailTransformState != _processedImage.TransformState;

    public EventHandler? ThumbnailChanged;

    public EventHandler? ThumbnailInvalidated;

    // TODO: Maybe delete depending on how we handle ocr cancellation
    public EventHandler? FullyDisposed;
}
