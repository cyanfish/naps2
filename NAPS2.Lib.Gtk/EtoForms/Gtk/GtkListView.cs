using Eto.Forms;
using Eto.GtkSharp;
using Gtk;
using Label = Gtk.Label;
using Orientation = Gtk.Orientation;

namespace NAPS2.EtoForms.Gtk;

public class GtkListView<T> : IListView<T> where T : notnull
{
    private readonly ListViewBehavior<T> _behavior;

    private ListSelection<T> _selection = ListSelection.Empty<T>();
    private bool _refreshing;
    private readonly ScrolledWindow _scrolledWindow;
    private readonly FlowBox _flowBox;
    private List<Entry> _entries = new();

    public GtkListView(ListViewBehavior<T> behavior)
    {
        _behavior = behavior;
        _scrolledWindow = new ScrolledWindow();
        _flowBox = new FlowBox
        {
            Orientation = Orientation.Horizontal,
            Valign = Align.Start,
            Homogeneous = _behavior.Checkboxes,
            ActivateOnSingleClick = false,
            MaxChildrenPerLine = uint.MaxValue,
            Margin = 8,
            ColumnSpacing = _behavior.Checkboxes ? 0 : 16u,
            RowSpacing = _behavior.Checkboxes ? 0 : 16u,
            SelectionMode = _behavior.Checkboxes
                ? SelectionMode.None
                : _behavior.MultiSelect
                    ? SelectionMode.Multiple
                    : SelectionMode.Single
        };
        if (!_behavior.Checkboxes)
        {
            _flowBox.SelectedChildrenChanged += FlowBoxSelectionChanged;
        }
        _scrolledWindow.Add(_flowBox);
        _scrolledWindow.StyleContext.AddClass("listview");
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

    public ScrolledWindow NativeControl => _scrolledWindow;

    public Control Control => _scrolledWindow.ToEto();

    public event EventHandler? Updated;

    public event EventHandler? SelectionChanged;

    public event EventHandler? ItemClicked;

    public event EventHandler<DropEventArgs>? Drop;

    public void SetItems(IEnumerable<T> items)
    {
        if (_refreshing)
        {
            throw new InvalidOperationException();
        }
        _refreshing = true;
        // TODO: Any better way to remove all?
        foreach (var widget in _flowBox.Children)
        {
            _flowBox.Remove(widget);
        }
        _entries.Clear();
        foreach (var item in items)
        {
            var widget = GetItemWidget(item);
            _flowBox.Add(widget);
            _entries.Add(new Entry
            {
                Index = _entries.Count,
                Item = item,
                Widget = widget
            });
        }
        _flowBox.ShowAll();
        SetSelectedItems();
        _refreshing = false;
        Updated?.Invoke(this, EventArgs.Empty);
    }

    private Widget GetItemWidget(T item)
    {
        var flowBoxChild = new FlowBoxChild();
        if (_behavior.Checkboxes)
        {
            var check = new CheckButton(_behavior.GetLabel(item));
            flowBoxChild.Add(check);
            flowBoxChild.CanFocus = false;
            check.Toggled += FlowBoxSelectionChanged;
        }
        else
        {
            var image = _behavior.GetImage(item, ImageSize).ToGtk();
            // TODO: Is there a better way to prevent the image from expanding in both dimensions?
            var hframe = new Box(Orientation.Horizontal, 0);
            hframe.Halign = Align.Center;
            hframe.Add(image);
            var vframe = new Box(Orientation.Vertical, 0);
            vframe.Valign = Align.Center;
            vframe.Add(hframe);
            if (_behavior.ShowLabels)
            {
                var label = new Label
                {
                    Text = _behavior.GetLabel(item),
                    LineWrap = true,
                    Justify = Justification.Center,
                    MaxWidthChars = 15
                };
                vframe.Add(label);
            }
            flowBoxChild.Add(vframe);
        }
        flowBoxChild.StyleContext.AddClass("listview-item");
        return flowBoxChild;
    }

    // TODO: Do we need this method? Clean up the name/doc at least
    public void RegenerateImages()
    {
        if (_refreshing)
        {
            throw new InvalidOperationException();
        }
        _refreshing = true;
        foreach (var entry in _entries)
        {
            _flowBox.Remove(entry.Widget);
            var newWidget = GetItemWidget(entry.Item);
            entry.Widget = newWidget;
        }
        _flowBox.ShowAll();
        SetSelectedItems();
        _refreshing = false;
        Updated?.Invoke(this, EventArgs.Empty);
    }

    public void ApplyDiffs(ListViewDiffs<T> diffs)
    {
        if (_refreshing)
        {
            throw new InvalidOperationException();
        }
        _refreshing = true;
        foreach (var op in diffs.AppendOperations)
        {
            var widget = GetItemWidget(op.Item);
            var index = _entries.Count;
            _flowBox.Add(widget);
            _entries.Add(new Entry
            {
                Item = op.Item,
                Widget = widget,
                Index = index
            });
        }
        foreach (var op in diffs.ReplaceOperations)
        {
            var entry = _entries[op.Index];
            _flowBox.Remove(entry.Widget);
            var newWidget = GetItemWidget(op.Item);
            _flowBox.Insert(newWidget, entry.Index);
            entry.Widget = newWidget;
            entry.Item = op.Item;
        }
        foreach (var op in diffs.TrimOperations)
        {
            foreach (var entry in _entries.Skip(_entries.Count - op.Count).ToList())
            {
                _flowBox.Remove(entry.Widget);
            }
            _entries = _entries.Take(_entries.Count - op.Count).ToList();
        }
        _flowBox.ShowAll();
        SetSelectedItems();
        _refreshing = false;
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
            if (!_refreshing)
            {
                _refreshing = true;
                SetSelectedItems();
                _refreshing = false;
            }
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void SetSelectedItems()
    {
        if (_behavior.Checkboxes)
        {
            foreach (var widget in _flowBox.Children)
            {
                GetCheckButton(widget).Active = false;
            }
        }
        else
        {
            _flowBox.UnselectAll();
        }
        var byItem = ByItem();
        foreach (var item in _selection)
        {
            if (byItem.Get(item) is not { } entry) continue;
            if (_behavior.Checkboxes)
            {
                GetCheckButton(entry.Widget).Active = true;
            }
            else
            {
                _flowBox.SelectChild((FlowBoxChild) entry.Widget);
            }
        }
    }

    private Dictionary<T, Entry> ByItem() => _entries.ToDictionary(x => x.Item);
    private Dictionary<Widget, Entry> ByWidget() => _entries.ToDictionary(x => x.Widget);

    private void FlowBoxSelectionChanged(object? sender, EventArgs e)
    {
        if (!_refreshing)
        {
            _refreshing = true;
            var byWidget = ByWidget();
            if (_behavior.Checkboxes)
            {
                var checkButtons = _flowBox.Children.Select(GetCheckButton).ToList();
                Selection = ListSelection.From(
                    checkButtons
                        .Where(check => check.Active)
                        .Select(check => byWidget[check.Parent].Item));
            }
            else
            {
                Selection = ListSelection.From(_flowBox.SelectedChildren.Select(x => byWidget[x].Item));
            }
            _refreshing = false;
        }
    }

    private static CheckButton GetCheckButton(Widget widget)
    {
        return (CheckButton) ((FlowBoxChild) widget).Child;
    }

    private class Entry
    {
        public T Item { get; set; }
        public Widget Widget { get; set; }
        public int Index { get; init; }
    }
}