using Eto.Forms;
using NAPS2.EtoForms.Ui;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Widgets;

public class PageSizeDropDownWidget : DropDownWidget<PageSizeDropDownWidget.PageSizeListItem>
{
    private readonly IFormBase _window;
    private PageSizeListItem? _customPageSize;
    private PageSizeListItem? _lastPageSizeItem;
    private List<ScanPageSize> _visiblePresets = new();

    public PageSizeDropDownWidget(IFormBase window)
    {
        _window = window;

        SelectedItemChanged += OnSelectedItemChanged;
        Format = x => x.Text;
    }

    public IEnumerable<ScanPageSize> VisiblePresets
    {
        get => _visiblePresets;
        set
        {
            _visiblePresets = value.ToList();
            RegenerateItems();
        }
    }

    private void RegenerateItems()
    {
        var presetSizes = _visiblePresets.Select(size => new PageSizeListItem(size)).ToList();
        var customSizes = _window.Config.Get(c => c.CustomPageSizePresets)
            .OrderBy(x => x.Name)
            .Select(preset => new PageSizeListItem
            {
                Type = ScanPageSize.Custom,
                Text = string.Format(MiscResources.NamedPageSizeFormat, preset.Name, preset.Dimens.Width,
                    preset.Dimens.Height, preset.Dimens.Unit.Description()),
                CustomName = preset.Name,
                CustomDimens = preset.Dimens
            }).ToList();

        if (_customPageSize != null && !customSizes.Contains(_customPageSize))
        {
            customSizes.Add(_customPageSize);
        }
        Items = presetSizes.Concat(customSizes).Append(new PageSizeListItem(ScanPageSize.Custom));
    }

    private void OnSelectedItemChanged(object? sender, EventArgs e)
    {
        if (Equals(SelectedItem, new PageSizeListItem(ScanPageSize.Custom)))
        {
            if (_lastPageSizeItem == null)
            {
                Log.Error("Expected last page size to be set");
                return;
            }
            // "Custom..." selected
            var form = _window.FormFactory.Create<PageSizeForm>();
            form.PageSizeDimens = _lastPageSizeItem.Type == ScanPageSize.Custom
                ? _lastPageSizeItem.CustomDimens
                : _lastPageSizeItem.Type.PageDimensions();
            form.ShowModal();
            if (form.Result)
            {
                _customPageSize = GetCustomPageSize(form.PageSizeName!, form.PageSizeDimens!);
                SelectedItem = _customPageSize;
                RegenerateItems();
            }
            else
            {
                SelectedItem = _lastPageSizeItem;
            }
        }
        _lastPageSizeItem = SelectedItem;
    }

    private PageSizeListItem GetCustomPageSize(string? name, PageDimensions dimens)
    {
        return new PageSizeListItem
        {
            Type = ScanPageSize.Custom,
            Text = string.IsNullOrEmpty(name)
                ? string.Format(MiscResources.CustomPageSizeFormat, dimens.Width, dimens.Height,
                    dimens.Unit.Description())
                : string.Format(MiscResources.NamedPageSizeFormat, name, dimens.Width, dimens.Height,
                    dimens.Unit.Description()),
            CustomName = name,
            CustomDimens = dimens
        };
    }

    public class PageSizeListItem : IListItem
    {
        public PageSizeListItem()
        {
        }

        public PageSizeListItem(ScanPageSize size)
        {
            Type = size;
            Text = size.Description();
        }

        public string Text { get; set; } = null!;

        public string Key => Text;

        public ScanPageSize Type { get; set; }

        public string? CustomName { get; set; }

        public PageDimensions? CustomDimens { get; set; }

        protected bool Equals(PageSizeListItem other)
        {
            return Type == other.Type && CustomName == other.CustomName && Equals(CustomDimens, other.CustomDimens);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PageSizeListItem) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Type;
                hashCode = (hashCode * 397) ^ (CustomName != null ? CustomName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CustomDimens != null ? CustomDimens.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public void SetCustom(string? customPageSizeName, PageDimensions customPageSize)
    {
        _customPageSize = GetCustomPageSize(customPageSizeName, customPageSize);
        SelectedItem = _customPageSize;
    }

    public void SetPreset(ScanPageSize pageSize)
    {
        SelectedItem = new PageSizeListItem(pageSize);
    }
}