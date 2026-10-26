using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

internal class CombinedSettingsForm : EtoDialogBase
{
    private const int NAVIGATOR_WIDTH = 200;
    private const int NAVIGATOR_SPACING = 10;
    private const int DEFAULT_WIDTH = 720;
    // GTK controls are generally taller
    private static int DefaultHeight => EtoPlatform.Current.IsGtk ? 640 : EtoPlatform.Current.IsMac ? 540 : 520;
    // The minimum panel area size, which keeps the minimum form size consistent between panels (as long as each panel
    // fits)
    private const int MIN_PANEL_WIDTH = 400;
    private const int MIN_PANEL_HEIGHT = 250;

    private readonly List<SettingsPanel> _panels;
    private readonly List<LayoutVisibility> _panelVisibilities;
    private readonly ISettingsNavigator _navigator = EtoPlatform.Current.CreateSettingsNavigator();
    private readonly Button _restoreDefaults = new() { Text = UiStrings.RestoreDefaults };
    private readonly HashSet<SettingsPanel> _shownPanels = [];
    private bool _isShown;
    private bool _saved;

    public CombinedSettingsForm(Naps2Config config, DialogHelper dialogHelper,
        DesktopFormProvider desktopFormProvider, KeyboardShortcutManager ksm) : base(config)
    {
        Title = UiStrings.SettingsFormTitle;
        IconName = "cog";

        _panels =
        [
            new GeneralSettingsPanel(this, desktopFormProvider),
            new PdfSettingsPanel(this, dialogHelper),
            new ImageSettingsPanel(this, dialogHelper),
            new EmailSettingsPanel(this)
        ];
        if (PlatformCompat.System.SupportsKeyboardShortcuts)
        {
            _panels.Add(new KeyboardShortcutsPanel(this, ksm, desktopFormProvider));
        }
        if (PlatformCompat.System.SupportsToolbarCustomization)
        {
            _panels.Add(new ToolbarSettingsPanel(this, desktopFormProvider));
        }
        _panelVisibilities = _panels.Select(_ => new LayoutVisibility(false)).ToList();

        _navigator.SetItems(_panels.Select(panel => new SettingsNavigatorItem(panel.Title, panel.IconName)));
        _navigator.SelectionChanged += (_, _) => UpdateSelectedPanel();
        // The navigator might already have selected the first item without raising SelectionChanged
        _navigator.SelectedIndex = 0;
        UpdateSelectedPanel();
        _restoreDefaults.Click += (_, _) => SelectedPanel.RestoreDefaults();
    }

    private SettingsPanel SelectedPanel => _panels[Math.Max(_navigator.SelectedIndex, 0)];

    public void SelectPanel<T>() where T : SettingsPanel
    {
        _navigator.SelectedIndex = _panels.FindIndex(panel => panel is T);
    }

    protected override void BuildLayout()
    {
        // A native sidebar is outside the client area
        var navigatorWidth = EtoPlatform.Current.SupportsWindowSidebar ? NAVIGATOR_WIDTH + NAVIGATOR_SPACING : 0;
        FormStateController.FixedDefaultClientSize = new SizeF(DEFAULT_WIDTH - navigatorWidth, DefaultHeight);

        var panels = L.Row(
            L.Column(
                new LayoutControl(null).Width(MIN_PANEL_WIDTH).SpacingAfter(0),
                _panels.Select((panel, i) => BuildPanelLayout(panel, _panelVisibilities[i])).Expand()
            ).SpacingAfter(0).Scale(),
            new LayoutControl(null).Height(MIN_PANEL_HEIGHT)
        ).Scale();
        LayoutController.Content = L.Column(
            EtoPlatform.Current.SupportsWindowSidebar
                ? panels
                : L.Row(_navigator.Control.Width(NAVIGATOR_WIDTH), panels).Spacing(NAVIGATOR_SPACING).Scale(),
            L.Row(
                _restoreDefaults.MinWidth(140),
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, Save),
                    C.CancelButton(this))
            )
        );
    }

    private static LayoutElement BuildPanelLayout(SettingsPanel panel, LayoutVisibility visibility)
    {
        var scrollableLayout = panel.BuildScrollableLayout();
        return L.Column(
            scrollableLayout != null ? L.Scrollable(scrollableLayout).Scale() : C.None(),
            panel.BuildFixedLayout() ?? C.None()
        ).Visible(visibility).Scale();
    }

    protected override void OnLoadComplete(EventArgs e)
    {
        base.OnLoadComplete(e);
        if (EtoPlatform.Current.SupportsWindowSidebar)
        {
            EtoPlatform.Current.AttachWindowSidebar(this, _navigator.Control, new WindowSidebarOptions
            {
                Width = NAVIGATOR_WIDTH,
                MinWidth = NAVIGATOR_WIDTH,
                MaxWidth = NAVIGATOR_WIDTH
            });
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _isShown = true;
        ShowPanel(SelectedPanel);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (!_saved)
        {
            foreach (var panel in _panels)
            {
                panel.Cancel();
            }
        }
    }

    private void UpdateSelectedPanel()
    {
        var selected = SelectedPanel;
        for (int i = 0; i < _panels.Count; i++)
        {
            _panelVisibilities[i].IsVisible = _panels[i] == selected;
        }
        if (EtoPlatform.Current.IsMac)
        {
            // Per the macOS guidelines, the title should reflect the current pane
            Title = selected.Title;
        }
        if (_isShown)
        {
            ShowPanel(selected);
        }
    }

    // Panels aren't shown until they're selected, as showing some panels is relatively slow (e.g. populating a grid)
    private void ShowPanel(SettingsPanel panel)
    {
        if (_shownPanels.Add(panel))
        {
            panel.OnShown();
        }
    }

    private void Save()
    {
        foreach (var panel in _panels)
        {
            panel.Save();
        }
        _saved = true;
    }
}