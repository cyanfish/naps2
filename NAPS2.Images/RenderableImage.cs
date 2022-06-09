namespace NAPS2.Images;

// TODO: Consider renaming
public class RenderableImage : IDisposable
{
    private readonly RefCount.Token _token;

    internal RenderableImage(IStorage storage, ImageMetadata metadata, TransformState transformState, RefCount refCount)
    {
        Storage = storage;
        Metadata = metadata;
        TransformState = transformState;
        _token = refCount.NewToken();
    }

    public RenderableImage(IStorage storage, ImageMetadata metadata, TransformState transformState)
        : this(storage, metadata, transformState, new RefCount(storage))
    {
    }

    // TODO: Make this an immutable record and include it in the constructor
    public PostProcessingData PostProcessingData { get; } = new();

    public IStorage Storage { get; }

    public ImageMetadata Metadata { get; }

    public TransformState TransformState { get; }

    public RenderableImage WithTransform(Transform transform)
    {
        // TODO: Should metadata update for some transforms?
        var newTransformState = new TransformState(TransformState.Transforms.Add(transform));
        return new RenderableImage(Storage, Metadata, newTransformState, _token.RefCount);
    }

    public RenderableImage Clone() => new(Storage, Metadata, TransformState, _token.RefCount);

    public void Dispose()
    {
        // TODO: Also dispose of postprocessingdata bitmap (?)
        _token.Dispose();
    }
}
