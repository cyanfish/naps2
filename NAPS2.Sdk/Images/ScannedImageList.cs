using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Util;

namespace NAPS2.Images
{
    public class ScannedImageList
    {
        private readonly ChangeTracker changeTracker;
        private ListSelection<ScannedImage> selection;

        public ScannedImageList(ChangeTracker changeTracker)
        {
            this.changeTracker = changeTracker;
            Images = new List<ScannedImage>();
        }

        public ScannedImageList(ChangeTracker changeTracker, List<ScannedImage> images)
        {
            this.changeTracker = changeTracker;
            Images = images;
        }

        public ThumbnailRenderer ThumbnailRenderer { get; set; }

        public List<ScannedImage> Images { get; }

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
            if (!Images.Any())
            {
                changeTracker.Clear();
            }
            else if (!originalList.SequenceEqual(Images))
            {
                changeTracker.Made();
            }
        }

        public Task MutateAsync(ListMutation<ScannedImage> mutation, ListSelection<ScannedImage> selectionToMutate = null)
        {
            return Task.Run(() => Mutate(mutation, selectionToMutate));
        }
    }
}
