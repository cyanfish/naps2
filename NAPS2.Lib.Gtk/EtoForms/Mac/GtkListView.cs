using Eto.Forms;

namespace NAPS2.EtoForms.Gtk;

public class GtkListView<T> : IListView<T> where T : notnull
{
    private readonly ListViewBehavior<T> _behavior;

    private ListSelection<T> _selection = ListSelection.Empty<T>();
    private bool _refreshing;

    public GtkListView(ListViewBehavior<T> behavior)
    {
        _behavior = behavior;
    }

    public int ImageSize { get; set; }

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

    public Control Control => new Label();

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
        foreach (var op in diffs.AppendOperations)
        {
        }
        foreach (var op in diffs.ReplaceOperations)
        {
        }
        foreach (var op in diffs.TrimOperations)
        {
        }
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
            _refreshing = false;
        }
    }
}