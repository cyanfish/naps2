using Eto.GtkSharp;
using Gtk;
using NAPS2.EtoForms.Widgets;
using Label = Gtk.Label;
using Orientation = Gtk.Orientation;

namespace NAPS2.EtoForms.Gtk;

public class GtkSettingsNavigator : ISettingsNavigator
{
    private readonly ListBox _listBox = new() { SelectionMode = SelectionMode.Browse };

    public GtkSettingsNavigator()
    {
        _listBox.StyleContext.AddClass("settings-navigator");
        _listBox.RowSelected += (_, _) => SelectionChanged?.Invoke(this, EventArgs.Empty);
        Control = _listBox.AsEto();
    }

    public Eto.Forms.Control Control { get; }

    public int SelectedIndex
    {
        get => _listBox.SelectedRow?.Index ?? -1;
        set
        {
            if (value != SelectedIndex)
            {
                _listBox.SelectRow(value >= 0 ? _listBox.GetRowAtIndex(value) : null);
            }
        }
    }

    public event EventHandler? SelectionChanged;

    public void SetItems(IEnumerable<SettingsNavigatorItem> items)
    {
        foreach (var row in _listBox.Children)
        {
            _listBox.Remove(row);
            row.Destroy();
        }
        foreach (var item in items)
        {
            var box = new Box(Orientation.Horizontal, 10);
            var icon = EtoPlatform.Current.IconProvider.GetIcon(item.IconName, IconScenario.Inline16);
            if (icon != null)
            {
                box.PackStart(new Image(icon.ToGdk()), false, false, 0);
            }
            box.PackStart(new Label(item.Label) { Xalign = 0 }, true, true, 0);
            var row = new ListBoxRow();
            row.Add(box);
            _listBox.Add(row);
        }
        _listBox.ShowAll();
    }
}