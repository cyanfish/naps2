using Eto.Forms;
using Eto.Mac;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Mac;

public class MacSettingsNavigator : NSTableViewDelegate, ISettingsNavigator
{
    private const float IconSize = 20;

    private readonly NSTableView _tableView;
    private readonly DataSource _dataSource = new();

    public MacSettingsNavigator()
    {
        _tableView = new NSTableView
        {
            Style = NSTableViewStyle.SourceList,
            HeaderView = null,
            RowHeight = 32,
            AllowsEmptySelection = false,
            DataSource = _dataSource,
            Delegate = this
        };
        _tableView.AddColumn(new NSTableColumn());
        var scrollView = new NSScrollView
        {
            DocumentView = _tableView,
            DrawsBackground = false,
            HasVerticalScroller = true,
            AutohidesScrollers = true
        };
        Control = scrollView.ToEto();
    }

    public Control Control { get; }

    public int SelectedIndex
    {
        get => (int) _tableView.SelectedRow;
        set
        {
            if (value != SelectedIndex)
            {
                _tableView.SelectRow(value, false);
            }
        }
    }

    public event EventHandler? SelectionChanged;

    public void SetItems(IEnumerable<SettingsNavigatorItem> items)
    {
        _dataSource.Items = items.ToList();
        _tableView.ReloadData();
    }

    public override NSView GetViewForItem(NSTableView tableView, NSTableColumn tableColumn, nint row)
    {
        var item = _dataSource.Items[(int) row];
        var imageView = new NSImageView
        {
            ImageScaling = NSImageScale.ProportionallyUpOrDown,
            TranslatesAutoresizingMaskIntoConstraints = false
        };
        if (EtoPlatform.Current.IconProvider.GetIcon(item.IconName, IconScenario.Inline16) is { } icon)
        {
            imageView.Image = icon.ToNS();
        }
        var textField = NSTextField.CreateLabel(item.Label);
        textField.LineBreakMode = NSLineBreakMode.TruncatingTail;
        textField.TranslatesAutoresizingMaskIntoConstraints = false;

        var cell = new NSTableCellView { ImageView = imageView, TextField = textField };
        cell.AddSubview(imageView);
        cell.AddSubview(textField);
        NSLayoutConstraint.ActivateConstraints([
            imageView.LeadingAnchor.ConstraintEqualTo(cell.LeadingAnchor, 4),
            imageView.CenterYAnchor.ConstraintEqualTo(cell.CenterYAnchor),
            imageView.WidthAnchor.ConstraintEqualTo(IconSize),
            imageView.HeightAnchor.ConstraintEqualTo(IconSize),
            textField.LeadingAnchor.ConstraintEqualTo(imageView.TrailingAnchor, 10),
            textField.TrailingAnchor.ConstraintLessThanOrEqualTo(cell.TrailingAnchor, -4),
            textField.CenterYAnchor.ConstraintEqualTo(cell.CenterYAnchor)
        ]);
        return cell;
    }

    public override void SelectionDidChange(NSNotification notification)
    {
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private class DataSource : NSTableViewDataSource
    {
        public List<SettingsNavigatorItem> Items { get; set; } = [];

        public override nint GetRowCount(NSTableView tableView) => Items.Count;
    }
}