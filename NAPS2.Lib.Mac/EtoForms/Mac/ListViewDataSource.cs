using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Mac;

public class ListViewDataSource<T> : NSCollectionViewDataSource where T : notnull
{
    private readonly IListView<T> _listView;
    private readonly ListViewBehavior<T> _behavior;

    public ListViewDataSource(IListView<T> listView, ListViewBehavior<T> behavior)
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
        var image = _behavior.GetImage(Items[i], _listView.ImageSize);
        var label = _behavior.ShowLabels ? _behavior.GetLabel(Items[i]) : null;
        return new ListViewItem(image, label);
    }
}