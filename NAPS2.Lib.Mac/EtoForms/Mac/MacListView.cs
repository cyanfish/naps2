using Eto.Forms;
using Eto.Mac;
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
        if (_behavior.AllowDragDrop)
        {
            _view.RegisterForDraggedTypes(new[] { _behavior.CustomDragDataType });
        }
        if (_behavior.AllowFileDrop)
        {
            _view.RegisterForDraggedTypes(new string[] { NSPasteboard.NSPasteboardTypeFileUrl });
        }
        _scrollView.DocumentView = _view;
        _panel.Content = _scrollView.ToEto();
    }

    public int ImageSize
    {
        get => (int) _layout.ItemSize.Width;
        set => _layout.ItemSize = new CGSize(value, value);
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

    // TODO: Implement item double-click
#pragma warning disable CS0067
    public event EventHandler? ItemClicked;
#pragma warning restore CS0067

    // TODO: Implement drag/drop
#pragma warning disable CS0067
    public event EventHandler<DropEventArgs>? Drop;
#pragma warning restore CS0067

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

    public override bool AcceptDrop(NSCollectionView collectionView, INSDraggingInfo draggingInfo,
        NSIndexPath indexPath,
        NSCollectionViewDropOperation dropOperation)
    {
        try
        {
            var position = (int) (dropOperation == NSCollectionViewDropOperation.Before
                ? indexPath.Item
                : indexPath.Item + 1);
            if (_behavior.AllowDragDrop && GetCustomData(draggingInfo, out var data))
            {
                Drop?.Invoke(this, new DropEventArgs(position, data));
                return true;
            }
            if (_behavior.AllowFileDrop && GetFilePaths(draggingInfo, out var filePaths))
            {
                Drop?.Invoke(this, new DropEventArgs(position, filePaths));
                return true;
            }
        }
        catch (Exception ex)
        {
            Log.ErrorException("Error dropping data", ex);
        }
        return false;
    }

    private bool GetCustomData(INSDraggingInfo draggingInfo, out byte[] data)
    {
        if (draggingInfo.DraggingPasteboard.CanReadItemWithDataConformingToTypes(new[]
                { _behavior.CustomDragDataType }))
        {
            var items = draggingInfo.DraggingPasteboard.PasteboardItems;
            data = items.Length > 1
                ? _behavior.MergeCustomDragData(items
                    .Select(x => x.GetDataForType(_behavior.CustomDragDataType).ToArray()).ToArray())
                : items[0].GetDataForType(_behavior.CustomDragDataType).ToArray();
            return true;
        }
        data = null!;
        return false;
    }

    private bool GetFilePaths(INSDraggingInfo draggingInfo, out IEnumerable<string> filePaths)
    {
        if (draggingInfo.DraggingPasteboard.CanReadItemWithDataConformingToTypes(new string[]
                { NSPasteboard.NSPasteboardTypeFileUrl }))
        {
            var items = draggingInfo.DraggingPasteboard.PasteboardItems;
            filePaths =
                items.Select(x =>
                {
                    var url = new NSUrl(x.GetStringForType(NSPasteboard.NSPasteboardTypeFileUrl));
                    return url.FilePathUrl?.Path;
                }).WhereNotNull();
            return true;
        }
        filePaths = null!;
        return false;
    }

    public override NSDragOperation ValidateDrop(NSCollectionView collectionView, INSDraggingInfo draggingInfo,
        ref NSIndexPath proposedDropIndexPath, ref NSCollectionViewDropOperation proposedDropOperation)
    {
        if (_behavior.AllowDragDrop && GetCustomData(draggingInfo, out var data))
        {
            return _behavior.GetCustomDragEffect(data).ToNS();
        }
        if (_behavior.AllowFileDrop && draggingInfo.DraggingPasteboard.CanReadItemWithDataConformingToTypes(new string[]
                { NSPasteboard.NSPasteboardTypeFileUrl }))
        {
            return NSDragOperation.Copy;
        }
        return NSDragOperation.None;
    }

    public override bool CanDragItems(NSCollectionView collectionView, NSIndexSet indexes, NSEvent evt)
    {
        return _behavior.AllowDragDrop && indexes.Count > 0;
    }

    public override INSPasteboardWriting? GetPasteboardWriter(NSCollectionView collectionView, NSIndexPath indexPath)
    {
        var item = new NSPasteboardItem();
        var binaryData = _behavior.SerializeCustomDragData(new[] { _dataSource.Items[(int) indexPath.Item] });
        item.SetDataForType(NSData.FromArray(binaryData), _behavior.CustomDragDataType);
        return item;
    }

    public override void UpdateDraggingItemsForDrag(NSCollectionView collectionView, INSDraggingInfo draggingInfo)
    {
    }

    public override void DraggingSessionWillBegin(NSCollectionView collectionView, NSDraggingSession draggingSession,
        CGPoint screenPoint,
        NSIndexSet indexes)
    {
    }

    public override void DraggingSessionEnded(NSCollectionView collectionView, NSDraggingSession draggingSession,
        CGPoint screenPoint,
        NSDragOperation dragOperation)
    {
    }
}