using Eto.GtkSharp;
using Eto.GtkSharp.Forms.ToolBar;
using Gdk;
using Gtk;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Gtk;
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
        DesktopCommands commands,
        IDarkModeProvider darkModeProvider,
        Sidebar sidebar)
        : base(config, keyboardShortcuts, notificationManager, cultureHelper, colorScheme, profileManager, imageList,
            thumbnailController, thumbnailProvider, desktopController, desktopScanController, imageListActions,
            imageListViewBehavior, desktopFormProvider, desktopSubFormController, commands, sidebar)
    {
        ((GtkDarkModeProvider) darkModeProvider).StyleContext =
            Eto.Forms.Gtk3Helpers.ToNative(this).StyleContext;
        var cssProvider = new CssProvider();
        var bgColor = colorScheme.BackgroundColor.ToHex(false);
        var fgColor = colorScheme.ForegroundColor.ToHex(false);
        var sepColor = colorScheme.SeparatorColor.ToHex(false);
        var brdColor = colorScheme.BorderColor.ToHex(false);
        cssProvider.LoadFromData(@"
            .desktop-toolbar-button * { min-width: 0; padding-left: 0; padding-right: 0; }
            .desktop-toolbar .image-button { min-width: 50px; padding-left: 0; padding-right: 0; }
            .desktop-toolbar .toggle { min-width: 0; padding-left: 0; padding-right: 0; }
            .preview-toolbar-button * { min-width: 0; padding-left: 0; padding-right: 0; }
            .preview-toolbar-button button { padding: 0 5px; }
            toolbar { border-bottom: 1px solid " + sepColor + @"; }
            .listview .frame { background-color: " + bgColor + @"; }
            .listview .drop-before { border-radius: 0; border-left: 3px solid " + fgColor + @"; padding-left: 0; }
            .listview .drop-after { border-radius: 0; border-right: 3px solid " + fgColor + @"; padding-right: 0; }
            .desktop-listview .listview-item image { border: 1px solid " + brdColor + @"; }
            .link { padding: 0; }
            .accessible-image-button { border: none; background: none; }
            .zoom-button { background: " + bgColor + @"; border: 1px solid " + brdColor + @"; border-radius: 0; }
        ");
        StyleContext.AddProviderForScreen(Gdk.Screen.Default, cssProvider, 800);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        var listView = (GtkListView<UiImage>) _listView;
        listView.NativeControl.StyleContext.AddClass("desktop-listview");
        PlaceProfilesToolbar();
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
        var div = excessWidth / (_toolbarButtonCount * 2 + _toolbarMenuToggleCount);
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
    }

    public override void PlaceProfilesToolbar()
    {
        if (Config.Get(c => c.ShowProfilesToolbar) && _profilesToolbar.Parent == null)
        {
            ((VBox) _toolbar.Parent).Add(_profilesToolbar);
            LayoutController.Invalidate();
        }
        if (!Config.Get(c => c.ShowProfilesToolbar) && _profilesToolbar.Parent != null)
        {
            ((VBox) _toolbar.Parent).Remove(_profilesToolbar);
            LayoutController.Invalidate();
        }
    }

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
            var item = new ToolButton(Icons.control_play_blue_small.ToEtoImage().ToGtk(), "test")
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

    protected override void CreateToolbarButton(Command command)
    {
        var button = new ToolButton(command.Image.ToGtk(), command.ToolBarText)
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
        var button = new MenuToolButton(command.Image.ToGtk(), command.ToolBarText)
        {
            Homogeneous = false,
            Sensitive = command.Enabled
        };
        button.Clicked += (_, _) => command.Execute();
        command.EnabledChanged += (_, _) => button.Sensitive = command.Enabled;
        button.Menu = CreateMenuWidget(command, menu);
        button.StyleContext.AddClass("desktop-toolbar-button");
        _toolbar.Add(button);
        _toolbarButtonCount++;
        _toolbarMenuToggleCount++;
        _menuButtons[menuType] = button;
    }

    protected override void CreateToolbarMenu(Command command, MenuProvider menu)
    {
        var button = new ToolButton(command.Image.ToGtk(), command.ToolBarText)
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

    private static Button CreateToolButton(Command command, Orientation orientation = Orientation.Vertical,
        int spacing = 4)
    {
        var box = new Box(orientation, spacing);
        box.Add(command.Image.ToGtk());
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
}