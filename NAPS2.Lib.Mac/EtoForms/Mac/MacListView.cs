using Eto.Forms;

namespace NAPS2.EtoForms.Mac;

public class MacListView<T> : IListView<T> where T : notnull
{
    private readonly NSCollectionView _view = new();
    private readonly ListViewBehavior<T> _behavior;

    private ListSelection<T> _selection = ListSelection.Empty<T>();
    private bool _refreshing;

    public MacListView(ListViewBehavior<T> behavior)
    {
        _behavior = behavior;
    }

    public int ImageSize
    {
        get => 0;
        set { }
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
    }

    // TODO: Do we need this method? Clean up the name/doc at least
    public void RegenerateImages()
    {
    }

    public void ApplyDiffs(ListViewDiffs<T> diffs)
    {
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
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}