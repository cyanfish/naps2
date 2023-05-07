using System.Drawing;
using System.Windows.Forms;
using Eto.WinForms;
using Eto.WinForms.Forms.Menu;
using NAPS2.EtoForms.Widgets;
using NAPS2.WinForms;
using ContextMenu = Eto.Forms.ContextMenu;

namespace NAPS2.EtoForms.WinForms;

public class WinFormsListView<T> : IListView<T> where T : notnull
{
    private readonly Naps2Config _config;

    private static readonly Pen DefaultPen = new(Color.Black, 1);
    private static readonly Pen SelectionPen = new(Color.FromArgb(0x60, 0xa0, 0xe8), 3);

    private readonly ListView _view;
    private readonly Eto.Forms.Control _viewEtoControl;
    private readonly ListViewBehavior<T> _behavior;

    private ListSelection<T> _selection = ListSelection.Empty<T>();
    private bool _refreshing;
    private ContextMenu? _contextMenu;

    public WinFormsListView(ListViewBehavior<T> behavior)
    {
        _behavior = behavior;
        _view = behavior.ScrollOnDrag ? new DragScrollListView() : new ListView();
        _view.MultiSelect = behavior.MultiSelect;

        _config = Naps2Config.Stub();
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

        _view.AllowDrop = _behavior.AllowDragDrop;
        _view.ItemActivate += OnItemActivate;
        _view.ItemDrag += OnItemDrag;
        _view.DragEnter += OnDragEnter;
        _view.DragDrop += OnDragDrop;
        _view.DragOver += OnDragOver;
        _view.DragLeave += OnDragLeave;
        _view.MouseMove += OnMouseMove;
        _view.MouseLeave += OnMouseLeave;

        _viewEtoControl = Eto.Forms.WinFormsHelpers.ToEto(_view);
        ImageList = UseCustomRendering
            ? new WinFormsImageList<T>.Custom(this, _behavior)
            : !_behavior.Checkboxes
                ? new WinFormsImageList<T>.Native(this, _behavior)
                : new WinFormsImageList<T>.Stub(this, _behavior);
        if (UseCustomRendering)
        {
            _view.OwnerDraw = true;
            _view.DrawItem += CustomRenderItem;
        }
    }

    private bool UseCustomRendering => !_behavior.ShowLabels && !_behavior.Checkboxes;


    private void CustomRenderItem(object? sender, DrawListViewItemEventArgs e)
    {
        int width, height;
        var image = ImageList.Get(e.Item);
        if (image.Width > image.Height)
        {
            width = ImageSize;
            height = (int) Math.Round(width * (image.Height / (double) image.Width));
        }
        else
        {
            if (_config.Get(c => c.EnableThumbnailText))
                height = ImageSize - 12;
            else
                height = ImageSize;
            width = (int) Math.Round(height * (image.Width / (double) image.Height));
        }
        var x = e.Bounds.Left + (e.Bounds.Width - width) / 2;
        var y = e.Bounds.Top + (e.Bounds.Height - height) / 2;
        e.Graphics.DrawImage(image, new Rectangle(x, y, width, height));
        if (_config.Get(c => c.EnableThumbnailText)) 
        { 
            // Draw the text below the image
            //var draw = _config.Get(c => c.EnableThumbnailText);
            // Create string to draw.
            String drawString = (e.ItemIndex+1).ToString() + " / "+_view.Items.Count.ToString(); 

            // Create font and brush.
            Font drawFont = new Font("Arial", 10);
            SolidBrush drawBrush = new SolidBrush(Color.Black);

            // Create point 
            float x1 = x + width/2;
            float y1 = y + height + 6;

            // Set format of string.
            StringFormat drawFormat = new StringFormat();
            drawFormat.Alignment = StringAlignment.Center;
        
            // Draw string to screen.
            e.Graphics.DrawString(drawString, drawFont, drawBrush, x1, y1, drawFormat);

        }
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

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        var data = e.Data.ToEto();
        if (data.Contains(_behavior.CustomDragDataType) && _behavior.AllowDragDrop)
        {
            e.Effect = _behavior.GetCustomDragEffect(data.GetData(_behavior.CustomDragDataType)).ToSwf();
        }
        else if (data.Contains("FileDrop") && _behavior.AllowFileDrop)
        {
            e.Effect = DragDropEffects.Copy;
        }
    }

    public Eto.Forms.Control Control => _viewEtoControl;

    public ContextMenu? ContextMenu
    {
        get => _contextMenu;
        set
        {
            _contextMenu = value;
            _view.ContextMenuStrip = (_contextMenu?.Handler as ContextMenuHandler)?.Control;
        }
    }

    public ListView NativeControl => _view;

    public event EventHandler? SelectionChanged;

    public event EventHandler? ItemClicked;

    public event EventHandler<DropEventArgs>? Drop;

    private ListView.ListViewItemCollection Items => _view.Items;

    private WinFormsImageList<T> ImageList { get; }

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
            var listViewItem = Items.Add(GetLabel(item));
            listViewItem.Tag = item;
            ImageList.Append(item, listViewItem);
        }
        SetSelectedItems();
        _refreshing = false;
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
        ImageList.Clear();

        var images = new List<Image>();
        foreach (ListViewItem listViewItem in Items)
        {
            var item = (T) listViewItem.Tag;
            images.Add(ImageList.PartialAppend(item));
        }
        ImageList.FinishPartialAppends(images);

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
                var listViewItem = Items.Add(GetLabel(append.Item));
                listViewItem.Tag = append.Item;
                ImageList.Append(append.Item, listViewItem);
            }
            foreach (var replace in diffs.ReplaceOperations)
            {
                Items[replace.Index].Tag = replace.Item;
                ImageList.Replace(replace.Item, replace.Index);
            }
            foreach (var trim in diffs.TrimOperations)
            {
                for (int i = 0; i < trim.Count; i++)
                {
                    Items.RemoveAt(Items.Count - 1);
                    ImageList.DeleteFromEnd();
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
        if (!_behavior.AllowDragDrop)
        {
            return;
        }
        // Provide drag data
        if (Selection.Count > 0)
        {
            var dataObject = new DataObject();
            dataObject.SetData(_behavior.CustomDragDataType, _behavior.SerializeCustomDragData(Selection.ToArray()));
            _view.DoDragDrop(dataObject, DragDropEffects.Move | DragDropEffects.Copy);
        }
    }

    private void OnDragDrop(object? sender, DragEventArgs e)
    {
        var index = GetDragIndex(e);
        if (index != -1)
        {
            var data = e.Data.ToEto();
            if (data.Contains(_behavior.CustomDragDataType))
            {
                Drop?.Invoke(this, new DropEventArgs(index, data.GetData(_behavior.CustomDragDataType)));
            }
            else if (data.Contains("FileDrop"))
            {
                var filePaths = (string[]) e.Data!.GetData(DataFormats.FileDrop);
                Drop?.Invoke(this, new DropEventArgs(index, filePaths));
            }
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