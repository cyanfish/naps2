using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Mac;

public class ListViewDataSource<T> : NSCollectionViewDataSource where T : notnull
{
    private readonly IListView<T> _listView;
    private readonly ListViewBehavior<T> _behavior;
    private readonly Action<T> _itemActivated;

    public ListViewDataSource(IListView<T> listView, ListViewBehavior<T> behavior, Action<T> itemActivated)
    {
        _listView = listView;
        _behavior = behavior;
        _itemActivated = itemActivated;
    }

    public List<T> Items { get; } = new();

    public override nint GetNumberofItems(NSCollectionView collectionView, nint section)
    {
        return Items.Count;
    }

    public override NSCollectionViewItem GetItem(NSCollectionView collectionView, NSIndexPath indexPath)
    {
        var i = (int) indexPath.Item;
        var item = Items[i];
        var image = _behavior.GetImage(item, _listView.ImageSize);
        var label = _behavior.ShowLabels ? _behavior.GetLabel(item) : null;
        return new ListViewItem(image, label, () => _itemActivated(item));
    }
}