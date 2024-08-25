using System.ComponentModel;
using System.Drawing;
using Eto.Forms;
using Eto.WinForms;
using Eto.WinForms.Forms.ToolBar;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Notifications;
using NAPS2.EtoForms.Widgets;
using NAPS2.EtoForms.WinForms;
using NAPS2.Scan;
using NAPS2.WinForms;
using WF = System.Windows.Forms;

namespace NAPS2.EtoForms.Ui;

public class WinFormsDesktopForm : DesktopForm
{
    private readonly Dictionary<DesktopToolbarMenuType, WF.ToolStripSplitButton> _menuButtons = new();
    private readonly ToolbarFormatter _toolbarFormatter = new(new StringWrapper());
    private readonly WF.Form _form;
    private WF.ToolStrip _mainToolStrip = null!;
    private WF.ToolStrip _profilesToolStrip = null!;
    private WF.ToolStripContainer _container = null!;

    public WinFormsDesktopForm(
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
        IIconProvider iconProvider)
        : base(config, keyboardShortcuts, notificationManager, cultureHelper, colorScheme, profileManager, imageList,
            thumbnailController, thumbnailProvider, desktopController, desktopScanController, imageListActions,
            imageListViewBehavior, desktopFormProvider, desktopSubFormController, commands, sidebar, iconProvider)
    {
        _form = this.ToNative();
        _form.FormClosing += OnFormClosing;
        _form.DpiChanged += OnDpiChanged;

        // TODO: Remove this if https://github.com/picoe/Eto/issues/2601 is fixed
        NativeListView.KeyDown += (_, e) => OnKeyDown(new KeyEventArgs(e.KeyData.ToEto(), KeyEventType.KeyDown));
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // Don't do anything here as we have a separate FormClosing event handler
        // That allows us to check the close reason (which Eto doesn't provide)
    }

    private void OnFormClosing(object? sender, WF.FormClosingEventArgs e)
    {
        if (!_desktopController.PrepareForClosing(e.CloseReason == WF.CloseReason.UserClosing))
        {
            e.Cancel = true;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        SaveToolStripLocation();
        base.OnClosed(e);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        LoadToolStripLocation();
        OnDpiChanged(this, EventArgs.Empty);

        NativeListView.TabIndex = 7;
        NativeListView.BorderStyle = WF.BorderStyle.None;
        NativeListView.Focus();
    }

    protected override void OnShown(EventArgs e)
    {
        _toolbarFormatter.RelayoutToolbar(_mainToolStrip);
        base.OnShown(e);
    }

    protected override LayoutElement GetControlButtons()
    {
        // Disabled buttons don't prevent click events from being sent to the listview below the button, so without this
        // "mouse catcher" control you could e.g. spam click zoom out until it's maxed and then accidentally keep
        // clicking and change the listview selection.
        var mouseCatcher = new WF.Button
        {
            BackColor = Color.White,
            Size = new Size(45, 23),
            FlatStyle = WF.FlatStyle.Flat
        };
        return L.Row(
            GetSidebarButton(),
            L.Overlay(
                L.Row(mouseCatcher.ToEto().AlignTrailing()),
                GetZoomButtons()
            )
        );
    }

    private WF.ListView NativeListView => ((WinFormsListView<UiImage>) _listView).NativeControl;

    protected override void SetCulture(string cultureId)
    {
        SaveToolStripLocation();
        base.SetCulture(cultureId);
    }

    protected override void ConfigureToolbars()
    {
        _mainToolStrip = ((ToolBarHandler) ToolBar.Handler).Control;
        _mainToolStrip.ShowItemToolTips = false;
        _mainToolStrip.TabStop = true;
        _mainToolStrip.ParentChanged += (_, _) =>
        {
            _toolbarFormatter.RelayoutToolbar(_mainToolStrip);
            LayoutController.Invalidate();
        };

        _profilesToolStrip = new WF.ToolStrip();
        _profilesToolStrip.ShowItemToolTips = false;
        _profilesToolStrip.TabStop = true;
        _profilesToolStrip.Location = new Point(0, 100);
        _profilesToolStrip.ParentChanged += (_, _) => LayoutController.Invalidate();
    }

    private void OnDpiChanged(object? sender, EventArgs e)
    {
        _mainToolStrip.ImageScalingSize = new Size(_form.DeviceDpi * 32 / 96, _form.DeviceDpi * 32 / 96);
        _profilesToolStrip.ImageScalingSize = new Size(_form.DeviceDpi * 16 / 96, _form.DeviceDpi * 16 / 96);
    }

    public override void PlaceProfilesToolbar()
    {
        if (Config.Get(c => c.ShowProfilesToolbar) && _profilesToolStrip.Parent == null)
        {
            _container.TopToolStripPanel.Controls.Add(_profilesToolStrip);
        }
        if (!Config.Get(c => c.ShowProfilesToolbar) && _profilesToolStrip.Parent != null)
        {
            _profilesToolStrip.Parent.Controls.Remove(_profilesToolStrip);
        }
    }

    protected override void UpdateProfilesToolbar()
    {
        var toolbarItems = _profilesToolStrip.Items;
        var profiles = _profileManager.Profiles;
        var extra = toolbarItems.Count - profiles.Count;
        var missing = profiles.Count - toolbarItems.Count;
        for (int i = 0; i < extra; i++)
        {
            toolbarItems.RemoveAt(toolbarItems.Count - 1);
        }
        for (int i = 0; i < missing; i++)
        {
            var item = new WF.ToolStripButton
            {
                TextImageRelation = WF.TextImageRelation.ImageBeforeText,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleLeft,
                Image = _iconProvider.GetIcon("control_play_blue_small").ToSD()
            };
            item.Click += (_, _) => _desktopScanController.ScanWithProfile((ScanProfile) item.Tag!);
            toolbarItems.Add(item);
        }
        for (int i = 0; i < profiles.Count; i++)
        {
            var profile = profiles[i];
            var item = toolbarItems[i];
            item.Tag = profile;
            if (item.Text != profile.DisplayName)
            {
                item.Text = profile.DisplayName;
            }
        }
    }

    protected override void BuildLayout()
    {
        base.BuildLayout();

        _container = new WF.ToolStripContainer();
        _container.TopToolStripPanel.Controls.Add(_mainToolStrip);
        PlaceProfilesToolbar();
        foreach (var panel in _container.Controls.OfType<WF.ToolStripPanel>())
        {
            // Allow tabbing through the toolbar for accessibility
            WinFormsHacks.SetControlStyle(panel, WF.ControlStyles.Selectable, true);
        }

        var wfContent = LayoutController.Container.ToNative();
        wfContent.Dock = WF.DockStyle.Fill;
        var etoContainer = _container.ToEto();
        Content = etoContainer;
        _container.ContentPanel.Controls.Add(wfContent);
    }

    protected override void SetThumbnailSpacing(int thumbnailSize)
    {
        const int MIN_PADDING = 6;
        const int MAX_PADDING = 66;
        // Linearly scale the padding with the thumbnail size
        int padding = MIN_PADDING + (MAX_PADDING - MIN_PADDING) * (thumbnailSize - ThumbnailSizes.MIN_SIZE) /
            (ThumbnailSizes.MAX_SIZE - ThumbnailSizes.MIN_SIZE);
        int hSpacing = thumbnailSize + padding;
        int vSpacing = thumbnailSize + padding * 2;
        WinFormsHacks.SetListSpacing(NativeListView, hSpacing, vSpacing);
    }

    protected override void CreateToolbarButton(Command command)
    {
        var item = new WF.ToolStripButton
        {
            TextImageRelation = WF.TextImageRelation.ImageAboveText
        };
        ApplyCommand(item, command);
        _mainToolStrip.Items.Add(item);
    }

    protected override void CreateToolbarButtonWithMenu(Command command, DesktopToolbarMenuType menuType,
        MenuProvider menu)
    {
        var item = new WF.ToolStripSplitButton
        {
            TextImageRelation = WF.TextImageRelation.ImageAboveText
        };
        ApplyCommand(item, command);
        _mainToolStrip.Items.Add(item);
        menu.Handle(subItems => SetUpMenu(item, subItems));
        _menuButtons[menuType] = item;
    }

    private void SetUpMenu(WF.ToolStripDropDownItem item, List<MenuProvider.Item> subItems)
    {
        item.DropDownItems.Clear();
        foreach (var subItem in subItems)
        {
            switch (subItem)
            {
                case MenuProvider.SeparatorItem:
                    item.DropDownItems.Add(new WF.ToolStripSeparator());
                    break;
                case MenuProvider.CommandItem commandSubItem:
                    item.DropDownItems.Add(ApplyCommand(new WF.ToolStripMenuItem
                    {
                        ImageScaling = WF.ToolStripItemImageScaling.None
                    }, commandSubItem.Command));
                    break;
                case MenuProvider.SubMenuItem subMenuSubItem:
                    var subMenu = new WF.ToolStripMenuItem();
                    ApplyCommand(subMenu, subMenuSubItem.Command);
                    // TODO: If submenus are dynamic this will memory leak or something
                    subMenuSubItem.MenuProvider.Handle(subSubItems => SetUpMenu(subMenu, subSubItems));
                    item.DropDownItems.Add(subMenu);
                    break;
            }
        }
    }

    protected override void CreateToolbarMenu(Command command, MenuProvider menu)
    {
        var item = new WF.ToolStripDropDownButton
        {
            TextImageRelation = WF.TextImageRelation.ImageAboveText,
            ShowDropDownArrow = false
        };
        ApplyCommand(item, command);
        _mainToolStrip.Items.Add(item);
        menu.Handle(subItems => SetUpMenu(item, subItems));
    }

    protected override void CreateToolbarStackedButtons(Command command1, Command command2)
    {
        var item = new ToolStripDoubleButton
        {
            FirstImage = command1.Image.ToSD(),
            FirstText = command1.ToolBarText,
            SecondImage = command2.Image.ToSD(),
            SecondText = command2.ToolBarText
        };
        command1.EnabledChanged += (_, _) => item.Enabled = command1.Enabled;
        item.FirstClick += (_, _) => command1.Execute();
        item.SecondClick += (_, _) => command2.Execute();
        _mainToolStrip.Items.Add(item);
    }

    private WF.ToolStripItem ApplyCommand(WF.ToolStripItem item, Command command)
    {
        void SetItemText() => item.Text = item is WF.ToolStripMenuItem ? command.MenuText : command.ToolBarText;
        item.Image = command.Image.ToSD();
        SetItemText();
        if (command is ActionCommand actionCommand)
        {
            actionCommand.TextChanged += (_, _) => SetItemText();
        }
        // TODO: We want a better way of determining which keyboard shortcuts are worth showing
        // Ideally we could show them all, but it can be really distracting. So only showing F2/F3 etc. right now.
        if (item is WF.ToolStripMenuItem menuItem && !command.Shortcut.ToString().Contains(","))
        {
            var swfKeys = command.Shortcut.ToSWF();
            if (WF.ToolStripManager.IsValidShortcut(swfKeys))
            {
                menuItem.ShortcutKeys = swfKeys;
            }
        }
        command.EnabledChanged += (_, _) => item.Enabled = command.Enabled;
        if (item is WF.ToolStripSplitButton button)
        {
            button.ButtonClick += (_, _) => command.Execute();
        }
        else
        {
            item.Click += (_, _) => command.Execute();
        }
        return item;
    }

    protected override void CreateToolbarSeparator()
    {
        _mainToolStrip.Items.Add(new WF.ToolStripSeparator());
    }

    public override void ShowToolbarMenu(DesktopToolbarMenuType menuType)
    {
        _menuButtons.Get(menuType)?.ShowDropDown();
    }

    private void SaveToolStripLocation()
    {
        Config.User.Set(c => c.DesktopToolStripDock, _mainToolStrip.Parent!.Dock.ToConfig());
        Config.User.Set(c => c.ProfilesToolStripDock, _profilesToolStrip.Parent?.Dock.ToConfig() ?? DockStyle.Top);
    }

    private void LoadToolStripLocation()
    {
        SetDock(_mainToolStrip, Config.Get(c => c.DesktopToolStripDock));
        if (_profilesToolStrip.Parent != null)
        {
            SetDock(_profilesToolStrip, Config.Get(c => c.ProfilesToolStripDock));
        }
    }

    private void SetDock(WF.ToolStrip toolStrip, DockStyle dock)
    {
        var wfDock = dock.ToWinForms();
        var panel = _container.Controls.OfType<WF.ToolStripPanel>().FirstOrDefault(x => x.Dock == wfDock);
        if (panel != null)
        {
            toolStrip.Parent = panel;
        }
        toolStrip.Parent!.TabStop = true;
    }
}