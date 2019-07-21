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
            // TODO: Need a selection change event
            set => selection = value ?? throw new ArgumentNullException(nameof(value));
        }

        public void Mutate(ListMutation<ScannedImage> mutation, ListSelection<ScannedImage> selectionToMutate = null)
        {
            // TODO: Make sure to update the selection
            // TODO: Need to improve change detection (e.g. transforms) 
            // TODO: Note that change tracking is closely related to Undo/Redo (comparing mementos), so keep that in mind
            // TODO: Not sure if this should be here or in UserActions
            var originalList = Images.ToList();
            if (selectionToMutate != null)
            {
                mutation.Apply(Images, ref selectionToMutate);
            }
            else
            {
                var selectionRef = Selection;
                mutation.Apply(Images, ref selectionRef);
                Selection = selectionRef;
            }
            // TODO (events - update and delete)
            // UpdateThumbnails(selection.ToSelectedIndices(imageList.Images), true, mutation.OnlyAffectsSelectionRange);
        }
        
        // TODO: Undo/redo etc. thoughts:
        // A memento is a copy of a ScannedImage list with Snapshots
        // Need to add a Restore operation for Snapshots
        // This lends itself well to undoing deletions (since snapshots already persist on disk).
        // However, we may need to tweak the way metadata disposal works (either dispose on ScannedImage.Dispose and restore on Snapshot.Restore, or add a Deleted flag to metadata)
        // Restoring on a disposed ScannedImage is iffy. Maybe Restore and RestoreDeleted should be separate.
        // Mementos should be comparable. When we mutate, we should add the end memento to the undo stack iff it's different than the top.
        // Rather than ChangeTracker this should be encompassed in some UndoQueue class, which has a memento for the last saved state and can compare in order to determine if changes have been made.
        // Perhaps everything that changes any image (including ImageForm stuff, insertions) should be encompassed by a mutation.

        public Task MutateAsync(ListMutation<ScannedImage> mutation, ListSelection<ScannedImage> selectionToMutate = null)
        {
            return Task.Run(() => Mutate(mutation, selectionToMutate));
        }
    }
}
