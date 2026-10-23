using System.Windows;
using System.Windows.Controls;
using Eto.Wpf;
using NAPS2.EtoForms.Widgets;
using Image = System.Windows.Controls.Image;

namespace NAPS2.EtoForms.Wpf;

public class WpfSettingsNavigator : ISettingsNavigator
{
    private readonly ListView _listView = new() { SelectionMode = SelectionMode.Single };

    public WpfSettingsNavigator()
    {
        _listView.SelectionChanged += (_, _) => SelectionChanged?.Invoke(this, EventArgs.Empty);
        Control = Eto.Forms.WpfHelpers.ToEto(_listView);
    }

    public Eto.Forms.Control Control { get; }

    public int SelectedIndex
    {
        get => _listView.SelectedIndex;
        set => _listView.SelectedIndex = value;
    }

    public event EventHandler? SelectionChanged;

    public void SetItems(IEnumerable<SettingsNavigatorItem> items)
    {
        _listView.Items.Clear();
        foreach (var item in items)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            var image = new Image { Width = 16, Height = 16, Margin = new Thickness(0, 0, 10, 0) };
            EtoPlatform.Current.AttachDpiDependency(Control, scale => image.Source =
                EtoPlatform.Current.IconProvider.GetIcon(item.IconName, IconScenario.Inline16, scale)?.ToWpf());
            panel.Children.Add(image);
            panel.Children.Add(new TextBlock { Text = item.Label, VerticalAlignment = VerticalAlignment.Center });
            _listView.Items.Add(new ListViewItem { Content = panel });
        }
    }
}
