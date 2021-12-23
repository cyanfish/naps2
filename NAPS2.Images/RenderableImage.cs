namespace NAPS2.Images;

// TODO: We're really overloading the "Image" terminology.
// It might make sense to call this a "Page" or similar.
public record RenderableImage(IStorage Storage, ImageMetadata Metadata, TransformState TransformState) : IDisposable
{
    private readonly RefCount.Token _token = new RefCount(Storage).NewToken();

    internal RenderableImage(IStorage storage, ImageMetadata metadata, TransformState transformState, RefCount refCount)
        : this(storage, metadata, transformState)
    {
        _token = refCount.NewToken();
    }

    public PostProcessingData PostProcessingData { get; } = new();

    public RenderableImage WithTransform(Transform transform)
    {
        // TODO: Should metadata update for some transforms?
        var newTransformState = new TransformState(TransformState.Transforms.Add(transform));
        return new RenderableImage(Storage, Metadata, newTransformState, _token.RefCount);
    }

    // TODO: Naming/conventions?
    public RenderableImage Copy() => new(Storage, Metadata, TransformState, _token.RefCount);

    public void Dispose()
    {
        _token.Dispose();
    }
}
