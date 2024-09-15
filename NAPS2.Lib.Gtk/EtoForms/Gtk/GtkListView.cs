using Eto.Forms;
using Eto.GtkSharp;
using Gdk;
using Gtk;
using NAPS2.EtoForms.Widgets;
using Drag = Gtk.Drag;
using Label = Gtk.Label;
using Orientation = Gtk.Orientation;

namespace NAPS2.EtoForms.Gtk;

public class GtkListView<T> : IListView<T> where T : notnull
{
    private static readonly TargetEntry[] DragTargetEntries =
    {
        // TODO: Maybe use a different mime for different list types (profiles/images)?
        // i.e. something similar to _behavior.CustomDragDataType but in mime format (maybe)
        new("application/x-naps2-items", 0, 0)
    };

    private readonly ListViewBehavior<T> _behavior;

    private ListSelection<T> _selection = ListSelection.Empty<T>();
    private bool _refreshing;
    private readonly ScrolledWindow _scrolledWindow;
    private readonly FlowBox _flowBox;
    private List<Entry> _entries = [];

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
        _flowBox.ChildActivated += OnChildActivated;
        _flowBox.ButtonPressEvent += OnButtonPress;
        var eventBox = new EventBox();
        eventBox.Child = _flowBox;
        if (_behavior.AllowDragDrop)
        {
            Drag.DestSet(eventBox, DestDefaults.All, GetDropTargetEntries(), DragAction.Copy | DragAction.Move);
            eventBox.DragDataReceived += OnDragDataReceived;
            eventBox.DragMotion += OnDragMotion;
            eventBox.DragLeave += OnDragLeave;
        }
        _scrolledWindow.Add(eventBox);
        _scrolledWindow.StyleContext.AddClass("listview");
        Control = _scrolledWindow.AsEto();
    }

    private void OnButtonPress(object o, ButtonPressEventArgs args)
    {
        if (args.Event.Button == 3)
        {
            // Right click
            ContextMenu?.Show();
        }
    }

    private void OnChildActivated(object o, ChildActivatedArgs args)
    {
        ItemClicked?.Invoke(this, EventArgs.Empty);
    }

    public Eto.Drawing.Size ImageSize { get; set; }

    public ScrolledWindow NativeControl => _scrolledWindow;

    public Control Control { get; }

    public ContextMenu? ContextMenu { get; set; }

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
            RemoveAndDisposeWidget(widget);
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
            using var image = _behavior.GetImage(this, item);
            var imageWidget = image.ToGdk().ToScaledImage(_flowBox.ScaleFactor);
            // TODO: Is there a better way to prevent the image from expanding in both dimensions?
            var hframe = new Box(Orientation.Horizontal, 0);
            hframe.Halign = Align.Center;
            hframe.Add(imageWidget);
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
            // TODO: Event box around the image instead of the frame?
            var eventBox = new EventBox();
            eventBox.Child = vframe;
            if (_behavior.AllowDragDrop || _behavior.AllowFileDrop)
            {
                eventBox.DragBegin += OnDragBegin;
                eventBox.DragDataGet += OnDragDataGet;
                Drag.SourceSet(eventBox, ModifierType.Button1Mask, DragTargetEntries, DragAction.Move);
            }
            flowBoxChild.Add(eventBox);
        }
        flowBoxChild.StyleContext.AddClass("listview-item");
        return flowBoxChild;
    }

    public void RegenerateImages()
    {
        if (_refreshing)
        {
            throw new InvalidOperationException();
        }
        if (_entries.Count == 0)
        {
            return;
        }
        _refreshing = true;
        foreach (var entry in _entries)
        {
            RemoveAndDisposeWidget(entry.Widget);
            var newWidget = GetItemWidget(entry.Item);
            entry.Widget = newWidget;
            _flowBox.Add(newWidget);
        }
        _flowBox.ShowAll();
        SetSelectedItems();
        _refreshing = false;
        Updated?.Invoke(this, EventArgs.Empty);
    }

    private void RemoveAndDisposeWidget(Widget widget)
    {
        _flowBox.Remove(widget);
        widget.Unrealize();
        widget.Dispose();
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
            RemoveAndDisposeWidget(entry.Widget);
            var newWidget = GetItemWidget(op.Item);
            _flowBox.Insert(newWidget, entry.Index);
            entry.Widget = newWidget;
            entry.Item = op.Item;
        }
        foreach (var op in diffs.TrimOperations)
        {
            foreach (var entry in _entries.Skip(_entries.Count - op.Count).ToList())
            {
                RemoveAndDisposeWidget(entry.Widget);
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

    private TargetEntry[] GetDropTargetEntries()
    {
        var list = new List<TargetEntry>();
        if (_behavior.AllowDragDrop)
        {
            list.Add(new("application/x-naps2-items", 0, 0));
        }
        if (_behavior.AllowFileDrop)
        {
            list.Add(new("text/uri-list", 0, 0));
        }
        return list.ToArray();
    }

    private void OnDragBegin(object sender, DragBeginArgs args)
    {
        // Select the item under the mouse cursor if not already.
        var dragWidget = (FlowBoxChild) ((EventBox) sender).Parent;
        var dragItem = ByWidget()[dragWidget].Item;
        if (!Selection.Contains(dragItem))
        {
            Selection = ListSelection.Of(dragItem);
        }
    }

    private void OnDragDataGet(object sender, DragDataGetArgs args)
    {
        if (Selection.Any())
        {
            // TODO: Can we set a pixbuf for the drag?
            args.SelectionData.Set(
                Atom.Intern(_behavior.CustomDragDataType, false),
                8,
                _behavior.SerializeCustomDragData(Selection.ToArray()));
        }
    }

    private void OnDragDataReceived(object sender, DragDataReceivedArgs args)
    {
        var index = GetDragIndex();
        if (args.SelectionData.DataType.Name == _behavior.CustomDragDataType && _behavior.AllowDragDrop)
        {
            Drop?.Invoke(this, new DropEventArgs(index, args.SelectionData.Data));
        }
        else if (args.SelectionData.Uris.Any() && _behavior.AllowFileDrop)
        {
            Drop?.Invoke(this, new DropEventArgs(index, args.SelectionData.Uris.Select(x => new Uri(x).LocalPath)));
        }
    }

    private void OnDragMotion(object sender, DragMotionArgs args)
    {
        if (args.Context.SelectedAction != DragAction.Move) return;
        // Show a visual indicator of the drop location
        ClearDropIndicator();
        var index = GetDragIndex();
        if (index == -1) return;
        var widgets = _flowBox.Children;
        // Show on the left (of the image to the right) if we're moving to the left, or on the right (of the image to
        // the left) if we're moving to the right.
        // This gives an accurate indication of where the image will appear especially if we're dragging across rows.
        // If the drop will have no effect (because we're dropping next to the selected image) this will show nothing.
        // TODO: This doesn't show a drop indicator if we're dropping inside a disjointed selection 
        var selectedIndices = Selection.ToSelectedIndices(_entries.Select(x => x.Item).ToList()).ToList();
        var selectionMin = selectedIndices.Min();
        var selectionMax = selectedIndices.Max() + 1;
        if (index < selectionMin)
        {
            widgets[index].StyleContext.AddClass("drop-before");
        }
        if (index > selectionMax)
        {
            widgets[index - 1].StyleContext.AddClass("drop-after");
        }
    }

    private void OnDragLeave(object sender, DragLeaveArgs args)
    {
        ClearDropIndicator();
    }

    private void ClearDropIndicator()
    {
        foreach (var widget in _flowBox.Children)
        {
            widget.StyleContext.RemoveClass("drop-before");
            widget.StyleContext.RemoveClass("drop-after");
        }
    }

    private int GetDragIndex()
    {
        if (_entries.Count == 0)
        {
            return 0;
        }
        var cp = GetMousePosRelativeToFlowbox();
        var dragToItem = _flowBox.GetChildAtPos(cp.X, cp.Y);
        if (dragToItem == null)
        {
            var items = _flowBox.Children.Cast<FlowBoxChild>().ToList();
            var minY = items.Select(x => x.Allocation.Top).Min();
            var maxY = items.Select(x => x.Allocation.Bottom).Max();
            if (cp.Y < minY)
            {
                cp.Y = minY;
            }
            if (cp.Y > maxY)
            {
                cp.Y = maxY;
            }
            var row = items.Where(x => x.Allocation.Top <= cp.Y && x.Allocation.Bottom >= cp.Y)
                .OrderBy(x => x.Allocation.X)
                .ToList();
            dragToItem = row.FirstOrDefault(x => x.Allocation.Right >= cp.X) ?? row.LastOrDefault();
        }
        if (dragToItem == null)
        {
            return -1;
        }
        int dragToIndex = dragToItem.Index;
        if (cp.X > (dragToItem.Allocation.X + dragToItem.Allocation.Width / 2))
        {
            dragToIndex++;
        }
        return dragToIndex;
    }

    private Point GetMousePosRelativeToFlowbox()
    {
        _flowBox.Window.GetOrigin(out var boxX, out var boxY);
        var mousePos = Mouse.Position;
        var cp = new Point((int) mousePos.X - boxX, (int) mousePos.Y - boxY);
        return cp;
    }

    private class Entry
    {
        public required T Item { get; set; }
        public required Widget Widget { get; set; }
        public required int Index { get; init; }
    }
}