using System.Threading;
using Eto.GtkSharp;
using Eto.GtkSharp.Forms.ToolBar;
using Gdk;
using Gtk;
using NAPS2.EtoForms.Gtk;
using NAPS2.ImportExport.Images;
using NAPS2.WinForms;
using Command = Eto.Forms.Command;

namespace NAPS2.EtoForms.Ui;

public class GtkDesktopForm : DesktopForm
{
    private Toolbar _toolbar;

    public GtkDesktopForm(
        Naps2Config config,
        // KeyboardShortcutManager ksm,
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
        : base(config, /*ksm,*/ notify, cultureHelper, profileManager,
            imageList, imageTransfer, thumbnailController, thumbnailProvider, desktopController, desktopScanController,
            imageListActions, desktopFormProvider, desktopSubFormController, commands)
    {
    }

    protected override void OnLoad(EventArgs e)
    {
        // TODO: What's the best place to initialize this? It needs to happen from the UI event loop.
        Invoker.Current = new SyncContextInvoker(SynchronizationContext.Current);
        base.OnLoad(e);
        ClientSize = new Eto.Drawing.Size(1000, 600);
        // TODO: This is a bit of a hack as for some reason the view doesn't update unless we do this
        ((GtkListView<UiImage>) _listView).Updated += (_, _) => Content = _listView.Control;
    }

    protected override void ConfigureToolbar()
    {
        _toolbar = ((ToolBarHandler) ToolBar.Handler).Control;
        _toolbar.Style = ToolbarStyle.Both;
    }

    protected override void CreateToolbarButton(Command command)
    {
        var button = new ToolButton(command.Image.ToGtk(), command.ToolBarText)
        {
            Homogeneous = false
        };
        button.StyleContext.AddProvider(new StyleProperties()
        {
            Data = { { "padding", "0" } }
        }, 0);
        button.Sensitive = command.Enabled;
        command.EnabledChanged +=
            (_, _) => button.Sensitive = command.Enabled;
        button.Clicked += (_, _) => command.Execute();
        _toolbar.Add(button);
    }

    protected override void CreateToolbarSeparator()
    {
        _toolbar.Add(new SeparatorToolItem());
    }

    protected override void CreateToolbarStackedButtons(Command command1, Command command2)
    {
        var toolItem = new ToolItem();
        var box = new Box(Orientation.Vertical, 16);
        box.Add(Icons.arrow_up_small.ToEtoImage().ToGtk());
        box.Add(Icons.arrow_down_small.ToEtoImage().ToGtk());
        toolItem.Add(box);
        _toolbar.Add(toolItem);
    }

    protected override void CreateToolbarButtonWithMenu(Command command, MenuProvider menu)
    {
        var button = new MenuToolButton(command.Image.ToGtk(), command.ToolBarText)
        {
            Homogeneous = false,
            Menu = CreateMenuWidget(menu)
        };
        button.Sensitive = command.Enabled;
        command.EnabledChanged +=
            (_, _) => button.Sensitive = command.Enabled;
        button.Clicked += (_, _) => command.Execute();
        _toolbar.Add(button);
    }

    private Menu CreateMenuWidget(MenuProvider menu)
    {
        var menuWidget = new Menu();
        menu.Handle(items =>
        {
            foreach (var child in menuWidget.Children)
            {
                menuWidget.Remove(child);
            }
            foreach (var item in items)
            {
                switch (item)
                {
                    case MenuProvider.CommandItem commandItem:
                        var menuItem = new MenuItem
                        {
                            Label = commandItem.Command.MenuText
                        };
                        menuItem.Sensitive = commandItem.Command.Enabled;
                        commandItem.Command.EnabledChanged +=
                            (_, _) => menuItem.Sensitive = commandItem.Command.Enabled;
                        menuItem.Activated += (_, _) => commandItem.Command.Execute();
                        menuWidget.Add(menuItem);
                        break;
                    case MenuProvider.SeparatorItem:
                        menuWidget.Add(new SeparatorMenuItem());
                        break;
                    case MenuProvider.SubMenuItem subMenuItem:
                        menuWidget.Add(CreateMenuWidget(subMenuItem.MenuProvider));
                        break;
                }
            }
        });
        return menuWidget;
    }

    protected override void CreateToolbarMenu(Command command, MenuProvider menu)
    {
        var button = new ToolButton(command.Image.ToGtk(), command.ToolBarText)
        {
            Homogeneous = false
        };
        var menuWidget = CreateMenuWidget(menu);
        button.Clicked += (_, _) => menuWidget.PopupAtWidget(button, Gravity.SouthWest, Gravity.NorthWest, null);
        button.Sensitive = command.Enabled;
        command.EnabledChanged +=
            (_, _) => button.Sensitive = command.Enabled;
        _toolbar.Add(button);
    }
}