using System.Collections.Immutable;
using System.Threading;

namespace NAPS2.Images;

public class ScannedImageList
{
    private readonly ImageContext _imageContext;
    private readonly TimedThrottle _runUpdateEventsThrottle;
    private Memento _savedState = Memento.Empty;
    private ListSelection<ProcessedImage> _selection;

    public ScannedImageList(ImageContext imageContext)
        : this(imageContext, new List<ProcessedImage>())
    {
    }

    public ScannedImageList(ImageContext imageContext, List<ProcessedImage> images)
    {
        _imageContext = imageContext;
        _runUpdateEventsThrottle = new TimedThrottle(RunUpdateEvents, TimeSpan.FromMilliseconds(100));
        Images = images;
        _selection = ListSelection.Empty<ProcessedImage>();
    }

    public ThumbnailRenderer? ThumbnailRenderer { get; set; }

    public List<ProcessedImage> Images { get; }

    public Memento CurrentState => new Memento(Images.ToImmutableList());

    public Memento SavedState
    {
        get => _savedState;
        set => _savedState = value ?? throw new ArgumentNullException(nameof(value));
    }

    public ListSelection<ProcessedImage> Selection
    {
        get => _selection;
        set => _selection = value ?? throw new ArgumentNullException(nameof(value));
    }

    public void UpdateSelection(ListSelection<ProcessedImage> newSelection)
    {
        Selection = newSelection;
        ImagesUpdated?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? ImagesUpdated;

    public void Mutate(ListMutation<ProcessedImage> mutation, ListSelection<ProcessedImage>? selectionToMutate = null)
    {
        MutateInternal(mutation, selectionToMutate);
        _runUpdateEventsThrottle.RunAction(SynchronizationContext.Current);
    }

    public async Task MutateAsync(ListMutation<ProcessedImage> mutation, ListSelection<ProcessedImage>? selectionToMutate = null)
    {
        await Task.Run(() => MutateInternal(mutation, selectionToMutate));
        _runUpdateEventsThrottle.RunAction(SynchronizationContext.Current);
    }

    private void MutateInternal(ListMutation<ProcessedImage> mutation, ListSelection<ProcessedImage>? selectionToMutate)
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