using Eto.GtkSharp;
using Eto.GtkSharp.Forms;
using Eto.GtkSharp.Forms.ToolBar;
using Gdk;
using Gtk;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Gtk;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Notifications;
using NAPS2.EtoForms.Widgets;
using NAPS2.Scan;
using Command = Eto.Forms.Command;

namespace NAPS2.EtoForms.Ui;

public class GtkDesktopForm : DesktopForm
{
    private readonly Dictionary<DesktopToolbarMenuType, MenuToolButton> _menuButtons = new();
    private Toolbar _toolbar = null!;
    private int _toolbarButtonCount;
    private int _toolbarMenuToggleCount;
    private int _toolbarPadding;
    private CssProvider? _toolbarPaddingCssProvider;
    private Toolbar _profilesToolbar = null!;
    private Box _windowContentBox = null!;
    private Box _innerContentBox = null!;
    private Eto.Forms.ContextMenu _toolbarContextMenu = null!;

    public GtkDesktopForm(
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
        IDarkModeProvider darkModeProvider,
        Sidebar sidebar,
        IIconProvider iconProvider,
        EditWithController editWithController)
        : base(config, keyboardShortcuts, notificationManager, cultureHelper, colorScheme, profileManager, imageList,
            thumbnailController, thumbnailProvider, desktopController, desktopScanController, imageListActions,
            imageListViewBehavior, desktopFormProvider, desktopSubFormController, commands, sidebar, iconProvider,
            editWithController)
    {
        ((GtkDarkModeProvider) darkModeProvider).StyleContext =
            Eto.Forms.Gtk3Helpers.ToNative(this).StyleContext;
        LoadCss();
        
        Load += (_, _) => colorScheme.ColorSchemeChanged += ColorSchemeChanged;
        UnLoad += (_, _) => colorScheme.ColorSchemeChanged -= ColorSchemeChanged;
    }

    private void ColorSchemeChanged(object? sender, EventArgs e)
    {
        LoadCss();
    }

    private void LoadCss()
    {
        var cssProvider = new CssProvider();
        var bgColor = _colorScheme.BackgroundColor.ToHex(false);
        var fgColor = _colorScheme.ForegroundColor.ToHex(false);
        var sepColor = _colorScheme.SeparatorColor.ToHex(false);
        var brdColor = _colorScheme.BorderColor.ToHex(false);
        cssProvider.LoadFromData(@"
            .desktop-toolbar-button * { min-width: 0; padding-left: 0; padding-right: 0; }
            .desktop-toolbar .image-button { min-width: 50px; padding-left: 0; padding-right: 0; }
            .desktop-toolbar .toggle { min-width: 0; padding-left: 0; padding-right: 0; }
            .preview-toolbar-button * { min-width: 0; padding-left: 0; padding-right: 0; }
            .preview-toolbar-button button { padding: 0 5px; }
            toolbar { border-bottom: 1px solid " + sepColor + @"; }
            toolbar.dock-left { border-bottom: none; border-right: 1px solid " + sepColor + @"; }
            toolbar.dock-bottom { border-bottom: none; border-top: 1px solid " + sepColor + @"; }
            toolbar.dock-right { border-bottom: none; border-left: 1px solid " + sepColor + @"; }
            .listview .frame { background-color: " + bgColor + @"; }
            .listview .drop-before { border-radius: 0; border-left: 3px solid " + fgColor + @"; padding-left: 0; }
            .listview .drop-after { border-radius: 0; border-right: 3px solid " + fgColor + @"; padding-right: 0; }
            .desktop-listview .listview-item image { border: 1px solid " + brdColor + @"; }
            .settings-navigator { background-color: @theme_base_color; border: 1px solid @borders; border-radius: 8px; padding: 4px; }
            .settings-navigator row { padding: 8px 10px; border-radius: 6px; }
            .link { padding: 0; }
            .donate-button { border: 1px solid #fbad5f; background: #feda96; }
            .donate-button:hover { background: #eeca86; }
            .zoom-button { background: " + bgColor + @"; border: 1px solid " + brdColor + @"; border-radius: 0; }
        ");
        StyleContext.AddProviderForScreen(Gdk.Screen.Default, cssProvider, 800);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        var listView = (GtkListView<UiImage>) _listView;
        listView.NativeControl.StyleContext.AddClass("desktop-listview");
        PlaceToolbars();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateToolbarPadding();
    }

    private void UpdateToolbarPadding()
    {
        // TODO: We need to handle exceptions from event handlers (if they're not already?)
        _toolbar.GetPreferredWidth(out _, out var toolbarWidth);
        var toolbarWidthLessPadding = toolbarWidth - _toolbarPadding;
        var excessWidth = Width - toolbarWidthLessPadding - 4;
        // A vertical toolbar has no width constraint
        var div = _toolbar.Orientation == Orientation.Vertical
            ? int.MaxValue
            : excessWidth / Math.Max(_toolbarButtonCount * 2 + _toolbarMenuToggleCount, 1);
        div = div.Clamp(0, 4);
        var buttonPadding = div;
        var togglePadding = div / 2;
        _toolbarPadding = buttonPadding * 2 * _toolbarButtonCount + togglePadding * 2 * _toolbarMenuToggleCount;
        if (_toolbarPaddingCssProvider == null)
        {
            _toolbarPaddingCssProvider = new CssProvider();
            StyleContext.AddProviderForScreen(Gdk.Screen.Default, _toolbarPaddingCssProvider, 800);
        }
        _toolbarPaddingCssProvider.LoadFromData($@"
            .desktop-toolbar .image-button {{ padding-left: {buttonPadding}px; padding-right: {buttonPadding}px; }}
            .desktop-toolbar .toggle {{ padding-left: {togglePadding}px; padding-right: {togglePadding}px; }}
        ");
    }

    protected override void ConfigureToolbars()
    {
        _toolbar = ((ToolBarHandler) ToolBar.Handler).Control;
        _toolbar.Style = ToolbarStyle.Both;
        _toolbar.StyleContext.AddClass("desktop-toolbar");

        _profilesToolbar = new Toolbar();
        _profilesToolbar.Style = ToolbarStyle.BothHoriz;

        _toolbarContextMenu = C.ContextMenu(this, GetToolbarContextMenuProvider());
        foreach (var toolbar in new[] { _toolbar, _profilesToolbar })
        {
            toolbar.PopupContextMenu += (_, args) =>
            {
                _toolbarContextMenu.Show();
                args.RetVal = true;
            };
        }

        // Eto puts the toolbar in a box above the window content. To support other dock positions, we instead put
        // the main toolbar next to the window content, and the profiles toolbar next to that in an inner box (so
        // they can be docked independently).
        ((Box) _toolbar.Parent).Remove(_toolbar);
        _windowContentBox = (Box) ((FormHandler) Handler).WindowContentControl;
        _innerContentBox = new Box(Orientation.Vertical, 0);
        foreach (var child in _windowContentBox.Children)
        {
            _windowContentBox.QueryChildPacking(child, out var expand, out var fill, out var padding, out var packType);
            _windowContentBox.Remove(child);
            if (packType == PackType.End)
            {
                _innerContentBox.PackEnd(child, expand, fill, padding);
            }
            else
            {
                _innerContentBox.PackStart(child, expand, fill, padding);
            }
        }
        _windowContentBox.PackStart(_innerContentBox, true, true, 0);
        _innerContentBox.Show();
    }

    internal IEnumerable<Toolbar> Toolbars => new[] { _toolbar, _profilesToolbar }.Where(x => x.Parent != null);

    public override void PlaceToolbars()
    {
        PlaceToolbar(_toolbar, _windowContentBox, ToolbarConfig.Get(c => c.DesktopToolStripDock));
        if (ToolbarConfig.Get(c => c.ShowProfilesToolbar))
        {
            PlaceToolbar(_profilesToolbar, _innerContentBox, ToolbarConfig.Get(c => c.ProfilesToolStripDock));
        }
        else
        {
            (_profilesToolbar.Parent as Container)?.Remove(_profilesToolbar);
        }
        UpdateToolbarPadding();
        LayoutController.Invalidate();
    }

    private static void PlaceToolbar(Toolbar toolbar, Box box, DockStyle dock)
    {
        bool vertical = dock is DockStyle.Left or DockStyle.Right;
        toolbar.Orientation = vertical ? Orientation.Vertical : Orientation.Horizontal;
        foreach (var d in Enum.GetValues<DockStyle>())
        {
            toolbar.StyleContext.RemoveClass(GetDockClass(d));
        }
        toolbar.StyleContext.AddClass(GetDockClass(dock));

        (toolbar.Parent as Container)?.Remove(toolbar);
        // Toolbars are stacked perpendicular to their orientation
        box.Orientation = vertical ? Orientation.Horizontal : Orientation.Vertical;
        box.PackStart(toolbar, false, false, 0);
        box.ReorderChild(toolbar, dock is DockStyle.Left or DockStyle.Top ? 0 : -1);
        toolbar.ShowAll();
    }

    private static string GetDockClass(DockStyle dock) => $"dock-{dock.ToString().ToLowerInvariant()}";

    protected override void UpdateProfilesToolbar()
    {
        var profiles = _profileManager.Profiles;
        var extra = _profilesToolbar.NItems - profiles.Count;
        var missing = profiles.Count - _profilesToolbar.NItems;
        for (int i = 0; i < extra; i++)
        {
            _profilesToolbar.Remove(_profilesToolbar.Children.Last());
        }
        for (int i = 0; i < missing; i++)
        {
            var item = new ToolButton(Icons.control_play_blue_16.ToEtoImage().ToGtk(), "test")
            {
                Homogeneous = false,
                IsImportant = true
            };
            item.Clicked += (_, _) => _desktopScanController.ScanWithProfile((ScanProfile) item.Data["naps2_profile"]!);
            _profilesToolbar.Add(item);
        }
        for (int i = 0; i < profiles.Count; i++)
        {
            var profile = profiles[i];
            var item = (ToolButton) _profilesToolbar.GetNthItem(i);
            item.Data["naps2_profile"] = profile;
            if (item.Label != profile.DisplayName)
            {
                item.Label = profile.DisplayName;
            }
        }
    }

    protected override void RecreateToolbarsAndMenus()
    {
        // The toolbar items are added natively rather than through Eto, so we need to remove them natively too
        foreach (var child in _toolbar.Children)
        {
            _toolbar.Remove(child);
        }
        _toolbarButtonCount = 0;
        _toolbarMenuToggleCount = 0;
        _menuButtons.Clear();
        base.RecreateToolbarsAndMenus();
        _toolbar.ShowAll();
        UpdateToolbarPadding();
    }

    protected override void CreateToolbarButton(Command command)
    {
        var button = new ToolButton(GetCommandImage(command, IconScenario.Toolbar), command.ToolBarText)
        {
            Homogeneous = false,
            Sensitive = command.Enabled
        };
        button.Clicked += (_, _) => command.Execute();
        command.EnabledChanged += (_, _) => button.Sensitive = command.Enabled;
        button.StyleContext.AddClass("desktop-toolbar-button");
        _toolbar.Add(button);
        _toolbarButtonCount++;
    }

    protected override void CreateToolbarSeparator()
    {
        _toolbar.Add(new SeparatorToolItem());
    }

    protected override void CreateToolbarStackedButtons(Command command1, Command command2)
    {
        // Simply create a ToolItem with two buttons stacked on top of each other
        var button1 = CreateToolButton(command1, Orientation.Horizontal);
        var button2 = CreateToolButton(command2, Orientation.Horizontal);
        // The content of both buttons has the same width (so their icons line up), centered in a vertical toolbar
        var sizeGroup = new SizeGroup(SizeGroupMode.Horizontal);
        foreach (var content in new[] { button1.Child, button2.Child })
        {
            content.Halign = Align.Center;
            sizeGroup.AddWidget(content);
        }
        var vbox = new Box(Orientation.Vertical, 0);
        vbox.Add(button1);
        vbox.Add(button2);
        var toolItem = new ToolItem();
        toolItem.Add(vbox);

        // Handle the toolbar overflowing into a menu
        toolItem.CreateMenuProxy += (o, args) =>
        {
            var menuItem1 = (ImageMenuItem) new Eto.Forms.ButtonMenuItem(command1).ControlObject;
            var menuItem2 = (ImageMenuItem) new Eto.Forms.ButtonMenuItem(command2).ControlObject;
            // Hack to show two menu items instead of one. Seems to work for now.
            menuItem1.ParentSet += (_, _) => ((Container) menuItem1.Parent)?.Add(menuItem2);
            toolItem.SetProxyMenuItem("", menuItem1);
        };
        // GTK ties the menu item's sensitivity to the ToolItem's sensitivity. We only need to check the first command
        // since the second command's menu item is hacked in.
        toolItem.Sensitive = command1.Enabled;
        command1.EnabledChanged += (_, _) => toolItem.Sensitive = command1.Enabled;

        _toolbar.Add(toolItem);
        _toolbarButtonCount++;
    }

    protected override void CreateToolbarButtonWithMenu(Command command, DesktopToolbarMenuType menuType,
        MenuProvider menu)
    {
        var button = new MenuToolButton(GetCommandImage(command, IconScenario.Toolbar), command.ToolBarText)
        {
            Homogeneous = false,
            Sensitive = command.Enabled
        };
        button.Clicked += (_, _) => command.Execute();
        command.EnabledChanged += (_, _) => button.Sensitive = command.Enabled;
        button.Menu = CreateMenuWidget(command, menu);
        button.StyleContext.AddClass("desktop-toolbar-button");
        button.ToolbarReconfigured += (_, _) => PlaceMenuArrow(button);
        PlaceMenuArrow(button);
        _toolbar.Add(button);
        _toolbarButtonCount++;
        _toolbarMenuToggleCount++;
        _menuButtons[menuType] = button;
    }

    private static void PlaceMenuArrow(MenuToolButton button)
    {
        // In a vertical toolbar, GTK puts the arrow below the button (pointing right), which takes up a lot of space.
        // Instead we keep it to the side (pointing down) like in a horizontal toolbar.
        if (button.Orientation != Orientation.Vertical || button.Child is not Box box) return;
        var arrowButton = box.Children.OfType<ToggleButton>().FirstOrDefault();
        var mainButton = box.Children.OfType<Button>().FirstOrDefault(x => x is not ToggleButton);
        if (arrowButton == null || mainButton == null) return;
        box.Orientation = Orientation.Horizontal;
        box.SetChildPacking(mainButton, true, true, 0, PackType.Start);
        (arrowButton.Child as Image)?.SetFromIconName("pan-down-symbolic", IconSize.Button);
        // Balance the arrow with a spacer of the same width so the content is centered in the toolbar like other
        // buttons
        var spacer = new Box(Orientation.Horizontal, 0);
        box.PackStart(spacer, false, false, 0);
        box.ReorderChild(spacer, 0);
        spacer.Show();
        var sizeGroup = new SizeGroup(SizeGroupMode.Horizontal);
        sizeGroup.AddWidget(spacer);
        sizeGroup.AddWidget(arrowButton);
    }

    protected override void CreateToolbarMenu(Command command, MenuProvider menu)
    {
        var button = new ToolButton(GetCommandImage(command, IconScenario.Toolbar), command.ToolBarText)
        {
            Homogeneous = false,
            Sensitive = command.Enabled
        };
        command.EnabledChanged += (_, _) => button.Sensitive = command.Enabled;
        var menuDelegate = GetMenuDelegate(CreateMenuWidget(command, menu), button);
        button.Clicked += menuDelegate;
        button.StyleContext.AddClass("desktop-toolbar-button");
        _toolbar.Add(button);
        _toolbarButtonCount++;
    }

    private Menu CreateMenuWidget(Command command, MenuProvider menu)
    {
        var subMenu = CreateSubMenu(command, menu);
        var menuItem = (ImageMenuItem) subMenu.ControlObject;
        return (Menu) menuItem.Submenu;
    }

    private EventHandler GetMenuDelegate(Menu menuWidget, Widget button)
    {
        return (_, _) => menuWidget.PopupAtWidget(button, Gravity.SouthWest, Gravity.NorthWest, null);
    }

    public override void ShowToolbarMenu(DesktopToolbarMenuType menuType)
    {
        var button = _menuButtons.Get(menuType);
        (button?.Menu as Menu)?.PopupAtWidget(button, Gravity.SouthWest, Gravity.NorthWest, null);
    }

    private Button CreateToolButton(Command command, Orientation orientation = Orientation.Vertical,
        int spacing = 4)
    {
        var box = new Box(orientation, spacing);
        box.Add(GetCommandImage(command, IconScenario.SmallToolbar));
        var label = new Label(command.ToolBarText);
        box.Add(label);
        var button = new Button(box)
        {
            Relief = ReliefStyle.None,
            Sensitive = command.Enabled
        };
        button.Clicked += (_, _) => command.Execute();
        command.EnabledChanged +=
            (_, _) => button.Sensitive = command.Enabled;
        return button;
    }

    private Image GetCommandImage(Command command, IconScenario scenario)
    {
        var scale = EtoPlatform.Current.GetScaleFactor(this);
        var image = ((ActionCommand) command).GetIconImage(scenario, scale);
        return image.ToGdk().ToScaledImage((int) scale);
    }
}