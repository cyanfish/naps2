using System.Collections.Immutable;
using System.Threading;

namespace NAPS2.Images;

public class UiImageList
{
    private const int MAX_UNDO_LENGTH = 20;

    public static UiImageList FromImages(List<UiImage> images) => new(images);

    private readonly UndoStack _undoStack = new(MAX_UNDO_LENGTH);

    private ListSelection<UiImage> _selection;
    private StateToken _savedState = new(ImmutableList<ProcessedImage.WeakReference>.Empty);

    public UiImageList() : this([])
    {
    }

    private UiImageList(List<UiImage> images)
    {
        Images = images.ToImmutableList();
        _selection = ListSelection.Empty<UiImage>();
    }

    public ImmutableList<UiImage> Images { get; private set; }

    public StateToken CurrentState => new(Images.Select(x => x.GetImageWeakReference()).ToImmutableList());

    public ListSelection<UiImage> Selection
    {
        get => _selection;
        private set => _selection = value ?? throw new ArgumentNullException(nameof(value));
    }

    public bool HasUnsavedChanges => _savedState != CurrentState || Images.Any(x => x.HasUnsavedChanges);

    public bool CanUndo => _undoStack.CanUndo;

    public bool CanRedo => _undoStack.CanRedo;

    public event EventHandler? SelectionChanged;

    public event EventHandler<ImageListEventArgs>? ImagesUpdated;

    public event EventHandler<ImageListEventArgs>? ImagesThumbnailChanged;

    public event EventHandler<ImageListEventArgs>? ImagesThumbnailInvalidated;

    public void AddToSelection(UiImage image)
    {
        UpdateSelection(ListSelection.From(Images.Where(x => x == image || Selection.Contains(x))));
    }

    public void UpdateSelection(ListSelection<UiImage> newSelection)
    {
        Selection = newSelection;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void MarkSaved(StateToken priorState, IEnumerable<ProcessedImage> savedImages)
    {
        var savedImagesSet = savedImages.ToHashSet();
        lock (this)
        {
            _savedState = priorState;
            foreach (var image in Images)
            {
                var imageState = image.GetImageWeakReference().ProcessedImage;
                if (savedImagesSet.Contains(imageState))
                {
                    image.MarkSaved(imageState);
                }
            }
        }
    }

    public void MarkAllSaved()
    {
        lock (this)
        {
            _savedState = CurrentState;
            foreach (var image in Images)
            {
                var imageState = image.GetImageWeakReference().ProcessedImage;
                image.MarkSaved(imageState);
            }
        }
    }

    public void Mutate(ListMutation<UiImage> mutation, ListSelection<UiImage>? selectionToMutate = null,
        bool isPassiveInteraction = false, bool updateUndoStack = true, bool disposeDeleted = true)
    {
        MutateInternal(mutation, selectionToMutate, isPassiveInteraction, updateUndoStack, disposeDeleted);
    }

    public async Task MutateAsync(ListMutation<UiImage> mutation, ListSelection<UiImage>? selectionToMutate = null,
        bool isPassiveInteraction = false, bool updateUndoStack = true, bool disposeDeleted = true)
    {
        await Task.Run(() =>
            MutateInternal(mutation, selectionToMutate, isPassiveInteraction, updateUndoStack, disposeDeleted));
    }

    private void MutateInternal(ListMutation<UiImage> mutation, ListSelection<UiImage>? selectionToMutate,
        bool isPassiveInteraction, bool updateUndoStack, bool disposeDeleted)
    {
        lock (this)
        {
            var currentSelection = _selection;
            var before = Images.ToList();
            var beforeTransforms = before.Select(img => img.TransformState).ToList();
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
            var after = Images.ToList();
            var afterTransforms = after.Select(img => img.TransformState).ToList();

            var allAdded = after.Except(before).ToList();
            foreach (var added in allAdded)
            {
                added.ThumbnailChanged += ImageThumbnailChanged;
                added.ThumbnailInvalidated += ImageThumbnailInvalidated;
            }
            var allRemoved = before.Except(after).ToList();
            foreach (var removed in allRemoved)
            {
                removed.ThumbnailChanged -= ImageThumbnailChanged;
                removed.ThumbnailInvalidated -= ImageThumbnailInvalidated;
                if (disposeDeleted)
                {
                    removed.Dispose();
                }
            }
            currentSelection = ListSelection.From(currentSelection.Except(allRemoved));

            if (updateUndoStack)
            {
                MaybeAddToUndoStack(before, beforeTransforms, after, afterTransforms);
            }

            if (currentSelection != _selection)
            {
                UpdateSelection(currentSelection);
            }
        }
        ImagesUpdated?.Invoke(this, new ImageListEventArgs(isPassiveInteraction));
    }

    private void MaybeAddToUndoStack(
        List<UiImage> before, List<TransformState> beforeTransforms,
        List<UiImage> after, List<TransformState> afterTransforms)
    {
        if (ReplaceRangeUndoElement.FromFullList(this, before, after)
            is { } replaceRangeUndoElement)
        {
            _undoStack.Push(replaceRangeUndoElement);
        }
        if (TransformImagesUndoElement.FromFullList(before, beforeTransforms, after, afterTransforms)
            is { } replaceTransformsUndoElement)
        {
            _undoStack.Push(replaceTransformsUndoElement);
        }
    }

    private void ImageThumbnailChanged(object? sender, EventArgs args)
    {
        // A thumbnail change indicates rendering which is a passive interaction.
        ImagesThumbnailChanged?.Invoke(this, new ImageListEventArgs(true));
    }

    private void ImageThumbnailInvalidated(object? sender, EventArgs args)
    {
        // A thumbnail invalidation indicates an image edit which is an active interaction.
        ImagesThumbnailInvalidated?.Invoke(this, new ImageListEventArgs(false));
    }

    public async Task Undo()
    {
        await Task.Run(() =>
        {
            lock (this)
            {
                _undoStack.Undo();
            }
        });
    }

    public async Task Redo()
    {
        await Task.Run(() =>
        {
            lock (this)
            {
                _undoStack.Redo();
            }
        });
    }

    public void PushUndoElement(IUndoElement undoElement)
    {
        lock (this)
        {
            _undoStack.Push(undoElement);
        }
    }

    /// <summary>
    /// A token that stores the current state of the image list for use in comparisons to determine if changes have been
    /// made since the last save. No disposal is needed.
    /// </summary>
    /// <param name="ImageReferences"></param>
    public record StateToken(ImmutableList<ProcessedImage.WeakReference> ImageReferences)
    {
        public virtual bool Equals(StateToken? other)
        {
            if (other == null)
            {
                return false;
            }

            return ObjectHelpers.ListEquals(ImageReferences, other.ImageReferences);
        }

        public override int GetHashCode()
        {
            return ObjectHelpers.ListHashCode(ImageReferences);
        }
    }
}