using System.Collections.Immutable;
using System.Threading;

namespace NAPS2.Images;

public class ScannedImageList
{
    private readonly ImageContext _imageContext;
    private readonly TimedThrottle _runUpdateEventsThrottle;
    private Memento _savedState = Memento.Empty;
    private ListSelection<RenderableImage> _selection;

    public ScannedImageList(ImageContext imageContext)
        : this(imageContext, new List<RenderableImage>())
    {
    }

    public ScannedImageList(ImageContext imageContext, List<RenderableImage> images)
    {
        _imageContext = imageContext;
        _runUpdateEventsThrottle = new TimedThrottle(RunUpdateEvents, TimeSpan.FromMilliseconds(100));
        Images = images;
        _selection = ListSelection.Empty<RenderableImage>();
    }

    public ThumbnailRenderer? ThumbnailRenderer { get; set; }

    public List<RenderableImage> Images { get; }

    public Memento CurrentState => new Memento(Images.ToImmutableList());

    public Memento SavedState
    {
        get => _savedState;
        set => _savedState = value ?? throw new ArgumentNullException(nameof(value));
    }

    public ListSelection<RenderableImage> Selection
    {
        get => _selection;
        set => _selection = value ?? throw new ArgumentNullException(nameof(value));
    }

    public void UpdateSelection(ListSelection<RenderableImage> newSelection)
    {
        Selection = newSelection;
        ImagesUpdated?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? ImagesUpdated;

    public void Mutate(ListMutation<RenderableImage> mutation, ListSelection<RenderableImage>? selectionToMutate = null)
    {
        MutateInternal(mutation, selectionToMutate);
        _runUpdateEventsThrottle.RunAction(SynchronizationContext.Current);
    }

    public async Task MutateAsync(ListMutation<RenderableImage> mutation, ListSelection<RenderableImage>? selectionToMutate = null)
    {
        await Task.Run(() => MutateInternal(mutation, selectionToMutate));
        _runUpdateEventsThrottle.RunAction(SynchronizationContext.Current);
    }

    private void MutateInternal(ListMutation<RenderableImage> mutation, ListSelection<RenderableImage>? selectionToMutate)
    {
        if (!ReferenceEquals(selectionToMutate, null))
        {
            mutation.Apply(Images, ref selectionToMutate);
        }
        else
        {
            mutation.Apply(Images, ref _selection);
        }
    }

    private void RunUpdateEvents()
    {
        UpdateImageMetadata();
        ImagesUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateImageMetadata()
    {
        int i = 0;
        foreach (var image in Images)
        {
            // TODO: Update index
            // image.Metadata.Index = i++;
        }
        _imageContext.ImageMetadataFactory.CommitAllMetadata();
    }

    // TODO: Undo/redo etc. thoughts:
    // A memento is a copy of a ScannedImage list with Snapshots
    // Need to add a Restore operation for Snapshots
    // This lends itself well to undoing deletions (since snapshots already persist on disk).
    // Perhaps everything that changes any image (including ImageForm stuff, insertions) should be encompassed by a mutation.
}