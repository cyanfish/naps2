using Eto.Drawing;
using Eto.Forms;
using Eto.Mac;
using Eto.Mac.Forms.Menu;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Mac;

public class MacListView<T> : NSCollectionViewDelegateFlowLayout, IListView<T> where T : notnull
{
    private readonly ListViewBehavior<T> _behavior;
    private readonly NSCollectionView _view = new();
    private readonly NSScrollView _scrollView = new();
    private readonly Panel _panel = new();
    private readonly NSCollectionViewLayout _layout;
    private readonly ListViewDataSource<T> _dataSource;

    private ListSelection<T> _selection = ListSelection.Empty<T>();
    private bool _refreshing;
    private ContextMenu? _contextMenu;
    private NSIndexPath? _lastClickedIndexPath;

    public MacListView(ListViewBehavior<T> behavior)
    {
        _behavior = behavior;
        _layout = _behavior.Checkboxes
            ? new NSCollectionViewGridLayout
            {
                MinimumInteritemSpacing = 0,
                MinimumLineSpacing = 0,
                MinimumItemSize = new CGSize(200, 17)
            }
            : new CustomFlowLayout
            {
                SectionInset = behavior.ShowLabels
                    ? new NSEdgeInsets(5, 5, 5, 5)
                    : new NSEdgeInsets(20, 20, 20, 20),
                MinimumInteritemSpacing = behavior.ShowLabels ? 5 : 15,
                MinimumLineSpacing = behavior.ShowLabels ? 5 : 15,
                TopAlign = behavior.ShowLabels,
                LeftGravity = false // TODO: I prefer this true, but it glitches out selection
            };
        _dataSource = new ListViewDataSource<T>(this, _behavior, ItemChecked, ItemActivated);
        _view.DataSource = _dataSource;
        _view.Delegate = this;
        _view.CollectionViewLayout = _layout;
        _view.Selectable = !behavior.Checkboxes;
        _view.AllowsMultipleSelection = !behavior.Checkboxes && behavior.MultiSelect;
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

    private void ItemChecked(T item, bool isChecked)
    {
        _selection = ListSelection.From(isChecked ? _selection.Append(item) : _selection.Except(new[] { item }));
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ItemActivated(T item)
    {
        // TODO: Propagate the item back to the click handler
        // That doesn't matter on e.g. Windows where double clicking sets the item as the sole selection. But on Mac
        // it's possible to select multiple items and double-click on one without losing your selection.
        ItemClicked?.Invoke(this, EventArgs.Empty);
    }

    public Size ImageSize
    {
        get => Size.Truncate(((NSCollectionViewFlowLayout) _layout).ItemSize.ToEto());
        set => ((NSCollectionViewFlowLayout) _layout).ItemSize = new CGSize(value.Width, value.Height);
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
        if (_dataSource.Items.Count == 0)
        {
            return;
        }
        _view.ReloadData();
    }

    public void ApplyDiffs(ListViewDiffs<T> diffs)
    {
        var originalItems = _dataSource.Items.ToList();
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
        if (IsSingleItemMove(originalItems, diffs, out var fromIndex, out var toIndex))
        {
            // If we're moving a single item, we can animate it + also improve selection behavior by doing a granular
            // move instead of reloading the whole list.
            // TODO: Should we do this in SetItems too? Or alternatively have ProfilesForm use ApplyDiffs?
            // TODO: Can we do this for anything else (e.g. multi-item moves)?
            var animator = (NSCollectionView) _view.Animator;
            animator.MoveItem(NSIndexPath.Create(0, fromIndex), NSIndexPath.Create(0, toIndex));
        }
        else if (!diffs.AppendOperations.Any() && !diffs.TrimOperations.Any())
        {
            // Only updating items
            var indexPaths = diffs.ReplaceOperations.Select(op => NSIndexPath.Create(0, op.Index)).ToArray();
            _view.ReloadItems(new NSSet<NSIndexPath>(indexPaths));
        }
        else
        {
            _view.ReloadData();
        }
    }

    private bool IsSingleItemMove(List<T> originalItems, ListViewDiffs<T> diffs, out int fromIndex, out int toIndex)
    {
        fromIndex = toIndex = 0;
        if (diffs.AppendOperations.Any() || diffs.TrimOperations.Any())
        {
            return false;
        }
        var replaces = diffs.ReplaceOperations;
        if (replaces.Count < 2)
        {
            return false;
        }

        // If this is a single item move, it's either moving from the highest index to the lowest index, or vice versa.
        // We can simulate each move on the original list of items and compare the results against the actual result
        // of all the diffs. This seems like the simplest solution.
        var lowest = replaces.Min(x => x.Index);
        var highest = replaces.Max(x => x.Index);

        var moveToLowest = originalItems.ToList();
        moveToLowest.RemoveAt(highest);
        moveToLowest.Insert(lowest, diffs.ReplaceOperations.Single(x => x.Index == lowest).Item);
        if (moveToLowest.SequenceEqual(_dataSource.Items))
        {
            fromIndex = highest;
            toIndex = lowest;
            return true;
        }

        var moveToHighest = originalItems.ToList();
        moveToHighest.RemoveAt(lowest);
        moveToHighest.Insert(highest, diffs.ReplaceOperations.Single(x => x.Index == highest).Item);
        if (moveToHighest.SequenceEqual(_dataSource.Items))
        {
            fromIndex = lowest;
            toIndex = highest;
            return true;
        }

        return false;
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
            if (_behavior.Checkboxes)
            {
                // TODO: Support this?
            }
            else
            {
                _view.SelectionIndexes =
                    NSIndexSet.FromArray(_selection.ToSelectedIndices(_dataSource.Items).Where(x => x != -1).ToArray());
            }
            _refreshing = false;
        }
    }

    public override void ItemsSelected(NSCollectionView collectionView, NSSet indexPaths)
    {
        if (_behavior.Checkboxes) return;
        _selection = ListSelection.FromSelectedIndices(_dataSource.Items,
            _view.SelectionIndexes.Select(x => (int) x));
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public override void ItemsDeselected(NSCollectionView collectionView, NSSet indexPaths)
    {
        if (_behavior.Checkboxes) return;
        _selection = ListSelection.FromSelectedIndices(_dataSource.Items,
            _view.SelectionIndexes.Select(x => (int) x));
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public override NSSet ShouldDeselectItems(NSCollectionView collectionView, NSSet indexPaths)
    {
        _lastClickedIndexPath = null;
        return indexPaths;
    }

    public override NSSet ShouldSelectItems(NSCollectionView collectionView, NSSet indexPaths)
    {
        // We want shift+click to select a range, instead of behaving the same as command+click.
        if (indexPaths.Count != 1)
        {
            _lastClickedIndexPath = null;
            return indexPaths;
        }
        var indexPath = (NSIndexPath) indexPaths.AnyObject;
        var prevIndexPath = _lastClickedIndexPath;
        _lastClickedIndexPath = indexPath;

        var isShiftHeld = (NSEvent.CurrentModifierFlags & NSEventModifierMask.ShiftKeyMask) != 0;
        if (!isShiftHeld || prevIndexPath == null)
        {
            return indexPaths;
        }
        var newIndexPaths = new NSMutableSet();
        var min = Math.Min((int) indexPath.Item, (int) prevIndexPath.Item);
        var max = Math.Max((int) indexPath.Item, (int) prevIndexPath.Item);
        for (var i = min; i <= max; i++)
        {
            newIndexPaths.Add(NSIndexPath.Create(0, i));
        }
        return newIndexPaths;
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
            using var image = _behavior.Checkboxes ? null : _behavior.GetImage(item, ImageSize);
            using var listItem = new ListViewItem(
                image, _behavior.GetLabel(item), _behavior.Checkboxes, _behavior.ColorScheme, null, false, () => { });
            listItem.LoadView();
            return listItem.View.FittingSize;
        }
        else
        {
            var size = _behavior.GetImage(item, ImageSize).Size;
            var max = (double) Math.Max(size.Width, size.Height);
            return new CGSize(size.Width * ImageSize.Width / max, size.Height * ImageSize.Width / max);
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
        try
        {
            if (proposedDropOperation == NSCollectionViewDropOperation.On)
            {
                // If we're dropping on top of an image, instead make it before/after the same image based on which
                // edge we're closer to.
                var itemFrame = _view.GetFrameForItem(proposedDropIndexPath.Item);
                var dragX = draggingInfo.DraggingLocation.X;
                if (dragX - itemFrame.Left > itemFrame.Right - dragX)
                {
                    proposedDropIndexPath =
                        NSIndexPath.Create(proposedDropIndexPath.Section, proposedDropIndexPath.Item + 1);
                }
                proposedDropOperation = NSCollectionViewDropOperation.Before;
            }
            if (_behavior.AllowDragDrop && GetCustomData(draggingInfo, out var data))
            {
                return _behavior.GetCustomDragEffect(data).ToNS();
            }
            if (_behavior.AllowFileDrop && draggingInfo.DraggingPasteboard.CanReadItemWithDataConformingToTypes(
                    new string[]
                        { NSPasteboard.NSPasteboardTypeFileUrl }))
            {
                return NSDragOperation.Copy;
            }
        }
        catch (Exception ex)
        {
            Log.ErrorException("Error validating drop", ex);
        }
        return NSDragOperation.None;
    }

    public override bool CanDragItems(NSCollectionView collectionView, NSIndexSet indexes, NSEvent evt)
    {
        return _behavior.AllowDragDrop && indexes.Count > 0;
    }

    public override INSPasteboardWriting? GetPasteboardWriter(NSCollectionView collectionView, NSIndexPath indexPath)
    {
        try
        {
            var item = new NSPasteboardItem();
            var binaryData = _behavior.SerializeCustomDragData(new[] { _dataSource.Items[(int) indexPath.Item] });
            item.SetDataForType(NSData.FromArray(binaryData), _behavior.CustomDragDataType);
            return item;
        }
        catch (Exception ex)
        {
            Log.ErrorException("Error serializing data for drag", ex);
        }
        return null;
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