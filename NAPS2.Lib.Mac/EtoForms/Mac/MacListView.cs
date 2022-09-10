using Eto.Forms;

namespace NAPS2.EtoForms.Mac;

public class MacListView<T> : NSCollectionViewDelegateFlowLayout, IListView<T> where T : notnull
{
    private readonly ListViewBehavior<T> _behavior;
    private readonly NSCollectionView _view = new();
    private readonly NSCollectionViewFlowLayout _layout;
    private readonly ListViewDataSource<T> _dataSource;

    private ListSelection<T> _selection = ListSelection.Empty<T>();
    private bool _refreshing;

    public MacListView(ListViewBehavior<T> behavior)
    {
        _behavior = behavior;
        _layout = new LeftFlowLayout
        {
            SectionInset = new NSEdgeInsets(20, 20, 20, 20),
            MinimumInteritemSpacing = 15,
            MinimumLineSpacing = 15
        };
        _dataSource = new ListViewDataSource<T>(this, _behavior);
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
                NSIndexSet.FromArray(_selection.ToSelectedIndices(_dataSource.Items).Where(x => x != -1).ToArray());
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