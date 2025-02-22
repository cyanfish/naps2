using NAPS2.Pdf;

namespace NAPS2.Images;

/// <summary>
/// An image that has gone through the scanning (or importing) process. It has metadata about the image, possibly
/// additional post-processing data from the scan, and may have transformations that have been applied during or after
/// the scan.
///
/// This type is immutable and uses a reference counting model for the underlying image storage. You can create a new
/// reference with Clone() that will need to be disposed, and the underlying image storage will only be disposed once
/// all related instances are disposed (or the parent ScanningContext is disposed).
/// </summary>
public class ProcessedImage : IRenderableImage, IPdfRendererProvider, IDisposable, IEquatable<ProcessedImage>
{
    private readonly RefCount.Token _token;
    private bool _disposed;

    internal ProcessedImage(ImageContext imageContext, IImageStorage storage, ImageMetadata metadata,
        PostProcessingData postProcessingData, TransformState transformState, RefCount storageRefCount)
    {
        ImageContext = imageContext;
        Storage = storage;
        Metadata = metadata;
        PostProcessingData = postProcessingData;
        TransformState = transformState;
        StorageRefCount = storageRefCount;
        _token = StorageRefCount.NewToken();
    }

    public ImageContext ImageContext { get; }

    // TODO: Consider having two copies of the image on disk - one before transforms, one after.
    public IImageStorage Storage { get; }

    internal RefCount StorageRefCount { get; }

    public ImageMetadata Metadata { get; }

    public PostProcessingData PostProcessingData { get; }

    public TransformState TransformState { get; }

    /// <summary>
    /// Creates a new ProcessedImage instance with the same underlying image storage/metadata and a new transform
    /// appended to the TransformState. All instances will need to be disposed before the underlying image storage is
    /// disposed.
    /// </summary>
    public ProcessedImage WithTransform(Transform transform, bool disposeSelf = false) =>
        WithTransformState(TransformState.AddOrSimplify(transform), disposeSelf);

    /// <summary>
    /// Creates a new ProcessedImage instance with the same underlying image storage/metadata and no transforms. All
    /// instances will need to be disposed before the underlying image storage is disposed.
    /// </summary>
    public ProcessedImage WithNoTransforms(bool disposeSelf = false) =>
        WithTransformState(TransformState.Empty, disposeSelf);

    /// <summary>
    /// Creates a new ProcessedImage instance with the same underlying image storage/metadata and a new transform
    /// state. All instances will need to be disposed before the underlying image storage is disposed.
    /// </summary>
    public ProcessedImage WithTransformState(TransformState newTransformState, bool disposeSelf = false)
    {
        var result =
            new ProcessedImage(ImageContext, Storage, Metadata, PostProcessingData, newTransformState, _token.RefCount);
        if (disposeSelf)
        {
            Dispose();
        }

        return result;
    }

    public ProcessedImage WithPostProcessingData(PostProcessingData postProcessingData, bool disposeSelf)
    {
        var result =
            new ProcessedImage(ImageContext, Storage, Metadata, postProcessingData, TransformState, _token.RefCount);
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

            return new(ImageContext, Storage, Metadata, PostProcessingData, TransformState, _token.RefCount);
        }
    }

    /// <summary>
    /// Creates a WeakReference wrapper for the current instance that doesn't have any effect on the instance's
    /// lifetime.
    /// </summary>
    /// <returns></returns>
    public WeakReference GetWeakReference() => new(this);

    public override bool Equals(object? obj)
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

    internal class InternalDisposer : IDisposable
    {
        private readonly IImageStorage _storage;
        private readonly PostProcessingData _postProcessingData;
        private bool _disposed;

        public InternalDisposer(IImageStorage storage, PostProcessingData postProcessingData, IProcessedImageOwner? owner)
        {
            _storage = storage;
            _postProcessingData = postProcessingData;
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

            _storage.Dispose();
            _postProcessingData.Thumbnail?.Dispose();
            _postProcessingData.OcrCts?.Cancel();
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

    IPdfRenderer IPdfRendererProvider.PdfRenderer => new PdfiumPdfRenderer();
}