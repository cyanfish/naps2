using System.ComponentModel;
using System.Drawing;
using Eto.Forms;
using Eto.WinForms;
using Eto.WinForms.Forms.ToolBar;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.WinForms;
using NAPS2.ImportExport.Images;
using NAPS2.WinForms;
using wf = System.Windows.Forms;

namespace NAPS2.EtoForms.Ui;

public class WinFormsDesktopForm : DesktopForm
{
    public static wf.ApplicationContext? ApplicationContext { get; set; }

    private readonly Dictionary<DesktopToolbarMenuType, wf.ToolStripSplitButton> _menuButtons = new();
    private readonly ToolbarFormatter _toolbarFormatter = new(new StringWrapper());
    private readonly wf.Form _form;
    private wf.ToolStrip _toolStrip = null!;
    private wf.ToolStripContainer _container = null!;

    public WinFormsDesktopForm(
        Naps2Config config,
        DesktopKeyboardShortcuts keyboardShortcuts,
        INotificationManager notify,
        CultureHelper cultureHelper,
        IProfileManager profileManager,
        UiImageList imageList,
        ImageTransfer imageTransfer,
        ThumbnailController thumbnailController,
        UiThumbnailProvider thumbnailProvider,
        DesktopController desktopController,
        IDesktopScanController desktopScanController,
        ImageListActions imageListActions,
        DesktopFormProvider desktopFormProvider,
        IDesktopSubFormController desktopSubFormController,
        DesktopCommands commands)
        : base(config, keyboardShortcuts, notify, cultureHelper, profileManager,
            imageList, imageTransfer, thumbnailController, thumbnailProvider, desktopController, desktopScanController,
            imageListActions, desktopFormProvider, desktopSubFormController, commands)
    {
        _form = this.ToNative();
        _form.FormClosing += OnFormClosing;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // Don't do anything here as we have a separate FormClosing event handler
        // That allows us to check the close reason (which Eto doesn't provide)
    }

    private void OnFormClosing(object? sender, wf.FormClosingEventArgs e)
    {
        if (!_desktopController.PrepareForClosing(e.CloseReason == wf.CloseReason.UserClosing))
        {
            e.Cancel = true;
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        NativeListView.TabIndex = 7;
        NativeListView.Dock = wf.DockStyle.Fill;
        // NativeListView.ContextMenuStrip = contextMenuStrip;
        // NativeListView.KeyDown += ListViewKeyDown;
        // NativeListView.MouseWheel += ListViewMouseWheel;
        NativeListView.Focus();
    }

    protected override void OnShown(EventArgs e)
    {
        _toolbarFormatter.RelayoutToolbar(_toolStrip);
        base.OnShown(e);
    }

    protected override LayoutElement GetZoomButtons()
    {
        // Disabled buttons don't prevent click events from being sent to the listview below the button, so without this
        // "mouse catcher" control you could e.g. spam click zoom out until it's maxed and then accidentally keep
        // clicking and change the listview selection.
        var mouseCatcher = new wf.Button
        {
            BackColor = Color.White,
            Size = new Size(45, 23),
            FlatStyle = wf.FlatStyle.Flat
        };
        return L.Overlay(
            mouseCatcher.ToEto(),
            base.GetZoomButtons()
        );
    }

    private wf.ListView NativeListView => ((WinFormsListView<UiImage>) _listView).NativeControl;

    protected override void SetMainForm(Form newMainForm)
    {
        base.SetMainForm(newMainForm);
        if (ApplicationContext == null)
        {
            Log.Error("ApplicationContext should not be null");
            return;
        }
        ApplicationContext.MainForm = newMainForm.ToSWF();
    }

    protected override void ConfigureToolbar()
    {
        _toolStrip = ((ToolBarHandler) ToolBar.Handler).Control;
        _toolStrip.ShowItemToolTips = false;
        _toolStrip.TabStop = true;
        _toolStrip.ImageScalingSize = new Size(32, 32);
        _toolStrip.ParentChanged += (_, _) => _toolbarFormatter.RelayoutToolbar(_toolStrip);
    }

    protected override LayoutElement GetMainContent()
    {
        _container = new wf.ToolStripContainer();
        _container.TopToolStripPanel.Controls.Add(_toolStrip);
        foreach (var panel in _container.Controls.OfType<wf.ToolStripPanel>())
        {
            // Allow tabbing through the toolbar for accessibility
            WinFormsHacks.SetControlStyle(panel, wf.ControlStyles.Selectable, true);
        }

        var wfContent = _listView.Control.ToNative();
        wfContent.Dock = wf.DockStyle.Fill;
        _container.ContentPanel.Controls.Add(wfContent);

        return _container.ToEto();
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
        var item = new wf.ToolStripButton
        {
            TextImageRelation = wf.TextImageRelation.ImageAboveText
        };
        ApplyCommand(item, command);
        _toolStrip.Items.Add(item);
    }

    protected override void CreateToolbarButtonWithMenu(Command command, DesktopToolbarMenuType menuType,
        MenuProvider menu)
    {
        var item = new wf.ToolStripSplitButton
        {
            TextImageRelation = wf.TextImageRelation.ImageAboveText
        };
        ApplyCommand(item, command);
        _toolStrip.Items.Add(item);
        menu.Handle(subItems => SetUpMenu(item, subItems));
        _menuButtons[menuType] = item;
    }

    private void SetUpMenu(wf.ToolStripDropDownItem item, List<MenuProvider.Item> subItems)
    {
        item.DropDownItems.Clear();
        foreach (var subItem in subItems)
        {
            switch (subItem)
            {
                case MenuProvider.SeparatorItem:
                    item.DropDownItems.Add(new wf.ToolStripSeparator());
                    break;
                case MenuProvider.CommandItem commandSubItem:
                    item.DropDownItems.Add(ApplyCommand(new wf.ToolStripMenuItem
                    {
                        ImageScaling = wf.ToolStripItemImageScaling.None
                    }, commandSubItem.Command));
                    break;
                case MenuProvider.SubMenuItem subMenuSubItem:
                    var subMenu = new wf.ToolStripMenuItem();
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
        var item = new wf.ToolStripDropDownButton
        {
            TextImageRelation = wf.TextImageRelation.ImageAboveText,
            ShowDropDownArrow = false
        };
        ApplyCommand(item, command);
        _toolStrip.Items.Add(item);
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
        _toolStrip.Items.Add(item);
    }

    private wf.ToolStripItem ApplyCommand(wf.ToolStripItem item, Command command)
    {
        void SetItemText() => item.Text = item is wf.ToolStripMenuItem ? command.MenuText : command.ToolBarText;
        item.Image = command.Image.ToSD();
        SetItemText();
        if (command is ActionCommand actionCommand)
        {
            actionCommand.TextChanged += (_, _) => SetItemText();
        }
        // TODO: We want a better way of determining which keyboard shortcuts are worth showing
        // Ideally we could show them all, but it can be really distracting. So only showing F2/F3 etc. right now.
        if (item is wf.ToolStripMenuItem menuItem && !command.Shortcut.ToString().Contains(","))
        {
            menuItem.ShortcutKeys = command.Shortcut.ToSWF();
        }
        command.EnabledChanged += (_, _) => item.Enabled = command.Enabled;
        if (item is wf.ToolStripSplitButton button)
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
        _toolStrip.Items.Add(new wf.ToolStripSeparator());
    }

    public override void ShowToolbarMenu(DesktopToolbarMenuType menuType)
    {
        _menuButtons.Get(menuType)?.ShowDropDown();
    }


    // TODO: Call these

    private void SaveToolStripLocation()
    {
        Config.User.Set(c => c.DesktopToolStripDock, _toolStrip.Parent.Dock.ToConfig());
    }

    private void LoadToolStripLocation()
    {
        var dock = Config.Get(c => c.DesktopToolStripDock).ToWinForms();
        if (dock != wf.DockStyle.None)
        {
            var panel = _container.Controls.OfType<wf.ToolStripPanel>().FirstOrDefault(x => x.Dock == dock);
            if (panel != null)
            {
                _toolStrip.Parent = panel;
            }
        }
        _toolStrip.Parent.TabStop = true;
    }
}