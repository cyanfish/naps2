using System.Drawing;
using System.Windows.Forms;
using Eto.WinForms;
using NAPS2.WinForms;

namespace NAPS2.EtoForms.WinForms;

public class WinFormsListView<T> : IListView<T> where T : notnull
{
    private readonly ListView _view;
    private readonly ListViewBehavior<T> _behavior;

    private ListSelection<T> _selection = ListSelection.Empty<T>();
    private bool _refreshing;

    public WinFormsListView(ListViewBehavior<T> behavior)
    {
        _behavior = behavior;
        _view = behavior.ScrollOnDrag ? new DragScrollListView() : new ListView();
        _view.LargeImageList = new ImageList
        {
            ColorDepth = ColorDepth.Depth32Bit,
            TransparentColor = Color.Transparent
        };
        _view.View = View.LargeIcon;
        _view.MultiSelect = behavior.MultiSelect;
        WinFormsHacks.SetControlStyle(_view, ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint,
            true);

        _view.SelectedIndexChanged += OnSelectedIndexChanged;
        _view.ItemActivate += OnItemActivate;
        _view.ItemDrag += OnItemDrag;
        _view.DragEnter += OnDragEnter;
        _view.DragDrop += OnDragDrop;
        _view.DragOver += OnDragOver;
        _view.DragLeave += OnDragLeave;
        _view.MouseMove += OnMouseMove;
        _view.MouseLeave += OnMouseLeave;
    }

    public int ImageSize
    {
        get => _view.LargeImageList.ImageSize.Width;
        set => WinFormsHacks.SetImageSize(_view.LargeImageList, new Size(value, value));
    }

    // TODO: Properties here vs on behavior?
    public bool AllowDrag { get; set; }

    public bool AllowDrop
    {
        get => _view.AllowDrop;
        set => _view.AllowDrop = value;
    }

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (!AllowDrop)
        {
            return;
        }
        e.Effect = _behavior.GetDropEffect(e.Data.ToEto()).ToSwf();
    }

    public Eto.Forms.Control Control => Eto.Forms.WinFormsHelpers.ToEto(_view);

    public ListView NativeControl => _view;

    public event EventHandler? SelectionChanged;

    public event EventHandler? ItemClicked;

    public event EventHandler<DropEventArgs>? Drop;

    private ImageList.ImageCollection ImageList => _view.LargeImageList.Images;

    private ListView.ListViewItemCollection Items => _view.Items;

    public void SetItems(IEnumerable<T> items)
    {
        if (_refreshing)
        {
            throw new InvalidOperationException();
        }
        _refreshing = true;
        Items.Clear();
        ImageList.Clear();
        foreach (var item in items)
        {
            ImageList.Add(_behavior.GetImage(item, ImageSize).ToSD());
            var listViewItem = Items.Add(GetLabel(item), ImageList.Count - 1);
            listViewItem.Tag = item;
        }
        SetSelectedItems();
        _refreshing = false;
    }

    private void SetSelectedItems()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            Items[i].Selected = Selection.Contains((T) Items[i].Tag);
        }
    }

    // TODO: Do we need this method? Clean up the name/doc at least
    public void RegenerateImages()
    {
        if (_refreshing)
        {
            throw new InvalidOperationException();
        }
        _refreshing = true;
        _view.BeginUpdate();
        if (ImageList.Count > 0)
        {
            ImageList.Clear();
        }

        var list = new List<Image>();
        foreach (var image in Items.OfType<ListViewItem>().Select(x => (T) x.Tag))
        {
            list.Add(_behavior.GetImage(image, ImageSize).ToSD());
        }

        foreach (ListViewItem item in Items)
        {
            item.ImageIndex = item.Index;
        }

        ImageList.AddRange(list.ToArray());
        _view.EndUpdate();
        _refreshing = false;
    }

    public void ApplyDiffs(ListViewDiffs<T> diffs)
    {
        if (_refreshing)
        {
            throw new InvalidOperationException();
        }
        _refreshing = true;
        _view.BeginUpdate();

        // TODO: We might want to make the differ even smarter. e.g. maybe it can generate an arbitrary order of operations that minimizes update cost
        // example: clear then append 1 instead of delete all but 1
        var originalImagesList = Items.OfType<ListViewItem>().Select(x => (T) x.Tag).ToList();
        var originalImagesSet = new HashSet<T>(originalImagesList);
        if (!diffs.AppendOperations.Any() && !diffs.ReplaceOperations.Any() &&
            diffs.TrimOperations.Any(x => x.Count == Items.Count))
        {
            ImageList.Clear();
            Items.Clear();
        }
        else
        {
            foreach (var append in diffs.AppendOperations)
            {
                // TODO: We want to use the thumbnail bitmap from the ImageRenderState, though we need to consider lifetime/disposal
                // TODO: Use AddRange instead?
                // TODO: Add this to the new ImageListViewBehavior
                //  _thumbnailProvider.GetThumbnail(append.Image.Source, ThumbnailSize)
                ImageList.Add(_behavior.GetImage(append.Item, ImageSize).ToSD());
                var item = Items.Add(GetLabel(append.Item));
                item.Tag = append.Item;
                // TODO: This isn't used above, is it needed?
                item.ImageIndex = ImageList.Count - 1;
            }
            foreach (var replace in diffs.ReplaceOperations)
            {
                ImageList[replace.Index] = _behavior.GetImage(replace.Item, ImageSize).ToSD();
                Items[replace.Index].Tag = replace.Item;
            }
            foreach (var trim in diffs.TrimOperations)
            {
                for (int i = 0; i < trim.Count; i++)
                {
                    ImageList.RemoveAt(ImageList.Count - 1);
                    Items.RemoveAt(Items.Count - 1);
                }
            }
        }
        SetSelectedItems();
        var newImagesList = Items.OfType<ListViewItem>().Select(x => (T) x.Tag).ToList();
        var newImagesSet = new HashSet<T>(newImagesList);
        if (originalImagesSet.SetEquals(newImagesSet) && !originalImagesList.SequenceEqual(newImagesList))
        {
            ScrollToSelection();
        }
        _view.EndUpdate();
        _view.Invalidate();
        _refreshing = false;
    }

    private void ScrollToSelection()
    {
        // If selection is empty (e.g. after interleave), this scrolls to top
        _view.EnsureVisible(_view.SelectedIndices.OfType<int>().LastOrDefault());
        _view.EnsureVisible(_view.SelectedIndices.OfType<int>().FirstOrDefault());
    }

    public ListSelection<T> Selection
    {
        get => _selection;
        set
        {
            if (_selection == value)
            {
                return;
            }
            _selection = value ?? throw new ArgumentNullException(nameof(value));
            if (!_refreshing)
            {
                _refreshing = true;
                SetSelectedItems();
                _refreshing = false;
            }
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private string GetLabel(T item)
    {
        if (!_behavior.ShowLabels)
        {
            return PlatformCompat.Runtime.UseSpaceInListViewItem ? " " : "";
        }
        return _behavior.GetLabel(item);
    }

    private void OnSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (!_refreshing)
        {
            _refreshing = true;
            Selection = ListSelection.From(_view.SelectedItems.Cast<ListViewItem>().Select(x => (T) x.Tag));
            _refreshing = false;
        }
    }

    private void OnItemActivate(object? sender, EventArgs e)
    {
        ItemClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnItemDrag(object? sender, ItemDragEventArgs e)
    {
        if (!AllowDrag)
        {
            return;
        }
        // Provide drag data
        if (Selection.Count > 0)
        {
            var dataObject = new DataObject();
            _behavior.SetDragData(Selection, dataObject.ToEto());
            _view.DoDragDrop(dataObject, DragDropEffects.Move | DragDropEffects.Copy);
        }
    }

    private void OnDragDrop(object? sender, DragEventArgs e)
    {
        var index = GetDragIndex(e);
        if (index != -1)
        {
            Drop?.Invoke(this, new DropEventArgs(index, e.Data.ToEto()));
        }
        _view.InsertionMark.Index = -1;
    }

    private void OnDragLeave(object? sender, EventArgs e)
    {
        _view.InsertionMark.Index = -1;
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (e.Effect == DragDropEffects.Move && Items.Count > 0)
        {
            var index = GetDragIndex(e);
            if (index == Items.Count)
            {
                _view.InsertionMark.Index = index - 1;
                _view.InsertionMark.AppearsAfterItem = true;
            }
            else
            {
                _view.InsertionMark.Index = index;
                _view.InsertionMark.AppearsAfterItem = false;
            }
        }
    }

    private int GetDragIndex(DragEventArgs e)
    {
        if (Items.Count == 0)
        {
            return 0;
        }
        Point cp = _view.PointToClient(new Point(e.X, e.Y));
        ListViewItem? dragToItem = _view.GetItemAt(cp.X, cp.Y);
        if (dragToItem == null)
        {
            var items = Items.Cast<ListViewItem>().ToList();
            var minY = items.Select(x => x.Bounds.Top).Min();
            var maxY = items.Select(x => x.Bounds.Bottom).Max();
            if (cp.Y < minY)
            {
                cp.Y = minY;
            }
            if (cp.Y > maxY)
            {
                cp.Y = maxY;
            }
            var row = items.Where(x => x.Bounds.Top <= cp.Y && x.Bounds.Bottom >= cp.Y).OrderBy(x => x.Bounds.X)
                .ToList();
            dragToItem = row.FirstOrDefault(x => x.Bounds.Right >= cp.X) ?? row.LastOrDefault();
        }
        if (dragToItem == null)
        {
            return -1;
        }
        int dragToIndex = dragToItem.Index;
        if (cp.X > (dragToItem.Bounds.X + dragToItem.Bounds.Width / 2))
        {
            dragToIndex++;
        }
        return dragToIndex;
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (_behavior.UseHandCursor)
        {
            _view.Cursor = _view.GetItemAt(e.X, e.Y) == null ? Cursors.Default : Cursors.Hand;
        }
    }

    private void OnMouseLeave(object? sender, EventArgs e)
    {
        if (_behavior.UseHandCursor)
        {
            _view.Cursor = Cursors.Default;
        }
    }
}