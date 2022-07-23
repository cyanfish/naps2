using System.Collections.Immutable;
using System.Threading;

namespace NAPS2.Images;

public class UiImageList
{
    private StateToken _savedState = new(ImmutableList<ProcessedImage.WeakReference>.Empty);
    private ListSelection<UiImage> _selection;

    public UiImageList() : this(new List<UiImage>())
    {
    }

    public UiImageList(List<UiImage> images)
    {
        Images = images.ToImmutableList();
        _selection = ListSelection.Empty<UiImage>();
    }

    public ImmutableList<UiImage> Images { get; private set; }

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

    public event EventHandler? SelectionChanged;

    public event EventHandler<ImageListEventArgs>? ImagesUpdated;

    public event EventHandler<ImageListEventArgs>? ImagesThumbnailChanged;

    public event EventHandler<ImageListEventArgs>? ImagesThumbnailInvalidated;

    public void UpdateSelection(ListSelection<UiImage> newSelection)
    {
        Selection = newSelection;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Mutate(ListMutation<UiImage> mutation, ListSelection<UiImage>? selectionToMutate = null,
        bool isPassiveInteraction = false)
    {
        MutateInternal(mutation, selectionToMutate, isPassiveInteraction);
    }

    public async Task MutateAsync(ListMutation<UiImage> mutation, ListSelection<UiImage>? selectionToMutate = null,
        bool isPassiveInteraction = false)
    {
        await Task.Run(() => MutateInternal(mutation, selectionToMutate, isPassiveInteraction));
    }

    private void MutateInternal(ListMutation<UiImage> mutation, ListSelection<UiImage>? selectionToMutate,
        bool isPassiveInteraction)
    {
        lock (this)
        {
            var currentSelection = _selection;
            var before = new HashSet<UiImage>(Images);
            var mutableImages = Images.ToList();
            if (!ReferenceEquals(selectionToMutate, null))
            {
                mutation.Apply(mutableImages, ref selectionToMutate);
            }
            else
            {
                mutation.Apply(mutableImages, ref currentSelection);
            }
            Images = mutableImages.ToImmutableList();
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
        ImagesUpdated?.Invoke(this, new ImageListEventArgs(isPassiveInteraction));
    }

    private void UpdateSelectionOnUiThread(ListSelection<UiImage> currentSelection)
    {
        // TODO: This won't work right as SyncContext.Current is only set on the UI thread anyway
        var syncContext = SynchronizationContext.Current;
        if (syncContext != null)
        {
            syncContext.Post(_ => UpdateSelection(currentSelection), null);
        }
        else
        {
            UpdateSelection(currentSelection);
        }
    }

    private void ImageThumbnailChanged(object? sender, EventArgs args)
    {
        // A thumbnail change indicates rendering which is a passive interaction.
        ImagesThumbnailChanged?.Invoke(this, new ImageListEventArgs(false));
    }

    private void ImageThumbnailInvalidated(object? sender, EventArgs args)
    {
        // A thumbnail invalidation indicates an image edit which is an active interaction.
        ImagesThumbnailInvalidated?.Invoke(this, new ImageListEventArgs(true));
    }

    /// <summary>
    /// A token that stores the current state of the image list for use in comparisons to determine if changes have been
    /// made since the last save. No disposal is needed.
    /// </summary>
    /// <param name="ImageReferences"></param>
    public record StateToken(ImmutableList<ProcessedImage.WeakReference> ImageReferences);
}