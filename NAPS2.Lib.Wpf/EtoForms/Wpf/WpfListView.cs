using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Eto.Wpf;
using NAPS2.EtoForms.Widgets;
using ContextMenu = Eto.Forms.ContextMenu;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;

namespace NAPS2.EtoForms.Wpf;

/// <summary>
/// A thumbnail list based on a WPF ListBox with a WrapPanel.
/// </summary>
public class WpfListView<T> : IListView<T> where T : notnull
{
    private readonly ListViewBehavior<T> _behavior;
    private readonly ListBox _listBox;

    private ListSelection<T> _selection = ListSelection.Empty<T>();
    private bool _refreshing;

    private Point? _dragStartPoint;
    private WpfListViewItem<T>? _deferredSelectItem;
    private WpfListViewItem<T>? _lastFocusedItem;
    private InsertionAdorner? _insertionAdorner;

    // For click-and-drag selection, relative to the items panel (so it moves with scrolling)
    private Point? _marqueeStart;
    private HashSet<WpfListViewItem<T>> _marqueeBaseSelection = [];
    private MarqueeAdorner? _marqueeAdorner;

    public WpfListView(ListViewBehavior<T> behavior)
    {
        _behavior = behavior;
        _listBox = new ListBox
        {
            SelectionMode = behavior.Checkboxes ? SelectionMode.Multiple
                : behavior.MultiSelect ? SelectionMode.Extended
                : SelectionMode.Single,
            BorderThickness = new Thickness(0),
            AllowDrop = behavior.AllowDragDrop || behavior.AllowFileDrop
        };
        _listBox.ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(
            behavior.Checkboxes ? typeof(UniformWrapPanel) : typeof(WrapPanel)));
        ScrollViewer.SetHorizontalScrollBarVisibility(_listBox, ScrollBarVisibility.Disabled);
        _listBox.FocusVisualStyle = null;
        _listBox.SelectionChanged += OnSelectionChanged;
        _listBox.KeyDown += OnKeyDown;
        _listBox.PreviewGotKeyboardFocus += (_, e) =>
        {
            if (e.NewFocus is WpfListViewItem<T> item) _lastFocusedItem = item;
        };
        _listBox.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        _listBox.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
        _listBox.PreviewMouseMove += OnPreviewMouseMove;
        _listBox.LostMouseCapture += (_, _) => EndMarquee();
        _listBox.DragEnter += OnDragOver;
        _listBox.DragOver += OnDragOver;
        _listBox.DragLeave += (_, _) => ClearInsertionMark();
        _listBox.Drop += OnDrop;
        Control = Eto.Forms.WpfHelpers.ToEto(_listBox);

        UpdateColors();
        _listBox.Loaded += (_, _) => _behavior.ColorScheme.ColorSchemeChanged += OnColorSchemeChanged;
        _listBox.Unloaded += (_, _) => _behavior.ColorScheme.ColorSchemeChanged -= OnColorSchemeChanged;
    }

    private void OnColorSchemeChanged(object? sender, EventArgs e) => _listBox.Dispatcher.Invoke(UpdateColors);

    private void UpdateColors()
    {
        _listBox.Background = _behavior.ColorScheme.BackgroundColor.ToWpfBrush();
        _listBox.BorderBrush = _behavior.ColorScheme.SeparatorColor.ToWpfBrush();
        foreach (var listBoxItem in Items)
        {
            listBoxItem.UpdateVisuals();
        }
    }

    public Eto.Forms.Control Control { get; }

    public ListBox NativeControl => _listBox;

    public ContextMenu? ContextMenu
    {
        get;
        set
        {
            field = value;
            _listBox.ContextMenu = (System.Windows.Controls.ContextMenu?) value?.ControlObject;
        }
    }

    public Eto.Drawing.Size ImageSize
    {
        get;
        set
        {
            field = value;
            foreach (var listBoxItem in Items)
            {
                listBoxItem.UpdateSize(value);
            }
        }
    } = new(48, 48);

    public Thickness ItemMargin
    {
        get;
        set
        {
            field = value;
            foreach (var listBoxItem in Items)
            {
                listBoxItem.Margin = value;
            }
        }
    } = new(4);

    public event EventHandler? SelectionChanged;

    public event EventHandler? ItemClicked;

    public event EventHandler<DropEventArgs>? Drop;

    private IEnumerable<WpfListViewItem<T>> Items => _listBox.Items.Cast<WpfListViewItem<T>>();

    public void SetItems(IEnumerable<T> items)
    {
        if (_refreshing) throw new InvalidOperationException();
        _refreshing = true;
        bool hadFocus = _listBox.IsKeyboardFocusWithin;
        _listBox.Items.Clear();
        foreach (var item in items)
        {
            _listBox.Items.Add(CreateItem(item));
        }
        UpdatePageNumbers();
        SetSelectedItems(hadFocus);
        _refreshing = false;
    }

    public void Update()
    {
        foreach (var listBoxItem in Items)
        {
            listBoxItem.Update();
        }
        UpdatePageNumbers();
    }

    public void RegenerateImages()
    {
        if (_refreshing) return;
        foreach (var listBoxItem in Items)
        {
            SetImage(listBoxItem);
        }
    }

    public void ApplyDiffs(ListViewDiffs<T> diffs)
    {
        if (_refreshing) throw new InvalidOperationException();
        _refreshing = true;
        bool hadFocus = _listBox.IsKeyboardFocusWithin;
        var originalItemsList = Items.Select(x => x.Value).ToList();

        foreach (var append in diffs.AppendOperations)
        {
            _listBox.Items.Add(CreateItem(append.Item));
        }
        foreach (var replace in diffs.ReplaceOperations)
        {
            var listBoxItem = (WpfListViewItem<T>) _listBox.Items[replace.Index]!;
            listBoxItem.Value = replace.Item;
            SetImage(listBoxItem);
        }
        foreach (var trim in diffs.TrimOperations)
        {
            for (int i = 0; i < trim.Count; i++)
            {
                _listBox.Items.RemoveAt(_listBox.Items.Count - 1);
            }
        }
        UpdatePageNumbers();
        SetSelectedItems(hadFocus);

        var newItemsList = Items.Select(x => x.Value).ToList();
        if (new HashSet<T>(originalItemsList).SetEquals(newItemsList) && !originalItemsList.SequenceEqual(newItemsList))
        {
            // Items were reordered, so keep the selection in view
            var selected = Items.Where(x => x.IsSelected).ToList();
            (selected.LastOrDefault() ?? Items.FirstOrDefault())?.BringIntoView();
            selected.FirstOrDefault()?.BringIntoView();
        }
        _refreshing = false;
    }

    public ListSelection<T> Selection
    {
        get => _selection;
        set
        {
            if (_selection == value) return;
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

    private void SetSelectedItems(bool hadFocus = false)
    {
        foreach (var listBoxItem in Items)
        {
            listBoxItem.IsSelected = _selection.Contains(listBoxItem.Value);
        }
        if (hadFocus || _listBox.IsKeyboardFocusWithin)
        {
            FocusSelection();
        }
    }

    /// <summary>
    /// After the selection is changed in code (e.g. items moved or deleted), moves focus to a selected item.
    /// </summary>
    private void FocusSelection()
    {
        var focused = Keyboard.FocusedElement as WpfListViewItem<T>;
        bool focusOnItem = focused != null && _listBox.Items.Contains(focused);
        var selected = Items.FirstOrDefault(x => x.IsSelected);
        if (selected == null)
        {
            if (!focusOnItem) FocusList();
        }
        else if (!focusOnItem || !focused!.IsSelected)
        {
            selected.Focus();
        }
    }

    /// <summary>
    /// Moves focus into the list without changing the selection, preferring the last focused item.
    /// </summary>
    private void FocusList()
    {
        var item = _lastFocusedItem != null && _listBox.Items.Contains(_lastFocusedItem)
            ? _lastFocusedItem
            : Items.FirstOrDefault();
        ((UIElement?) item ?? _listBox).Focus();
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_refreshing) return;
        _refreshing = true;
        Selection = ListSelection.From(Items.Where(x => x.IsSelected).Select(x => x.Value));
        _refreshing = false;
    }

    private WpfListViewItem<T> CreateItem(T value)
    {
        var listBoxItem = new WpfListViewItem<T>(_behavior, value) { Margin = ItemMargin };
        listBoxItem.MouseDoubleClick += (_, _) => ItemClicked?.Invoke(this, EventArgs.Empty);
        SetImage(listBoxItem);
        return listBoxItem;
    }

    private void SetImage(WpfListViewItem<T> listBoxItem)
    {
        if (_behavior.Checkboxes) return;
        using var etoImage = _behavior.GetImage(this, listBoxItem.Value);
        listBoxItem.SetImage(etoImage.ToWpf(), ImageSize);
    }

    private void UpdatePageNumbers()
    {
        if (!_behavior.ShowPageNumbers || _behavior.ShowLabels) return;
        int count = _listBox.Items.Count;
        int i = 0;
        foreach (var listBoxItem in Items)
        {
            listBoxItem.SetPageNumber(++i, count);
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && _listBox.SelectedItems.Count > 0)
        {
            ItemClicked?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = null;
        _deferredSelectItem = null;
        if (ItemsControl.ContainerFromElement(_listBox, (DependencyObject) e.OriginalSource) is not
            WpfListViewItem<T> listBoxItem)
        {
            OnEmptySpaceClicked(e);
            return;
        }
        if (!_behavior.AllowDragDrop || e.ClickCount > 1) return;
        _dragStartPoint = e.GetPosition(_listBox);
        if (listBoxItem.IsSelected && Keyboard.Modifiers == ModifierKeys.None && _listBox.SelectedItems.Count > 1)
        {
            // Clicking an item in a multi-selection would normally reset the selection to just that item, which
            // makes it impossible to drag multiple items. Defer that until mouse up (if no drag happened).
            _deferredSelectItem = listBoxItem;
            listBoxItem.Focus();
            e.Handled = true;
        }
    }

    private void OnEmptySpaceClicked(MouseButtonEventArgs e)
    {
        // Ignore clicks on the scroll bar
        for (var element = e.OriginalSource as DependencyObject;
             element != null && element != _listBox;
             element = VisualTreeHelper.GetParent(element))
        {
            if (element is ScrollBar) return;
        }
        if (Keyboard.Modifiers == ModifierKeys.None && !_behavior.Checkboxes)
        {
            _listBox.UnselectAll();
        }
        if (!_listBox.IsKeyboardFocusWithin || Keyboard.FocusedElement == _listBox)
        {
            FocusList();
        }
        if (_behavior.MultiSelect && !_behavior.Checkboxes)
        {
            _marqueeStart = e.GetPosition(ItemsPanel);
            _marqueeBaseSelection = Items.Where(x => x.IsSelected).ToHashSet();
            _listBox.CaptureMouse();
        }
        e.Handled = true;
    }

    private Panel ItemsPanel => _listBox.FindDescendant<WrapPanel>()!;

    private static bool IsDrag(Vector delta) =>
        Math.Abs(delta.X) >= SystemParameters.MinimumHorizontalDragDistance ||
        Math.Abs(delta.Y) >= SystemParameters.MinimumVerticalDragDistance;

    private void UpdateMarquee(Point start, MouseEventArgs e)
    {
        AutoScroll(e.GetPosition(_listBox));
        var panel = ItemsPanel;
        var end = e.GetPosition(panel);
        if (_marqueeAdorner == null)
        {
            if (!IsDrag(end - start)) return;
            // The panel's adorner layer is inside the scroll viewer, so the marquee is clipped to the visible area
            _marqueeAdorner = new MarqueeAdorner(panel);
            AdornerLayer.GetAdornerLayer(panel)?.Add(_marqueeAdorner);
        }
        var rect = new Rect(start, end);
        _marqueeAdorner.Rect = rect;

        _refreshing = true;
        foreach (var listBoxItem in Items)
        {
            var bounds = listBoxItem.TransformToAncestor(panel).TransformBounds(new Rect(listBoxItem.RenderSize));
            listBoxItem.IsSelected = _marqueeBaseSelection.Contains(listBoxItem) || rect.IntersectsWith(bounds);
        }
        // Keep _refreshing set so the selection change doesn't move the focus (and scroll) while dragging
        Selection = ListSelection.From(Items.Where(x => x.IsSelected).Select(x => x.Value));
        _refreshing = false;
    }

    private void EndMarquee()
    {
        if (_marqueeStart == null) return;
        _marqueeStart = null;
        _marqueeBaseSelection = [];
        if (_marqueeAdorner != null)
        {
            AdornerLayer.GetAdornerLayer(_marqueeAdorner.AdornedElement)?.Remove(_marqueeAdorner);
            _marqueeAdorner = null;
        }
        _listBox.ReleaseMouseCapture();
    }

    private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        EndMarquee();
        if (_deferredSelectItem != null)
        {
            _listBox.SelectedItems.Clear();
            _deferredSelectItem.IsSelected = true;
        }
        _dragStartPoint = null;
        _deferredSelectItem = null;
    }

    private void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        // The marquee has the mouse captured until the button is released
        if (_marqueeStart is { } marqueeStart)
        {
            UpdateMarquee(marqueeStart, e);
            return;
        }
        if (_dragStartPoint is not { } start || e.LeftButton != MouseButtonState.Pressed) return;
        if (!IsDrag(e.GetPosition(_listBox) - start)) return;
        _dragStartPoint = null;
        _deferredSelectItem = null;
        if (Selection.Count == 0) return;
        var dataObject = new DataObject();
        dataObject.SetData(_behavior.CustomDragDataType, _behavior.SerializeCustomDragData(Selection.ToArray()));
        DragDrop.DoDragDrop(_listBox, dataObject, DragDropEffects.Move | DragDropEffects.Copy);
        ClearInsertionMark();
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = GetDragEffect(e.Data);
        e.Handled = true;
        if (_behavior.ScrollOnDrag)
        {
            AutoScroll(e.GetPosition(_listBox));
        }
        if (e.Effects == DragDropEffects.Move && _listBox.Items.Count > 0)
        {
            var index = GetDragIndex(e.GetPosition(_listBox));
            if (index >= 0)
            {
                bool after = index == _listBox.Items.Count;
                ShowInsertionMark((WpfListViewItem<T>) _listBox.Items[after ? index - 1 : index]!, after);
            }
        }
        else
        {
            ClearInsertionMark();
        }
    }

    private DragDropEffects GetDragEffect(IDataObject data)
    {
        if (_behavior.AllowDragDrop && data.GetDataPresent(_behavior.CustomDragDataType) &&
            data.GetData(_behavior.CustomDragDataType) is byte[] bytes)
        {
            return _behavior.GetCustomDragEffect(bytes) switch
            {
                Eto.Forms.DragEffects.Move => DragDropEffects.Move,
                Eto.Forms.DragEffects.Copy => DragDropEffects.Copy,
                _ => DragDropEffects.None
            };
        }
        if (_behavior.AllowFileDrop && data.GetDataPresent(DataFormats.FileDrop))
        {
            return DragDropEffects.Copy;
        }
        return DragDropEffects.None;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        ClearInsertionMark();
        var index = GetDragIndex(e.GetPosition(_listBox));
        if (index == -1) return;
        if (_behavior.AllowDragDrop && e.Data.GetData(_behavior.CustomDragDataType) is byte[] bytes)
        {
            Drop?.Invoke(this, new DropEventArgs(index, bytes));
        }
        else if (_behavior.AllowFileDrop && e.Data.GetData(DataFormats.FileDrop) is string[] filePaths)
        {
            Drop?.Invoke(this, new DropEventArgs(index, filePaths));
        }
    }

    private void AutoScroll(Point position)
    {
        const double edge = 30;
        if (_listBox.FindDescendant<ScrollViewer>() is not { } scrollViewer) return;
        if (position.Y < edge) scrollViewer.LineUp();
        else if (position.Y > _listBox.ActualHeight - edge) scrollViewer.LineDown();
    }

    private int GetDragIndex(Point position)
    {
        if (_listBox.Items.Count == 0) return 0;
        var bounds = Items
            .Select((item, index) => (index, rect: item.TransformToAncestor(_listBox)
                .TransformBounds(new Rect(0, 0, item.ActualWidth, item.ActualHeight))))
            .ToList();
        double y = Math.Clamp(position.Y, bounds.Min(x => x.rect.Top), bounds.Max(x => x.rect.Bottom));
        var row = bounds.Where(x => x.rect.Top <= y && x.rect.Bottom >= y).OrderBy(x => x.rect.Left).ToList();
        if (row.Count == 0) return -1;
        var target = row.FirstOrDefault(x => x.rect.Right >= position.X);
        if (target.rect == default) target = row.Last();
        return position.X > target.rect.Left + target.rect.Width / 2 ? target.index + 1 : target.index;
    }

    private void ShowInsertionMark(WpfListViewItem<T> listBoxItem, bool after)
    {
        if (_insertionAdorner?.AdornedElement == listBoxItem && _insertionAdorner.After == after) return;
        ClearInsertionMark();
        var layer = AdornerLayer.GetAdornerLayer(listBoxItem);
        if (layer == null) return;
        _insertionAdorner = new InsertionAdorner(listBoxItem, after);
        layer.Add(_insertionAdorner);
    }

    private void ClearInsertionMark()
    {
        if (_insertionAdorner == null) return;
        AdornerLayer.GetAdornerLayer(_insertionAdorner.AdornedElement)?.Remove(_insertionAdorner);
        _insertionAdorner = null;
    }

    /// <summary>
    /// A WrapPanel where every item is as wide as the widest one, so the items line up in columns.
    /// </summary>
    private class UniformWrapPanel : WrapPanel
    {
        protected override Size MeasureOverride(Size constraint)
        {
            double itemWidth = 0;
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                itemWidth = Math.Max(itemWidth, child.DesiredSize.Width);
            }
            ItemWidth = itemWidth;
            return base.MeasureOverride(constraint);
        }
    }

    private class InsertionAdorner(UIElement adornedElement, bool after) : Adorner(adornedElement)
    {
        public bool After { get; } = after;

        protected override void OnRender(DrawingContext drawingContext)
        {
            var size = AdornedElement.RenderSize;
            var brush = (Brush) (TryFindResource(SystemColors.HighlightBrushKey) ?? Brushes.DodgerBlue);
            double x = After ? size.Width + 2 : -2;
            drawingContext.DrawLine(new Pen(brush, 3), new Point(x, 0), new Point(x, size.Height));
        }
    }

    // Click and drag selection rectangle
    private class MarqueeAdorner : Adorner
    {
        public MarqueeAdorner(UIElement adornedElement) : base(adornedElement)
        {
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        }

        public Rect Rect
        {
            get;
            set
            {
                field = value;
                InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var brush = (Brush) (TryFindResource(SystemColors.HighlightBrushKey) ?? Brushes.DodgerBlue);
            var fill = brush.Clone();
            fill.Opacity = 0.2;
            var pen = new Pen(brush, 1);
            drawingContext.DrawRectangle(fill, pen, Rect);
        }
    }

}