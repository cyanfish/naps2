namespace NAPS2.Scan.Internal;

/// <summary>
/// A scanned page being post-processed: the ProcessedImage being built, along with an in-memory render of it. The
/// render is updated incrementally by applying only the transforms added since it was last rendered.
/// </summary>
internal sealed class PostProcessingPage : IDisposable
{
    private IMemoryImage? _rendered;
    private TransformState _renderedState = TransformState.Empty;

    /// <summary>
    /// Takes ownership of the image, which must have the same content as the ProcessedImage's storage.
    /// </summary>
    public PostProcessingPage(ProcessedImage processedImage, IMemoryImage image)
    {
        ProcessedImage = processedImage;
        _rendered = image;
    }

    public ProcessedImage ProcessedImage { get; private set; }

    public void AddTransform(Transform transform)
    {
        ProcessedImage = ProcessedImage.WithTransform(transform, true);
    }

    public void SetPostProcessingData(PostProcessingData data)
    {
        ProcessedImage = ProcessedImage.WithPostProcessingData(data, true);
    }

    /// <summary>
    /// Gets the image rendered with all transforms added so far. The returned image is owned by this object and is only
    /// valid until the next render.
    /// </summary>
    public IMemoryImage GetRendered() => GetRendered(ProcessedImage.TransformState);

    /// <summary>
    /// Gets the image rendered with the specified transforms, which must start with the transforms already rendered.
    /// </summary>
    public IMemoryImage GetRendered(TransformState state)
    {
        if (_rendered == null)
        {
            throw new InvalidOperationException("The rendered image has already been taken");
        }
        var rendered = _renderedState.Transforms;
        if (state.Transforms.Count < rendered.Count || !rendered.SequenceEqual(state.Transforms.Take(rendered.Count)))
        {
            throw new InvalidOperationException("Transforms that were already rendered can't be changed or merged");
        }
        _rendered = _rendered.PerformAllTransforms(state.Transforms.Skip(rendered.Count));
        _renderedState = state;
        return _rendered;
    }

    /// <summary>
    /// Gets the image rendered with all transforms added so far, transferring ownership to the caller. No further renders
    /// are possible after this.
    /// </summary>
    public IMemoryImage TakeRendered()
    {
        var rendered = GetRendered();
        _rendered = null;
        return rendered;
    }

    public void Dispose()
    {
        _rendered?.Dispose();
    }
}
