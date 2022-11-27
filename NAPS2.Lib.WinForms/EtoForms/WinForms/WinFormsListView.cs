using System.Drawing;
using System.Windows.Forms;
using Eto.WinForms;
using NAPS2.WinForms;

namespace NAPS2.EtoForms.WinForms;

public class WinFormsListView<T> : IListView<T> where T : notnull
{
    private static readonly Pen DefaultPen = new(Color.Black, 1);
    private static readonly Pen SelectionPen = new(Color.FromArgb(0, 0x66, 0xe8), 3);

    private readonly ListView _view;
    private readonly ListViewBehavior<T> _behavior;

    private ListSelection<T> _selection = ListSelection.Empty<T>();
    private bool _refreshing;

    public WinFormsListView(ListViewBehavior<T> behavior)
    {
        _behavior = behavior;
        _view = behavior.ScrollOnDrag ? new DragScrollListView() : new ListView();
        _view.MultiSelect = behavior.MultiSelect;
        if (_behavior.Checkboxes)
        {
            _view.View = View.List;
            _view.CheckBoxes = true;
            _view.ItemChecked += OnSelectedIndexChanged;
        }
        else
        {
            _view.View = View.LargeIcon;
            _view.LargeImageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                TransparentColor = Color.Transparent
            };
            WinFormsHacks.SetControlStyle(_view,
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint,
                true);
            _view.SelectedIndexChanged += OnSelectedIndexChanged;
        }

        _view.ItemActivate += OnItemActivate;
        _view.ItemDrag += OnItemDrag;
        _view.DragEnter += OnDragEnter;
        _view.DragDrop += OnDragDrop;
        _view.DragOver += OnDragOver;
        _view.DragLeave += OnDragLeave;
        _view.MouseMove += OnMouseMove;
        _view.MouseLeave += OnMouseLeave;

        _view.OwnerDraw = true;
        _view.DrawItem += ViewOnDrawItem;
    }

    private void ViewOnDrawItem(object sender, DrawListViewItemEventArgs e)
    {
        int width, height;
        var image = ImageList[e.Item.Index];
        if (image.Width > image.Height)
        {
            width = ImageSize;
            height = (int) Math.Round(width * (image.Height / (double) image.Width));
        }
        else
        {
            height = ImageSize;
            width = (int) Math.Round(height * (image.Width / (double) image.Height));
        }
        var x = e.Bounds.Left + (e.Bounds.Width - width) / 2;
        var y = e.Bounds.Top + (e.Bounds.Height - height) / 2;
        e.Graphics.DrawImage(image, new Rectangle(x, y, width, height));

        // Draw border
        if (e.Item.Selected)
        {
            e.Graphics.DrawRectangle(SelectionPen, x - 2, y - 2, width + 3, height + 3);
        }
        else
        {
            e.Graphics.DrawRectangle(DefaultPen, x, y, width, height);
        }
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

    private ListView.ListViewItemCollection Items => _view.Items;

    private List<Image> ImageList { get; } = new();

    public void SetItems(IEnumerable<T> items)
    {
        if (_refreshing)
        {
            throw new InvalidOperationException();
        }
        _refreshing = true;
        Items.Clear();
        ClearImageList();
        foreach (var item in items)
        {
            if (!_behavior.Checkboxes)
            {
                ImageList.Add(_behavior.GetImage(item, ImageSize).ToSD());
            }
            var listViewItem = Items.Add(GetLabel(item));
            listViewItem.Tag = item;
        }
        SetSelectedItems();
        _refreshing = false;
    }

    private void ClearImageList()
    {
        if (!_behavior.Checkboxes)
        {
            foreach (var image in ImageList) image.Dispose();
            ImageList.Clear();
        }
    }

    private void SetSelectedItems()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (_behavior.Checkboxes)
            {
                Items[i].Checked = Selection.Contains((T) Items[i].Tag);
            }
            else
            {
                Items[i].Selected = Selection.Contains((T) Items[i].Tag);
            }
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
        ClearImageList();

        foreach (var image in Items.OfType<ListViewItem>().Select(x => (T) x.Tag))
        {
            ImageList.Add(_behavior.GetImage(image, ImageSize).ToSD());
        }

        foreach (ListViewItem item in Items)
        {
            item.ImageIndex = item.Index;
        }

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
        var originalItemsList = Items.OfType<ListViewItem>().Select(x => (T) x.Tag).ToList();
        var originalItemsSet = new HashSet<T>(originalItemsList);
        if (!diffs.AppendOperations.Any() && !diffs.ReplaceOperations.Any() &&
            diffs.TrimOperations.Any(x => x.Count == Items.Count))
        {
            ClearImageList();
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
                var item = Items.Add(GetLabel(append.Item));
                item.Tag = append.Item;
                if (!_behavior.Checkboxes)
                {
                    ImageList.Add(_behavior.GetImage(append.Item, ImageSize).ToSD());
                }
            }
            foreach (var replace in diffs.ReplaceOperations)
            {
                if (!_behavior.Checkboxes)
                {
                    ImageList[replace.Index].Dispose();
                    ImageList[replace.Index] = _behavior.GetImage(replace.Item, ImageSize).ToSD();
                }
                Items[replace.Index].Tag = replace.Item;
            }
            foreach (var trim in diffs.TrimOperations)
            {
                for (int i = 0; i < trim.Count; i++)
                {
                    if (!_behavior.Checkboxes)
                    {
                        ImageList[ImageList.Count - 1].Dispose();
                        ImageList.RemoveAt(ImageList.Count - 1);
                    }
                    Items.RemoveAt(Items.Count - 1);
                }
            }
        }
        SetSelectedItems();
        var newItemsList = Items.OfType<ListViewItem>().Select(x => (T) x.Tag).ToList();
        var newItemsSet = new HashSet<T>(newItemsList);
        if (originalItemsSet.SetEquals(newItemsSet) && !originalItemsList.SequenceEqual(newItemsList))
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

    private string GetLabel(T item) => _behavior.ShowLabels ? _behavior.GetLabel(item) : "";

    private void OnSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (!_refreshing)
        {
            _refreshing = true;
            var items = _behavior.Checkboxes
                ? _view.CheckedItems.Cast<ListViewItem>()
                : _view.SelectedItems.Cast<ListViewItem>();
            Selection = ListSelection.From(items.Select(x => (T) x.Tag));
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