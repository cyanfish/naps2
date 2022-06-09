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
public class ProcessedImage : IDisposable
{
    private readonly RefCount.Token _token;

    internal ProcessedImage(IImageStorage storage, ImageMetadata metadata, TransformState transformState,
        RefCount refCount)
    {
        Storage = storage;
        Metadata = metadata;
        TransformState = transformState;
        _token = refCount.NewToken();
    }

    public ProcessedImage(IImageStorage storage, ImageMetadata metadata, TransformState transformState)
        : this(storage, metadata, transformState, new RefCount(storage))
    {
    }

    // TODO: Make this an immutable record and include it in the constructor
    public PostProcessingData PostProcessingData { get; } = new();

    public IImageStorage Storage { get; }

    public ImageMetadata Metadata { get; }

    public TransformState TransformState { get; }

    /// <summary>
    /// Creates a new ProcessedImage instance with the same underlying image storage/metadata and a new transform
    /// appended to the TransformState. All instances will need to be disposed before the underlying image storage is
    /// disposed.
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public ProcessedImage WithTransform(Transform transform)
    {
        // TODO: Should metadata update for some transforms?
        var newTransformState = TransformState.AddOrSimplify(transform);
        return new ProcessedImage(Storage, Metadata, newTransformState, _token.RefCount);
    }

    /// <summary>
    /// Creates a new ProcessedImage instance with the same underlying image storage/metadata and no transforms. All
    /// instances will need to be disposed before the underlying image storage is disposed.
    /// </summary>
    /// <returns></returns>
    public ProcessedImage WithNoTransforms()
    {
        return new ProcessedImage(Storage, Metadata, TransformState.Empty, _token.RefCount);
    }

    /// <summary>
    /// Creates a new ProcessedImage instance with the same underlying image storage/metadata. All instances will need
    /// to be disposed before the underlying image storage is disposed.
    /// </summary>
    /// <returns></returns>
    public ProcessedImage Clone() => new(Storage, Metadata, TransformState, _token.RefCount);

    /// <summary>
    /// Creates a WeakReference wrapper for the current instance that doesn't have any effect on the instance's
    /// lifetime.
    /// </summary>
    /// <returns></returns>
    public WeakReference GetWeakReference() => new WeakReference(this);

    public void Dispose()
    {
        // TODO: Also dispose of postprocessingdata bitmap (?)
        _token.Dispose();
    }

    public record WeakReference(ProcessedImage ProcessedImage);
}