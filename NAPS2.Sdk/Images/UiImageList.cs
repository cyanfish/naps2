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

    // TODO: Can we inject this?
    public ThumbnailRenderer? ThumbnailRenderer { get; set; }

    // TODO: Make this immutable?
    public List<UiImage> Images { get; }

    public StateToken CurrentState => new(Images.Select(x => x.GetImageWeakReference()).ToImmutableList());

    public StateToken SavedState
    {
        get => _savedState;
        set => _savedState = value ?? throw new ArgumentNullException(nameof(value));
    }

    // TODO: We should make this selection maintain insertion order, or otherwise guarantee that for things like FDesktop.SavePDF we actually get the images in the right order
    public ListSelection<UiImage> Selection
    {
        get => _selection;
        private set => _selection = value ?? throw new ArgumentNullException(nameof(value));
    }

    public event EventHandler? ImagesUpdated;

    public event EventHandler? ImagesThumbnailChanged;

    public event EventHandler? ImagesThumbnailInvalidated;

    public void UpdateSelection(ListSelection<UiImage> newSelection)
    {
        Selection = newSelection;
        ImagesUpdated?.Invoke(this, EventArgs.Empty);
    }

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
        lock (this)
        {
            var currentSelection = _selection;
            var before = new HashSet<UiImage>(Images);
            if (!ReferenceEquals(selectionToMutate, null))
            {
                mutation.Apply(Images, ref selectionToMutate);
            }
            else
            {
                mutation.Apply(Images, ref currentSelection);
            }
            var after = new HashSet<UiImage>(Images);

            foreach (var added in after.Except(before))
            {
                added.ThumbnailChanged += ImageThumbnailChanged;
                added.ThumbnailInvalidated += ImageThumbnailInvalidated;
            }
            foreach (var removed in before.Except(after))
            {
                removed.ThumbnailChanged -= ImageThumbnailChanged;
                removed.ThumbnailInvalidated -= ImageThumbnailInvalidated;
                removed.Dispose();
            }

            if (currentSelection != _selection)
            {
                UpdateSelectionOnUiThread(currentSelection);
            }
        }
        ImagesUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateSelectionOnUiThread(ListSelection<UiImage> currentSelection)
    {
        // TODO: This won't work right as SyncContext.Current is only set on the UI thread anyway
        var syncContext = SynchronizationContext.Current;
        if (syncContext != null)
        {
            syncContext.Post(_ => _selection = currentSelection, null);
        }
        else
        {
            _selection = currentSelection;
        }
    }

    private void ImageThumbnailChanged(object? sender, EventArgs args)
    {
        ImagesThumbnailChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ImageThumbnailInvalidated(object? sender, EventArgs args)
    {
        ImagesThumbnailInvalidated?.Invoke(this, EventArgs.Empty);
    }

    private void RunUpdateEvents()
    {
        // TODO: Maybe move this out of this class to an event handler?
        lock (this)
        {
            _recoveryStorageManager.WriteIndex(Images);
        }
    }

    /// <summary>
    /// A token that stores the current state of the image list for use in comparisons to determine if changes have been
    /// made since the last save. No disposal is needed.
    /// </summary>
    /// <param name="ImageReferences"></param>
    public record StateToken(ImmutableList<ProcessedImage.WeakReference> ImageReferences);
}