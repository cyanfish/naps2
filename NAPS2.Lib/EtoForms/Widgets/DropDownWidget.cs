using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Widgets;

public class DropDownWidget<T> where T : notnull
{
    private readonly DropDown _dropDown = new();
    private T[] _items = [];

    private bool _hasUserPreferredItem;
    private T? _userPreferredItem;
    private bool _changingItems;

    public DropDownWidget()
    {
        EtoPlatform.Current.ConfigureDropDown(_dropDown);
        _dropDown.SelectedIndexChanged += DropDown_SelectedIndexChanged;
        _dropDown.PreLoad += PreLoad;
        if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
        {
            GetClosestItem = GetClosestItemByComparing;
        }
    }

    protected virtual void PreLoad(object sender, EventArgs e)
    {
    }

    public Func<T, string> Format { get; set; } = x => x?.ToString() ?? "";

    public Func<T[], T, T>? GetClosestItem { get; set; }

    public bool Enabled
    {
        get => _dropDown.Enabled;
        set => _dropDown.Enabled = value;
    }

    public event EventHandler? SelectedItemChanged;

    public IEnumerable<T> Items
    {
        get => _items;
        set
        {
            _items = value.ToArray();
            _changingItems = true;
            var previousSelection = InternalSelectedItem;
            _dropDown.Items.Clear();
            foreach (var item in _items)
            {
                _dropDown.Items.Add(new ListItem
                {
                    Text = Format(item),
                    Tag = item
                });
            }
            InternalSelectedItem = GetPreferredSelection(previousSelection);
            _changingItems = false;
            if (!Equals(InternalSelectedItem, previousSelection))
            {
                SelectedItemChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private T? GetPreferredSelection(T? previousSelection)
    {
        if (_hasUserPreferredItem && _items.Contains(_userPreferredItem))
        {
            return _userPreferredItem;
        }
        if (_items.Contains(previousSelection))
        {
            return previousSelection;
        }
        var compareTo = _hasUserPreferredItem ? _userPreferredItem : previousSelection;
        if (_items.Length > 0 && compareTo != null && GetClosestItem != null)
        {
            return GetClosestItem(_items, compareTo);
        }
        if (_items.Length > 0)
        {
            return _items[0];
        }
        return default;
    }

    public T? SelectedItem
    {
        get => InternalSelectedItem;
        set
        {
            _hasUserPreferredItem = true;
            _userPreferredItem = value;
            _changingItems = true;
            InternalSelectedItem = value;
            _changingItems = false;
        }
    }

    private T? InternalSelectedItem
    {
        get
        {
            if (_dropDown.SelectedIndex == -1) return default;
            return (T) ((ListItem) _dropDown.Items[_dropDown.SelectedIndex]).Tag;
        }
        set => _dropDown.SelectedIndex = Array.IndexOf(_items, value);
    }

    private void DropDown_SelectedIndexChanged(object? sender, EventArgs eventArgs)
    {
        if (!_changingItems)
        {
            _hasUserPreferredItem = true;
            _userPreferredItem = InternalSelectedItem;
            SelectedItemChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private T GetClosestItemByComparing(T[] items, T x)
    {
        foreach (var y in items)
        {
            if (((IComparable<T>) x).CompareTo(y) <= 0)
            {
                return y;
            }
        }
        return items[^1];
    }

    public static implicit operator LayoutElement(DropDownWidget<T> control) => control.AsControl();

    public DropDown AsControl() => _dropDown;
}