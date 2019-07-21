using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Util;

namespace NAPS2.Images
{
    public class ScannedImageList
    {
        private Memento savedState = Memento.Empty;
        private ListSelection<ScannedImage> selection;

        public ScannedImageList()
        {
            Images = new List<ScannedImage>();
        }

        public ScannedImageList(List<ScannedImage> images)
        {
            Images = images;
        }

        public ThumbnailRenderer ThumbnailRenderer { get; set; }

        public List<ScannedImage> Images { get; }

        public Memento CurrentState => new Memento(Images);

        public Memento SavedState
        {
            get => savedState;
            set => savedState = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ListSelection<ScannedImage> Selection
        {
            get => selection;
            set => selection = value ?? throw new ArgumentNullException(nameof(value));
        }

        public event EventHandler SelectionChanged;

        public event EventHandler<ImagesUpdatedEventArgs> ImagesUpdated;

        public event EventHandler ImagesDeleted;

        public void Mutate(ListMutation<ScannedImage> mutation, ListSelection<ScannedImage> selectionToMutate = null)
        {
            // TODO: Not sure if this should be here or in UserActions
            int selectionMin = selection.ToSelectedIndices(Images).Min();
            int selectionMax = selection.ToSelectedIndices(Images).Max();
            if (selectionToMutate != null)
            {
                mutation.Apply(Images, ref selectionToMutate);
            }
            else
            {
                var selectionRef = Selection;
                mutation.Apply(Images, ref selectionRef);
                if (selectionRef != Selection)
                {
                    Selection = selectionRef;
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    selectionMin = Math.Min(selectionMin, selection.ToSelectedIndices(Images).Min());
                    selectionMax = Math.Max(selectionMax, selection.ToSelectedIndices(Images).Max());
                }
            }
            if (mutation.IsDeletion)
            {
                ImagesDeleted?.Invoke(this, EventArgs.Empty);
            }
            else if(mutation.OnlyAffectsSelectionRange)
            {
                ImagesUpdated?.Invoke(this, new ImagesUpdatedEventArgs
                {
                    AffectedRangeMin = selectionMin,
                    AffectedRangeMax = selectionMax
                });
            }
            else
            {
                ImagesUpdated?.Invoke(this, new ImagesUpdatedEventArgs
                {
                    AffectedRangeMin = 0,
                    AffectedRangeMax = Images.Count
                });
            }
        }

        // TODO: Undo/redo etc. thoughts:
        // A memento is a copy of a ScannedImage list with Snapshots
        // Need to add a Restore operation for Snapshots
        // This lends itself well to undoing deletions (since snapshots already persist on disk).
        // However, we may need to tweak the way metadata disposal works (either dispose on ScannedImage.Dispose and restore on Snapshot.Restore, or add a Deleted flag to metadata)
        // Restoring on a disposed ScannedImage is iffy. Maybe Restore and RestoreDeleted should be separate.
        // Perhaps everything that changes any image (including ImageForm stuff, insertions) should be encompassed by a mutation.

        public Task MutateAsync(ListMutation<ScannedImage> mutation, ListSelection<ScannedImage> selectionToMutate = null)
        {
            return Task.Run(() => Mutate(mutation, selectionToMutate));
        }

        public class ImagesUpdatedEventArgs : EventArgs
        {
            public int AffectedRangeMin { get; set; }
            
            public int AffectedRangeMax { get; set; }
        }
    }
}
