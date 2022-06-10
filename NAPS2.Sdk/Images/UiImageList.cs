using System.Collections.Immutable;
using System.Threading;

namespace NAPS2.Images;

public class UiImageList
{
    private readonly RecoveryStorageManager _recoveryStorageManager;
    private readonly TimedThrottle _runUpdateEventsThrottle;
    private StateToken _savedState = new(ImmutableList<ProcessedImage.WeakReference>.Empty);
    private ListSelection<UiImage> _selection;

    public UiImageList(RecoveryStorageManager recoveryStorageManager)
        : this(recoveryStorageManager, new List<UiImage>())
    {
    }

    public UiImageList(RecoveryStorageManager recoveryStorageManager, List<UiImage> images)
    {
        _recoveryStorageManager = recoveryStorageManager;
        _runUpdateEventsThrottle = new TimedThrottle(RunUpdateEvents, TimeSpan.FromMilliseconds(100));
        Images = images;
        _selection = ListSelection.Empty<UiImage>();
    }

    public ThumbnailRenderer? ThumbnailRenderer { get; set; }

    public List<UiImage> Images { get; }

    public StateToken CurrentState => new(Images.Select(x => x.GetImageWeakReference()).ToImmutableList());

    public StateToken SavedState
    {
        get => _savedState;
        set => _savedState = value ?? throw new ArgumentNullException(nameof(value));
    }

    public ListSelection<UiImage> Selection
    {
        get => _selection;
        set => _selection = value ?? throw new ArgumentNullException(nameof(value));
    }

    public void UpdateSelection(ListSelection<UiImage> newSelection)
    {
        Selection = newSelection;
        ImagesUpdated?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? ImagesUpdated;

    public void Mutate(ListMutation<UiImage> mutation, ListSelection<UiImage>? selectionToMutate = null)
    {
        MutateInternal(mutation, selectionToMutate);
        _runUpdateEventsThrottle.RunAction(SynchronizationContext.Current);
    }

    public async Task MutateAsync(ListMutation<UiImage> mutation, ListSelection<UiImage>? selectionToMutate = null)
    {
        await Task.Run(() => MutateInternal(mutation, selectionToMutate));
        _runUpdateEventsThrottle.RunAction(SynchronizationContext.Current);
    }

    private void MutateInternal(ListMutation<UiImage> mutation, ListSelection<UiImage>? selectionToMutate)
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
        _recoveryStorageManager.WriteIndex(Images);
        ImagesUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// A token that stores the current state of the image list for use in comparisons to determine if changes have been
    /// made since the last save. No disposal is needed.
    /// </summary>
    /// <param name="ImageReferences"></param>
    public record StateToken(ImmutableList<ProcessedImage.WeakReference> ImageReferences);
}