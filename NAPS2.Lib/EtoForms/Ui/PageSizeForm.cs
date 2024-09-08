using System.Collections.Immutable;
using System.Globalization;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Ui;

public class PageSizeForm : EtoDialogBase
{
    private readonly ComboBox _name = new();
    private readonly NumericMaskedTextBox<decimal> _width = new();
    private readonly NumericMaskedTextBox<decimal> _height = new();
    private readonly EnumDropDownWidget<LocalizedPageSizeUnit> _unit = new();

    private PageDimensions? _initialDimens;

    public PageSizeForm(Naps2Config config, IIconProvider iconProvider)
        : base(config)
    {
        DeletePageSizeCommand = new ActionCommand(DeletePageSize)
        {
            Image = iconProvider.GetIcon("cross_small")
        };
        _name.SelectedValueChanged += Name_SelectionChange;
        _name.TextChanged += Name_TextChanged;
    }

    public ActionCommand DeletePageSizeCommand { get; set; }

    private void DeletePageSize()
    {
        if (MessageBox.Show(string.Format(MiscResources.ConfirmDelete, _name.Text), MiscResources.Delete,
                MessageBoxButtons.OKCancel, MessageBoxType.Question, MessageBoxDefaultButton.OK) == DialogResult.Ok)
        {
            var presets = Config.Get(c => c.CustomPageSizePresets);
            presets = presets.RemoveAll(x => x.Name == _name.Text);
            Config.User.Set(c => c.CustomPageSizePresets, presets);

            UpdateDropdown();
            _name.Text = "";
            UpdateDimens(_initialDimens!);
        }
    }

    public string? PageSizeName { get; set; }

    public PageDimensions? PageSizeDimens { get; set; }

    public bool Result { get; private set; }

    private ImmutableList<NamedPageSize> Presets => Config.Get(c => c.CustomPageSizePresets);

    protected override void BuildLayout()
    {
        _initialDimens = PageSizeDimens ?? ScanPageSize.Letter.PageDimensions();
        UpdateDropdown();
        _name.Text = PageSizeName ?? "";
        DeletePageSizeCommand.Enabled = Presets.Any(x => x.Name == _name.Text);
        UpdateDimens(_initialDimens!);

        Title = UiStrings.PageSizeFormTitle;

        FormStateController.RestoreFormState = false;
        FormStateController.FixedHeightLayout = true;
        base.BuildLayout();

        LayoutController.Content = L.Column(
            C.Label(UiStrings.NameOptional),
            L.Row(
                _name.Scale().NaturalWidth(250).AlignCenter(),
                C.Button(DeletePageSizeCommand, ButtonImagePosition.Overlay).AlignCenter().Width(30)
            ),
            C.Spacer(),
            C.Label(UiStrings.Dimensions),
            L.Row(
                _width.Scale().AlignCenter(),
                C.Label("X").AlignCenter(),
                _height.Scale().AlignCenter(),
                _unit.AsControl().Scale().AlignCenter()
            ),
            L.Row(
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, Submit),
                    C.CancelButton(this))
            )
        );
    }

    private void UpdateDropdown()
    {
        _name.Items.Clear();
        foreach (var preset in Presets.OrderBy(x => x.Name))
        {
            _name.Items.Add(preset.Name);
        }
    }

    private void UpdateDimens(PageDimensions dimens)
    {
        _width.Text = dimens.Width.ToString(CultureInfo.CurrentCulture);
        _height.Text = dimens.Height.ToString(CultureInfo.CurrentCulture);
        _unit.SelectedItem = dimens.Unit;
    }

    private void Name_SelectionChange(object? sender, EventArgs e)
    {
        var dimens = Presets.Where(x => x.Name == _name.SelectedKey).Select(x => x.Dimens)
            .FirstOrDefault();
        if (dimens != null)
        {
            UpdateDimens(dimens);
        }
    }

    private void Name_TextChanged(object? sender, EventArgs e)
    {
        DeletePageSizeCommand.Enabled = Presets.Any(x => x.Name == _name.Text);
    }

    private void Submit()
    {
        const NumberStyles numberStyle = NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands |
                                         NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingSign;
        if (!decimal.TryParse(_width.Text, numberStyle, CultureInfo.CurrentCulture, out decimal width))
        {
            _width.Focus();
            return;
        }
        if (!decimal.TryParse(_height.Text, numberStyle, CultureInfo.CurrentCulture, out decimal height))
        {
            _height.Focus();
            return;
        }
        PageSizeName = null;
        PageSizeDimens = new PageDimensions
        {
            Width = width,
            Height = height,
            Unit = _unit.SelectedItem
        };
        if (!string.IsNullOrWhiteSpace(_name.Text))
        {
            PageSizeName = _name.Text;
            var presets = Presets.RemoveAll(x => x.Name == PageSizeName);
            presets = presets.Add(new NamedPageSize
            {
                Name = PageSizeName,
                Dimens = PageSizeDimens
            });
            Config.User.Set(c => c.CustomPageSizePresets, presets);
        }
        Result = true;
    }
}