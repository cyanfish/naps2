using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Eto.Forms;
using Eto.Wpf;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Notifications;
using NAPS2.EtoForms.Widgets;
using NAPS2.EtoForms.Wpf;
using Button = System.Windows.Controls.Button;
using VerticalAlignment = System.Windows.VerticalAlignment;
using Command = Eto.Forms.Command;
using ContextMenu = System.Windows.Controls.ContextMenu;
using Image = System.Windows.Controls.Image;
using Orientation = System.Windows.Controls.Orientation;
using Separator = System.Windows.Controls.Separator;
using ToolBar = System.Windows.Controls.ToolBar;

namespace NAPS2.EtoForms.Ui;

public class WpfDesktopForm : DesktopForm
{
    private readonly WpfToolbarFormatter _toolbarFormatter = new();
    private readonly Dictionary<DesktopToolbarMenuType, (FrameworkElement target, ContextMenu menu)> _menus = new();
    private readonly ToolBar _profilesToolBar = new();
    private readonly ToolBarTray _toolbarTray = new() { Background = Brushes.Transparent };
    private readonly ToolBarTray _profilesToolbarTray = new() { Background = Brushes.Transparent };
    private readonly Border _toolbarBorder = new();
    private readonly Border _profilesToolbarBorder = new() { Visibility = Visibility.Collapsed };
    private ContentControl _toolbarHolder = null!;
    private ToolBar _mainToolBar = null!;

    public WpfDesktopForm(
        Naps2Config config,
        DesktopKeyboardShortcuts keyboardShortcuts,
        NotificationManager notificationManager,
        CultureHelper cultureHelper,
        ColorScheme colorScheme,
        IProfileManager profileManager,
        UiImageList imageList,
        ThumbnailController thumbnailController,
        UiThumbnailProvider thumbnailProvider,
        DesktopController desktopController,
        IDesktopScanController desktopScanController,
        ImageListActions imageListActions,
        ImageListViewBehavior imageListViewBehavior,
        DesktopFormProvider desktopFormProvider,
        IDesktopSubFormController desktopSubFormController,
        Lazy<DesktopCommands> commands,
        Sidebar sidebar,
        IIconProvider iconProvider,
        EditWithController editWithController)
        : base(config, keyboardShortcuts, notificationManager, cultureHelper, colorScheme, profileManager, imageList,
            thumbnailController, thumbnailProvider, desktopController, desktopScanController, imageListActions,
            imageListViewBehavior, desktopFormProvider, desktopSubFormController, commands, sidebar, iconProvider,
            editWithController)
    {
        this.ToNative().SizeChanged += (_, e) =>
        {
            if (_toolbarTray.Orientation == Orientation.Horizontal ? e.WidthChanged : e.HeightChanged)
            {
                _toolbarFormatter.RelayoutToolbar(_mainToolBar);
            }
        };
        Load += (_, _) =>
        {
            colorScheme.ColorSchemeChanged += ColorSchemeChanged;
            System.Windows.Application.Current.SessionEnding += SessionEnding;
        };
        UnLoad += (_, _) =>
        {
            colorScheme.ColorSchemeChanged -= ColorSchemeChanged;
            System.Windows.Application.Current.SessionEnding -= SessionEnding;
        };
        UpdateColors();
    }

    private WpfListView<UiImage> WpfListView => (WpfListView<UiImage>) _listView;

    private void ColorSchemeChanged(object? sender, EventArgs e) => this.ToNative().Dispatcher.Invoke(UpdateColors);

    private void UpdateColors()
    {
        _toolbarBorder.BorderBrush = _profilesToolbarBorder.BorderBrush = _colorScheme.SeparatorColor.ToWpfBrush();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        PlaceToolbars();
        // Separator between the sidebar and the listview
        WpfListView.NativeControl.BorderThickness = new Thickness(1, 0, 0, 0);
        WpfListView.NativeControl.Focus();
    }

    protected override void OnLoadComplete(EventArgs e)
    {
        // Lay out the toolbar before the form is rendered so it doesn't visibly change
        _toolbarFormatter.RelayoutToolbar(_mainToolBar);
        base.OnLoadComplete(e);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        this.ToNative().SizeToContent = SizeToContent.Manual;
    }

    // The entire app is being forced to close (e.g. for a system restart)
    private void SessionEnding(object? sender, SessionEndingCancelEventArgs e) => UserClosing = false;

    protected override void RecreateToolbarsAndMenus()
    {
        _toolbarFormatter.Clear();
        base.RecreateToolbarsAndMenus();
        _toolbarFormatter.RelayoutToolbar(_mainToolBar);
    }

    protected override void ConfigureToolbars()
    {
        _mainToolBar = (ToolBar) ToolBar.ControlObject;
        ToolBarTray.SetIsLocked(_profilesToolBar, true);

        // Eto puts the toolbar in a holder that's docked (to the top) in the window's DockPanel
        _toolbarHolder = (ContentControl) _mainToolBar.Parent;
        _toolbarHolder.Content = null;
        _toolbarTray.ToolBars.Add(_mainToolBar);
        _toolbarTray.ContextMenu = GetToolbarContextMenuProvider().ToWpfContextMenu(this);
        _toolbarBorder.Child = _toolbarTray;
        _toolbarHolder.Content = _toolbarBorder;

        // The profiles toolbar is docked separately, inside the main toolbar (i.e. later in the DockPanel)
        var dockPanel = (DockPanel) _toolbarHolder.Parent;
        _profilesToolbarTray.ToolBars.Add(_profilesToolBar);
        _profilesToolbarTray.ContextMenu = _toolbarTray.ContextMenu;
        _profilesToolbarBorder.Child = _profilesToolbarTray;
        dockPanel.Children.Insert(dockPanel.Children.IndexOf(_toolbarHolder) + 1, _profilesToolbarBorder);
    }

    public override void PlaceToolbars()
    {
        PlaceToolbar(_toolbarHolder, _toolbarTray, _toolbarBorder, ToolbarConfig.Get(c => c.DesktopToolStripDock));
        PlaceToolbar(_profilesToolbarBorder, _profilesToolbarTray, _profilesToolbarBorder,
            ToolbarConfig.Get(c => c.ProfilesToolStripDock));
        _profilesToolbarBorder.Visibility =
            ToolbarConfig.Get(c => c.ShowProfilesToolbar) ? Visibility.Visible : Visibility.Collapsed;
        _toolbarFormatter.RelayoutToolbar(_mainToolBar);
    }

    private static void PlaceToolbar(UIElement dockedElement, ToolBarTray tray, Border border, DockStyle dock)
    {
        DockPanel.SetDock(dockedElement, dock switch
        {
            DockStyle.Left => Dock.Left,
            DockStyle.Bottom => Dock.Bottom,
            DockStyle.Right => Dock.Right,
            _ => Dock.Top
        });
        tray.Orientation = dock is DockStyle.Left or DockStyle.Right
            ? Orientation.Vertical
            : Orientation.Horizontal;
        // Separator between the toolbar and the content
        border.BorderThickness = dock switch
        {
            DockStyle.Left => new Thickness(0, 0, 1, 0),
            DockStyle.Bottom => new Thickness(0, 1, 0, 0),
            DockStyle.Right => new Thickness(1, 0, 0, 0),
            _ => new Thickness(0, 0, 0, 1)
        };
    }

    public override void UpdateListView()
    {
        WpfListView.Update();
    }

    protected override void UpdateProfilesToolbar()
    {
        _profilesToolBar.Items.Clear();
        foreach (var profile in _profileManager.Profiles)
        {
            var (content, image, _) = _toolbarFormatter.CreateButtonContent(profile.DisplayName, false);
            SetIcon(image, scale => _iconProvider.GetIcon("control_play_blue", IconScenario.SmallToolbar, scale));
            var button = _toolbarFormatter.CreateProfileButton(content);
            button.Click += (_, _) => _desktopScanController.ScanWithProfile(profile);
            _profilesToolBar.Items.Add(button);
        }
    }

    protected override void SetThumbnailSpacing(int thumbnailSize, float scale)
    {
        const int MIN_PADDING = 6;
        const int MAX_PADDING = 66;
        // Linearly scale the padding with the thumbnail size
        int padding = MIN_PADDING + (MAX_PADDING - MIN_PADDING) * (thumbnailSize - ThumbnailSizes.MIN_SIZE) /
            (ThumbnailSizes.MAX_SIZE - ThumbnailSizes.MIN_SIZE);
        WpfListView.ItemMargin = new Thickness(padding / 2.0);
    }

    protected override void CreateToolbarButton(Command command)
    {
        var button = _toolbarFormatter.CreateLargeButton(CreateButtonContent(command, true));
        button.BindCommand(command);
        _mainToolBar.Items.Add(button);
    }

    protected override void CreateToolbarButtonWithMenu(Command command, DesktopToolbarMenuType menuType,
        MenuProvider menu)
    {
        // WPF has no split button, so we put a regular button next to a narrow dropdown button
        var mainButton = _toolbarFormatter.CreateLargeButton(
            CreateButtonContent(command, true), isSplit: true);
        mainButton.BindCommand(command);
        var arrowButton = _toolbarFormatter.CreateArrowButton();
        AutomationProperties.SetName(arrowButton, command.ToolBarText);
        DockPanel.SetDock(arrowButton, Dock.Right);
        var group = new DockPanel { Children = { arrowButton, mainButton } };
        var contextMenu = menu.ToWpfContextMenu(this);
        arrowButton.Click += (_, _) => ShowMenu(group, contextMenu);
        arrowButton.BindEnabled(command);
        _menus[menuType] = (group, contextMenu);
        _mainToolBar.Items.Add(group);
    }

    protected override LayoutElement GetZoomButtons()
    {
        var zoomIn = C.ImageButton(Commands.ZoomIn);
        EtoPlatform.Current.ConfigureZoomButton(zoomIn, "zoom_in");
        var zoomOut = C.ImageButton(Commands.ZoomOut);
        EtoPlatform.Current.ConfigureZoomButton(zoomOut, "zoom_out");
        // Join the buttons into a single "pill", with one divider line (the border is translucent, so overlapping the
        // buttons would make the divider darker)
        zoomOut.ToNative().SetValue(Border.CornerRadiusProperty, new CornerRadius(4, 0, 0, 4));
        var zoomInNative = (System.Windows.Controls.Control) zoomIn.ToNative();
        zoomInNative.SetValue(Border.CornerRadiusProperty, new CornerRadius(0, 4, 4, 0));
        zoomInNative.BorderThickness = new Thickness(0, 1, 1, 1);
        return L.Row(zoomOut.AlignTrailing(), zoomIn.AlignTrailing()).Spacing(0);
    }

    protected override void CreateToolbarMenu(Command command, MenuProvider menu)
    {
        var button = _toolbarFormatter.CreateLargeButton(CreateButtonContent(command, true));
        var contextMenu = menu.ToWpfContextMenu(this);
        button.Click += (_, _) => ShowMenu(button, contextMenu);
        AutomationProperties.SetName(button, command.ToolBarText);
        button.BindEnabled(command);
        _mainToolBar.Items.Add(button);
    }

    protected override void CreateToolbarStackedButtons(Command command1, Command command2)
    {
        var panel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            Children = { CreateStackedButton(command1), CreateStackedButton(command2) }
        };
        System.Windows.Controls.Grid.SetIsSharedSizeScope(panel, true);
        _mainToolBar.Items.Add(panel);
    }

    private Button CreateStackedButton(Command command)
    {
        var button = _toolbarFormatter.CreateStackedButton(CreateButtonContent(command, false));
        button.BindCommand(command);
        return button;
    }

    protected override void CreateToolbarSeparator()
    {
        _mainToolBar.Items.Add(new Separator());
    }

    public override void ShowToolbarMenu(DesktopToolbarMenuType menuType)
    {
        if (_menus.TryGetValue(menuType, out var menu))
        {
            ShowMenu(menu.target, menu.menu);
        }
    }

    private static void ShowMenu(FrameworkElement target, ContextMenu menu)
    {
        menu.PlacementTarget = target;
        menu.Placement = PlacementMode.Bottom;
        menu.IsOpen = true;
        // Clicking the button while the menu is open should only close the menu, so while it's open, clicks go through
        // the button (closing the menu as with any outside click) instead of re-opening it
        target.IsHitTestVisible = false;
        menu.Closed += OnClosed;

        void OnClosed(object sender, RoutedEventArgs e)
        {
            target.IsHitTestVisible = true;
            menu.Closed -= OnClosed;
        }
    }

    private FrameworkElement CreateButtonContent(Command command, bool large)
    {
        var (content, image, label) = _toolbarFormatter.CreateButtonContent(command.ToolBarText, large);
        var actionCommand = (ActionCommand) command;
        SetIcon(image, scale => actionCommand.GetIconImage(large ? IconScenario.Toolbar : IconScenario.SmallToolbar, scale));
        actionCommand.TextChanged += (_, _) => label.Text = command.ToolBarText;
        return content;
    }

    private void SetIcon(Image image, Func<float, Eto.Drawing.Image?> getIcon) =>
        EtoPlatform.Current.AttachDpiDependency(this, scale => image.Source = getIcon(scale)?.ToWpf());
}
