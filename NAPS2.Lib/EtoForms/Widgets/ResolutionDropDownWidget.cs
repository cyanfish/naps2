using System.Globalization;
using Eto.Forms;
using NAPS2.EtoForms.Ui;

namespace NAPS2.EtoForms.Widgets;

public class ResolutionDropDownWidget : DropDownWidget<ResolutionDropDownWidget.ResolutionListItem>
{
    private readonly IFormBase _window;
    private ResolutionListItem? _customResolution;
    private ResolutionListItem? _lastResolutionItem;
    private List<int> _visiblePresets = new();

    public ResolutionDropDownWidget(IFormBase window)
    {
        _window = window;

        SelectedItemChanged += OnSelectedItemChanged;
        Format = x => x.Text;
    }

    public IEnumerable<int> VisiblePresets
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
        var resolutions = _visiblePresets.Select(size => new ResolutionListItem(size, false)).ToList();
        if (_customResolution != null && !resolutions.Contains(_customResolution))
        {
            int i = 0;
            while (i < resolutions.Count && resolutions[i].Dpi <= _customResolution.Dpi)
            {
                i++;
            }
            if (i == 0 || resolutions[i - 1].Dpi != _customResolution.Dpi)
            {
                resolutions.Insert(i, _customResolution);
            }
        }
        Items = resolutions.Append(new ResolutionListItem { Text = SettingsResources.Resolution_Custom });
    }

    private void OnSelectedItemChanged(object? sender, EventArgs e)
    {
        if (Equals(SelectedItem, new ResolutionListItem { Text = SettingsResources.Resolution_Custom }))
        {
            if (_lastResolutionItem == null)
            {
                Log.Error("Expected last resolution to be set");
                return;
            }
            // "Custom..." selected
            var form = _window.FormFactory.Create<ResolutionForm>();
            form.Dpi = _window.Config.Get(c => c.LastCustomResolution);
            form.ShowModal();
            if (form.Result)
            {
                _window.Config.User.Set(c => c.LastCustomResolution, form.Dpi);
                _customResolution = new ResolutionListItem(form.Dpi, true);
                SelectedItem = _customResolution;
                RegenerateItems();
            }
            else
            {
                SelectedItem = _lastResolutionItem;
            }
        }
        _lastResolutionItem = SelectedItem;
    }

    public class ResolutionListItem : IListItem
    {
        public ResolutionListItem()
        {
        }

        public ResolutionListItem(int dpi, bool custom)
        {
            Text = string.Format(SettingsResources.DpiFormat, dpi.ToString(CultureInfo.InvariantCulture));
            Dpi = dpi;
            Custom = custom;
        }

        public string Text { get; set; } = null!;

        public string Key => Text;

        public int Dpi { get; }

        public bool Custom { get; }

        protected bool Equals(ResolutionListItem other)
        {
            return Dpi == other.Dpi && Custom == other.Custom;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ResolutionListItem) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Dpi;
                hashCode = (hashCode * 397) ^ Custom.GetHashCode();
                return hashCode;
            }
        }
    }

    public void SetDpi(int dpi)
    {
        if (VisiblePresets.Contains(dpi))
        {
            _customResolution = null;
            SelectedItem = new ResolutionListItem(dpi, false);
        }
        else
        {
            _customResolution = new ResolutionListItem(dpi, true);
            SelectedItem = _customResolution;
        }
    }
}