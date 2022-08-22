using CoreAnimation;
using Eto.Drawing;
using Eto.Forms;
using Eto.Mac;

namespace NAPS2.EtoForms.Mac;

public class MacListView<T> : NSCollectionViewDelegateFlowLayout, IListView<T> where T : notnull
{
    private readonly ListViewBehavior<T> _behavior;
    private readonly NSCollectionView _view = new();
    private readonly NSCollectionViewFlowLayout _layout;
    private readonly DataSource<T> _dataSource;

    private ListSelection<T> _selection = ListSelection.Empty<T>();
    private bool _refreshing;

    public MacListView(ListViewBehavior<T> behavior)
    {
        _behavior = behavior;
        _layout = new NSCollectionViewFlowLayout
        {
            SectionInset = new NSEdgeInsets(20, 20, 20, 20),
            MinimumInteritemSpacing = 15,
            MinimumLineSpacing = 15
        };
        _dataSource = new DataSource<T>(this, _behavior);
        _view.DataSource = _dataSource;
        _view.Delegate = this;
        _view.CollectionViewLayout = _layout;
        _view.Selectable = true;
        _view.AllowsMultipleSelection = true;
    }

    public int ImageSize
    {
        get => (int) _layout.ItemSize.Width;
        set => _layout.ItemSize = new CGSize(value, value);
    }

    // TODO: Properties here vs on behavior?
    public bool AllowDrag { get; set; }

    public bool AllowDrop { get; set; }

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (!AllowDrop)
        {
            return;
        }
        e.Effects = _behavior.GetDropEffect(e.Data);
    }

    public Control Control => _view.ToEto();

    public event EventHandler? SelectionChanged;

    public event EventHandler? ItemClicked;

    public event EventHandler<DropEventArgs>? Drop;

    public void SetItems(IEnumerable<T> items)
    {
        _dataSource.Items.Clear();
        _dataSource.Items.AddRange(items);
        _view.ReloadData();
    }

    // TODO: Do we need this method? Clean up the name/doc at least
    public void RegenerateImages()
    {
        _view.ReloadData();
    }

    public void ApplyDiffs(ListViewDiffs<T> diffs)
    {
        foreach (var op in diffs.AppendOperations)
        {
            _dataSource.Items.Add(op.Item);
        }
        foreach (var op in diffs.ReplaceOperations)
        {
            _dataSource.Items[op.Index] = op.Item;
        }
        foreach (var op in diffs.TrimOperations)
        {
            _dataSource.Items.RemoveRange(_dataSource.Items.Count - op.Count, op.Count);
        }
        _view.ReloadData();
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
            UpdateViewSelection();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UpdateViewSelection()
    {
        if (!_refreshing)
        {
            _refreshing = true;
            _view.SelectionIndexes =
                NSIndexSet.FromArray(_selection.ToSelectedIndices(_dataSource.Items).ToArray());
            _refreshing = false;
        }
    }

    public override void ItemsSelected(NSCollectionView collectionView, NSSet indexPaths)
    {
        _selection = ListSelection.FromSelectedIndices(_dataSource.Items,
            indexPaths.Cast<NSIndexPath>().Select(x => (int) x.Item));
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public override void ItemsChanged(NSCollectionView collectionView, NSSet indexPaths, NSCollectionViewItemHighlightState highlightState)
    {
        UpdateViewSelection();
    }

    public override CGSize SizeForItem(NSCollectionView collectionView, NSCollectionViewLayout collectionViewLayout, NSIndexPath indexPath)
    {
        var item = _dataSource.Items[(int) indexPath.Item];
        var size = _behavior.GetImage(item, ImageSize).Size;
        var max = (double) Math.Max(size.Width, size.Height);
        return new CGSize(size.Width * ImageSize / max, size.Height * ImageSize / max);
    }
}

public class DataSource<T> : NSCollectionViewDataSource where T : notnull
{
    private readonly IListView<T> _listView;
    private readonly ListViewBehavior<T> _behavior;

    public DataSource(IListView<T> listView, ListViewBehavior<T> behavior)
    {
        _listView = listView;
        _behavior = behavior;
    }

    public List<T> Items { get; } = new();

    public override nint GetNumberofItems(NSCollectionView collectionView, nint section)
    {
        return Items.Count;
    }

    public override NSCollectionViewItem GetItem(NSCollectionView collectionView, NSIndexPath indexPath)
    {
        var i = (int) indexPath.Item;
        return new Cell
        {
            CellImage = _behavior.GetImage(Items[i], _listView.ImageSize)
        };
    }
}

public class Cell : NSCollectionViewItem
{
    private bool _selected;

    public Image CellImage { get; set; }

    public override void LoadView()
    {
        var imageView = new NSImageView
        {
            Image = CellImage.ToNS()
        };
        Console.WriteLine("Setting up imageview layer " + CellImage.Width + " " + CellImage.Height);
        imageView.WantsLayer = true;
        imageView.CanDrawSubviewsIntoLayer = true;
        imageView.Frame = new CGRect(0, 0, CellImage.Width, CellImage.Height);
        var layer = new CALayer();
        layer.Frame = new CGRect(0, 0, CellImage.Width, CellImage.Height);
        layer.CornerRadius = 4;
        layer.MasksToBounds = true;
        layer.Contents = CellImage.ToCG();
        layer.ZPosition = 1000;
        imageView.Layer = layer;
        View = imageView;
        UpdateViewForSelectedState();
    }

    public override bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            UpdateViewForSelectedState();
        }
    }

    private void UpdateViewForSelectedState()
    {
        Console.WriteLine("UpdateViewForSelectedState " + Selected);
        var layer = ((NSImageView) View).Layer;
        layer.BorderWidth = Selected ? 4 : 1;
        layer.BorderColor = Selected ? NSColor.SelectedContentBackground.ToCG() : NSColor.Black.ToCG();
    }
}