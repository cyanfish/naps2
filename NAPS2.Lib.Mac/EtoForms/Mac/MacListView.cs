using Eto.Forms;
using Eto.Mac.Forms.Menu;

namespace NAPS2.EtoForms.Mac;

public class MacListView<T> : NSCollectionViewDelegateFlowLayout, IListView<T> where T : notnull
{
    private readonly ListViewBehavior<T> _behavior;
    private readonly NSCollectionView _view = new();
    private readonly NSScrollView _scrollView = new();
    private readonly Panel _panel = new();
    private readonly NSCollectionViewFlowLayout _layout;
    private readonly ListViewDataSource<T> _dataSource;

    private ListSelection<T> _selection = ListSelection.Empty<T>();
    private bool _refreshing;
    private ContextMenu? _contextMenu;

    public MacListView(ListViewBehavior<T> behavior)
    {
        _behavior = behavior;
        _layout = new CustomFlowLayout
        {
            SectionInset = behavior.ShowLabels
                ? new NSEdgeInsets(5, 5, 5, 5)
                : new NSEdgeInsets(20, 20, 20, 20),
            MinimumInteritemSpacing = behavior.ShowLabels ? 5 : 15,
            MinimumLineSpacing = behavior.ShowLabels ? 5 : 15,
            TopAlign = behavior.ShowLabels,
            LeftGravity = false // TODO: I prefer this true, but it glitches out selection
        };
        _dataSource = new ListViewDataSource<T>(this, _behavior);
        _view.DataSource = _dataSource;
        _view.Delegate = this;
        _view.CollectionViewLayout = _layout;
        _view.Selectable = true;
        _view.AllowsMultipleSelection = behavior.MultiSelect;
        _scrollView.DocumentView = _view;
        _panel.Content = _scrollView.ToEto();
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

    public Control Control => _panel;

    public ContextMenu? ContextMenu
    {
        get => _contextMenu;
        set
        {
            _contextMenu = value;
            _view.Menu = (_contextMenu?.Handler as ContextMenuHandler)?.Control;
        }
    }

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
            _view.SelectionIndexes.Select(x => (int) x));
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public override void ItemsDeselected(NSCollectionView collectionView, NSSet indexPaths)
    {
        _selection = ListSelection.FromSelectedIndices(_dataSource.Items,
            _view.SelectionIndexes.Select(x => (int) x));
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public override void ItemsChanged(NSCollectionView collectionView, NSSet indexPaths,
        NSCollectionViewItemHighlightState highlightState)
    {
        UpdateViewSelection();
    }

    public override CGSize SizeForItem(NSCollectionView collectionView, NSCollectionViewLayout collectionViewLayout,
        NSIndexPath indexPath)
    {
        var item = _dataSource.Items[(int) indexPath.Item];
        if (_behavior.ShowLabels)
        {
            using var image = _behavior.GetImage(item, ImageSize);
            using var listItem = new ListViewItem(image, _behavior.GetLabel(item));
            listItem.LoadView();
            return listItem.View.FittingSize;
        }
        else
        {
            var size = _behavior.GetImage(item, ImageSize).Size;
            var max = (double) Math.Max(size.Width, size.Height);
            return new CGSize(size.Width * ImageSize / max, size.Height * ImageSize / max);
        }
    }
}