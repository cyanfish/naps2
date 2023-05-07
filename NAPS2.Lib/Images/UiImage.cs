namespace NAPS2.Images;

// TODO: Write tests for this class
/// <summary>
/// A mutable container for an image in the NAPS2 UI that can be edited, has a thumbnail, etc.
/// </summary>
public class UiImage : IDisposable
{
    private ProcessedImage _processedImage;
    private IMemoryImage? _thumbnail;
    private TransformState? _thumbnailTransformState;
    private bool _saved;
    private bool _disposed;

    public UiImage(ProcessedImage image)
    {
        _processedImage = image;
        var ppd = _processedImage.PostProcessingData;
        if (ppd.Thumbnail != null && ppd.ThumbnailTransformState != null)
        {
            _thumbnail = _processedImage.PostProcessingData.Thumbnail;
            _thumbnailTransformState = _processedImage.PostProcessingData.ThumbnailTransformState;
        }
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

    public void MarkSaved(ProcessedImage state)
    {
        lock (this)
        {
            if (Equals(state, _processedImage))
            {
                _saved = true;
            }
        }
    }

    public void Dispose()
    {
        lock (this)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _processedImage.Dispose();

            if (_thumbnail != null)
            {
                _thumbnail.Dispose();
                _thumbnail = null;
            }
        }
    }

    public void AddTransform(Transform transform, IMemoryImage? prerenderedThumbnail = null)
    {
        AddTransforms(new[] { transform }, prerenderedThumbnail);
    }

    public void AddTransforms(IEnumerable<Transform> transforms, IMemoryImage? prerenderedThumbnail = null)
    {
        var transformList = transforms.ToList();
        if (transformList.All(x => x.IsNull))
        {
            prerenderedThumbnail?.Dispose();
            return;
        }
        lock (this)
        {
            foreach (var transform in transformList)
            {
                _processedImage = _processedImage.WithTransform(transform, true);
            }
            if (prerenderedThumbnail != null)
            {
                _thumbnail?.Dispose();
                _thumbnail = prerenderedThumbnail;
                _thumbnailTransformState = _processedImage.TransformState;
            }
            _saved = false;
        }
        ThumbnailInvalidated?.Invoke(this, EventArgs.Empty);
        if (prerenderedThumbnail != null)
        {
            ThumbnailChanged?.Invoke(this, EventArgs.Empty);
        }
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
            _saved = false;
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

    public int GetThumbnailSize()
    {
        lock (this)
        {
            if (_thumbnail == null)
            {
                return -1;
            }
            return Math.Max(_thumbnail.Width, _thumbnail.Height);
        }
    }

    public void SetThumbnail(IMemoryImage image, TransformState transformState)
    {
        lock (this)
        {
            _thumbnail?.Dispose();
            _thumbnail = image;
            _thumbnailTransformState = transformState;
        }
        ThumbnailChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsThumbnailDirty => _thumbnailTransformState != _processedImage.TransformState;

    public bool HasUnsavedChanges => !_saved;

    public EventHandler? ThumbnailChanged;

    public EventHandler? ThumbnailInvalidated;

    public ImageRenderState GetImageRenderState()
    {
        lock (this)
        {
            return new ImageRenderState(_processedImage.GetWeakReference(), _thumbnailTransformState, _thumbnail, this);
        }
    }

    public string DisplayName { get; set; } = "";
}