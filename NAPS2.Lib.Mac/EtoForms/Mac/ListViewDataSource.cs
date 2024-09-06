using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Mac;

public class ListViewDataSource<T> : NSCollectionViewDataSource where T : notnull
{
    private readonly IListView<T> _listView;
    private readonly ListViewBehavior<T> _behavior;
    private readonly Action<T, bool> _itemChecked;
    private readonly Action<T> _itemActivated;

    public ListViewDataSource(IListView<T> listView, ListViewBehavior<T> behavior, Action<T, bool> itemChecked,
        Action<T> itemActivated)
    {
        _listView = listView;
        _behavior = behavior;
        _itemChecked = itemChecked;
        _itemActivated = itemActivated;
    }

    public List<T> Items { get; } = [];

    public override nint GetNumberofItems(NSCollectionView collectionView, nint section)
    {
        return Items.Count;
    }

    public override NSCollectionViewItem GetItem(NSCollectionView collectionView, NSIndexPath indexPath)
    {
        var i = (int) indexPath.Item;
        var item = Items[i];
        var image = _behavior.Checkboxes ? null : _behavior.GetImage(_listView, item);
        var label = _behavior.ShowLabels ? _behavior.GetLabel(item) : null;
        return new ListViewItem(
            image, label, _behavior.Checkboxes, _behavior.ColorScheme,
            isChecked => _itemChecked(item, isChecked),
            _listView.Selection.Contains(item),
            () => _itemActivated(item));
    }
}