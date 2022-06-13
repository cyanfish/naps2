namespace NAPS2.Images;

// TODO: Write tests for this class
/// <summary>
/// An image that has gone through the scanning (or importing) process. It has metadata about the image, possibly
/// additional post-processing data from the scan, and may have transformations that have been applied during or after
/// the scan.
///
/// This type is immutable and uses a reference counting model for the underlying image storage. You can create a new
/// reference with Clone() that will need to be disposed, and the underlying image storage will only be disposed once
/// all related instances are disposed (or the parent ScanningContext is disposed).
/// </summary>
public class ProcessedImage : IDisposable, IEquatable<ProcessedImage>
{
    private readonly RefCount.Token _token;
    private bool _disposed;

    internal ProcessedImage(IImageStorage storage, ImageMetadata metadata, PostProcessingData postProcessingData,
        TransformState transformState, RefCount refCount)
    {
        Storage = storage;
        Metadata = metadata;
        PostProcessingData = postProcessingData;
        TransformState = transformState;
        _token = refCount.NewToken();
    }

    public ProcessedImage(IImageStorage storage, ImageMetadata metadata, PostProcessingData postProcessingData,
        TransformState transformState, IProcessedImageOwner? owner = null)
    {
        Storage = storage;
        Metadata = metadata;
        PostProcessingData = postProcessingData;
        TransformState = transformState;
        var internalDisposer = new InternalDisposer(this, owner);
        var refCount = new RefCount(internalDisposer);
        _token = refCount.NewToken();
    }

    public IImageStorage Storage { get; }

    public ImageMetadata Metadata { get; }

    public PostProcessingData PostProcessingData { get; }

    public TransformState TransformState { get; }

    /// <summary>
    /// Creates a new ProcessedImage instance with the same underlying image storage/metadata and a new transform
    /// appended to the TransformState. All instances will need to be disposed before the underlying image storage is
    /// disposed.
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public ProcessedImage WithTransform(Transform transform, bool disposeSelf = false)
    {
        // TODO: Should metadata update for some transforms?
        var newTransformState = TransformState.AddOrSimplify(transform);
        var result = new ProcessedImage(Storage, Metadata, PostProcessingData, newTransformState, _token.RefCount);
        if (disposeSelf)
        {
            Dispose();
        }

        return result;
    }

    /// <summary>
    /// Creates a new ProcessedImage instance with the same underlying image storage/metadata and no transforms. All
    /// instances will need to be disposed before the underlying image storage is disposed.
    /// </summary>
    /// <returns></returns>
    public ProcessedImage WithNoTransforms()
    {
        return new ProcessedImage(Storage, Metadata, PostProcessingData, TransformState.Empty, _token.RefCount);
    }

    public ProcessedImage WithPostProcessingData(PostProcessingData postProcessingData, bool disposeSelf)
    {
        var result = new ProcessedImage(Storage, Metadata, postProcessingData, TransformState, _token.RefCount);
        if (disposeSelf)
        {
            Dispose();
        }

        return result;
    }

    /// <summary>
    /// Creates a new ProcessedImage instance with the same underlying image storage/metadata. All instances will need
    /// to be disposed before the underlying image storage is disposed.
    /// </summary>
    /// <returns></returns>
    public ProcessedImage Clone()
    {
        lock (this)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ProcessedImage));
            }

            return new(Storage, Metadata, PostProcessingData, TransformState, _token.RefCount);
        }
    }

    /// <summary>
    /// Creates a WeakReference wrapper for the current instance that doesn't have any effect on the instance's
    /// lifetime.
    /// </summary>
    /// <returns></returns>
    public WeakReference GetWeakReference() => new(this);

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ProcessedImage) obj);
    }

    public bool Equals(ProcessedImage? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        // TODO: Should we also compare metadata? 
        return Storage.Equals(other.Storage) && TransformState.Equals(other.TransformState);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Storage.GetHashCode() * 397) ^ TransformState.GetHashCode();
        }
    }

    public void Dispose()
    {
        lock (this)
        {
            _token.Dispose();
            _disposed = true;
        }
    }

    private class InternalDisposer : IDisposable
    {
        private readonly ProcessedImage _processedImage;
        private bool _disposed;

        public InternalDisposer(ProcessedImage processedImage, IProcessedImageOwner? owner)
        {
            _processedImage = processedImage;
            owner?.Register(this);
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
            }

            _processedImage.Storage.Dispose();
            _processedImage.PostProcessingData.Thumbnail?.Dispose();
        }
    }

    /// <summary>
    /// A class functionally equivalent to a ProcessedImage reference, but that makes explicit the intention not to
    /// have ownership over or prevent disposal of the image.
    /// </summary>
    /// <param name="ProcessedImage">
    /// The reference. Users should prefer not to directly access this unless it is understood that it may be disposed
    /// at any moment.
    /// </param>
    public record WeakReference(ProcessedImage ProcessedImage);
}