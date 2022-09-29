using Eto.Forms;
using Eto.GtkSharp;
using Gtk;
using Orientation = Gtk.Orientation;
using GtkImageView = Gtk.Image;

namespace NAPS2.EtoForms.Gtk;

public class GtkListView<T> : IListView<T> where T : notnull
{
    private readonly ListViewBehavior<T> _behavior;

    private ListSelection<T> _selection = ListSelection.Empty<T>();
    private bool _refreshing;
    private readonly ScrolledWindow _scrolledWindow;
    private readonly FlowBox _flowBox;
    private readonly Dictionary<T, (Widget widget, int index)> _widgetMap = new();

    public GtkListView(ListViewBehavior<T> behavior)
    {
        _behavior = behavior;
        _scrolledWindow = new ScrolledWindow();
        _flowBox = new FlowBox
        {
            Orientation = Orientation.Horizontal,
            Valign = Align.Start,
            Homogeneous = false,
            ActivateOnSingleClick = false,
            MaxChildrenPerLine = uint.MaxValue,
            Margin = 8,
            ColumnSpacing = 16,
            RowSpacing = 16
        };
        _scrolledWindow.Add(_flowBox);
        // _flowBox.SelectionMode = SelectionMode.Multiple;
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

    public Control Control => _scrolledWindow.ToEto();

    public event EventHandler? Updated;

    public event EventHandler? SelectionChanged;

    public event EventHandler? ItemClicked;

    public event EventHandler<DropEventArgs>? Drop;

    public void SetItems(IEnumerable<T> items)
    {
        // TODO: Any better way to remove all?
        foreach (var widget in _flowBox.Children)
        {
            _flowBox.Remove(widget);
        }
        foreach (var item in items)
        {
            var widget = GetItemWidget(item);
            _flowBox.Add(widget);
        }
        Updated?.Invoke(this, EventArgs.Empty);
    }

    private Widget GetItemWidget(T item)
    {
        var flowBoxChild = new FlowBoxChild();
        var image = _behavior.GetImage(item, ImageSize).ToGtk();
        flowBoxChild.Add(image);
        return flowBoxChild;
    }

    // TODO: Do we need this method? Clean up the name/doc at least
    public void RegenerateImages()
    {
        foreach (var item in _widgetMap.Keys)
        {
            var (oldWidget, index) = _widgetMap[item];
            _flowBox.Remove(oldWidget);
            var newWidget = GetItemWidget(item);
            _widgetMap[item] = (newWidget, index);
        }
        Updated?.Invoke(this, EventArgs.Empty);
    }

    public void ApplyDiffs(ListViewDiffs<T> diffs)
    {
        foreach (var op in diffs.AppendOperations)
        {
            var widget = GetItemWidget(op.Item);
            var index = _widgetMap.Count;
            _flowBox.Add(widget);
            _widgetMap[op.Item] = (widget, index);
        }
        foreach (var op in diffs.ReplaceOperations)
        {
            var (oldWidget, index) = _widgetMap[op.Item];
            _flowBox.Remove(oldWidget);
            var newWidget = GetItemWidget(op.Item);
            _widgetMap[op.Item] = (newWidget, index);
        }
        foreach (var op in diffs.TrimOperations)
        {
            foreach (var item in op.DeletedItems)
            {
                var (widget, _) = _widgetMap[item];
                _flowBox.Remove(widget);
                _widgetMap.Remove(item);
            }
        }
        Updated?.Invoke(this, EventArgs.Empty);
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