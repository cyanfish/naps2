using Eto.Forms;

namespace NAPS2.EtoForms.Widgets;

/// <summary>
/// A vertical list of items (e.g. settings panels) where exactly one item is selected at a time.
/// </summary>
public interface ISettingsNavigator
{
    Control Control { get; }

    int SelectedIndex { get; set; }

    event EventHandler SelectionChanged;

    void SetItems(IEnumerable<SettingsNavigatorItem> items);
}