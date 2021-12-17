using System.Windows.Forms;

namespace NAPS2.WinForms;

public class SelectableListView<T> : ISelectable<T>
{
    private readonly ListView _listView;
    private ListSelection<T> _selection = ListSelection.Empty<T>();
    private bool _refreshing;

    public SelectableListView(ListView listView)
    {
        _listView = listView;
        listView.SelectedIndexChanged += ListViewOnSelectedIndexChanged;
    }

    public event EventHandler SelectionChanged;

    private void ListViewOnSelectedIndexChanged(object sender, EventArgs e)
    {
        if (!_refreshing)
        {
            _refreshing = true;
            Selection = ListSelection.From(_listView.SelectedItems.Cast<ListViewItem>().Select(x => (T) x.Tag));
            _refreshing = false;
        }
    }

    public ListSelection<T> Selection
    {
        get => _selection;
        set
        {
            _selection = value ?? throw new ArgumentNullException(nameof(value));
            if (!_refreshing)
            {
                _refreshing = true;
                for (int i = 0; i < _listView.Items.Count; i++)
                {
                    _listView.Items[i].Selected = _selection.Contains((T) _listView.Items[i].Tag);
                }
                _refreshing = false;
            }
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void RefreshItems(IEnumerable<T> items, Func<T, string> labelFunc, Func<T, int> imageIndexFunc)
    {
        _refreshing = true;
        _listView.Items.Clear();
        foreach (var item in items)
        {
            var listViewItem = _listView.Items.Add(labelFunc(item), imageIndexFunc(item));
            listViewItem.Tag = item;
        }
        for (int i = 0; i < _listView.Items.Count; i++)
        {
            _listView.Items[i].Selected = Selection.Contains((T) _listView.Items[i].Tag);
        }
        _refreshing = false;
    }
}