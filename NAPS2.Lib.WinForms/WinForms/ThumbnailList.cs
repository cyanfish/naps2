using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using NAPS2.EtoForms.WinForms;

namespace NAPS2.WinForms
{
    public partial class ThumbnailList : DragScrollListView
    {
        private readonly UiThumbnailProvider _thumbnailProvider = new();
        private UiImageList _imageList;
        private ImageListSyncer _syncer;

        private ListSelection<UiImage> _lastSelection = ListSelection.Empty<UiImage>();
        private bool _disableControlSelectionChangedEvent;

        // TODO: We need to do this somewhere
        // Scroll to selection
        // If selection is empty (e.g. after interleave), this scrolls to top
        // thumbnailList1.EnsureVisible(SelectedIndices.LastOrDefault());
        // thumbnailList1.EnsureVisible(SelectedIndices.FirstOrDefault());

        public ThumbnailList()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            LargeImageList = ilThumbnailList;
            ItemSelectionChanged += ControlSelectionChanged;
        }

        public void Initialize(UiImageList imageList)
        {
            _imageList = imageList;
            // TODO: Do we need to dispose this? Technically we should on form close, but that's the app close anyway 
            _syncer = new ImageListSyncer(imageList, UpdateThumbnails, SynchronizationContext.Current);
            _imageList.ImagesUpdated += ImagesUpdated;
        }

        private void ControlSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (_disableControlSelectionChangedEvent) return;
            var selectedImages = Items.Cast<ListViewItem>().Where(x => x.Selected).Select(x => (ImageRenderState) x.Tag)
                .Select(x => x.Source);
            _imageList.UpdateSelection(ListSelection.From(selectedImages));
        }

        private void ImagesUpdated(object sender, EventArgs e)
        {
            var selection = _imageList.Selection;
            if (!Equals(selection, _lastSelection))
            {
                Invoke(() => UpdateControlSelection(selection));
            }
        }

        private void UpdateControlSelection(ListSelection<UiImage> selection)
        {
            _disableControlSelectionChangedEvent = true;
            var visibleImages = Items.Cast<ListViewItem>().Select(x => (ImageRenderState) x.Tag)
                .Select(x => x.Source).ToList();
            SelectedIndices.Clear();
            // TODO: Maybe store if the index is -1 so we know we need to update the selection when the thumbnail list is updated
            foreach (int i in selection.ToSelectedIndices(visibleImages).Where(x => x != -1))
            {
                SelectedIndices.Add(i);
            }
            _disableControlSelectionChangedEvent = false;
        }

        private void UpdateThumbnails(ImageListDiffs diffs)
        {
            lock (this)
            {
                BeginUpdate();

                // TODO: We might want to make the differ even smarter. e.g. maybe it can generate an arbitrary order of operations that minimizes update cost
                // example: clear then append 1 instead of delete all but 1
                if (!diffs.AppendOperations.Any() && !diffs.ReplaceOperations.Any() &&
                    diffs.TrimOperations.Any(x => x.Count == Items.Count))
                {
                    ilThumbnailList.Images.Clear();
                    Items.Clear();
                }
                else
                {
                    foreach (var append in diffs.AppendOperations)
                    {
                        // TODO: We want to use the thumbnail bitmap from the ImageRenderState, though we need to consider lifetime/disposal
                        // TODO: Use AddRange instead?
                        ilThumbnailList.Images.Add(_thumbnailProvider.GetThumbnail(append.Image.Source, ThumbnailSize));
                        var item = Items.Add(ItemText);
                        item.Tag = append.Image;
                        item.ImageIndex = ilThumbnailList.Images.Count - 1;
                    }
                    foreach (var replace in diffs.ReplaceOperations)
                    {
                        ilThumbnailList.Images[replace.Index] =
                            _thumbnailProvider.GetThumbnail(replace.Image.Source, ThumbnailSize);
                        Items[replace.Index].Tag = replace.Image;
                    }
                    foreach (var trim in diffs.TrimOperations)
                    {
                        for (int i = 0; i < trim.Count; i++)
                        {
                            ilThumbnailList.Images.RemoveAt(ilThumbnailList.Images.Count - 1);
                            Items.RemoveAt(Items.Count - 1);
                        }
                    }
                }
                // TODO: Maybe only call this if needed
                UpdateControlSelection(_imageList.Selection);
                EndUpdate();
            }
            Invalidate();
        }

        public int ThumbnailSize
        {
            get => ilThumbnailList.ImageSize.Width;
            set => ListViewImageSizeHack.SetImageSize(ilThumbnailList, new Size(value, value));
        }

        private string ItemText => PlatformCompat.Runtime.UseSpaceInListViewItem ? " " : "";

        // TODO: We can probably delete this, but need to compare perf vs the normal diff
        public void RegenerateThumbnailList(List<UiImage> images)
        {
            lock (this)
            {
                BeginUpdate();
                if (ilThumbnailList.Images.Count > 0)
                {
                    ilThumbnailList.Images.Clear();
                }

                var list = new List<Image>();
                foreach (var image in images)
                {
                    list.Add(_thumbnailProvider.GetThumbnail(image, ThumbnailSize));
                }

                foreach (ListViewItem item in Items)
                {
                    item.ImageIndex = item.Index;
                }

                ilThumbnailList.Images.AddRange(list.ToArray());
                EndUpdate();
            }
        }
    }
}