using Eto.Drawing;
using Eto.Forms;
using Eto.Mac;
using NAPS2.ImportExport.Images;
using NAPS2.WinForms;

namespace NAPS2.EtoForms.Ui;

public class MacDesktopForm : DesktopForm
{
    public MacDesktopForm(
        Naps2Config config,
        // KeyboardShortcutManager ksm,
        INotificationManager notify,
        CultureHelper cultureHelper,
        IProfileManager profileManager,
        UiImageList imageList,
        ImageTransfer imageTransfer,
        ThumbnailRenderQueue thumbnailRenderQueue,
        UiThumbnailProvider thumbnailProvider,
        DesktopController desktopController,
        IDesktopScanController desktopScanController,
        ImageListActions imageListActions,
        DesktopFormProvider desktopFormProvider,
        IDesktopSubFormController desktopSubFormController)
        : base(config, /*ksm,*/ notify, cultureHelper, profileManager,
            imageList, imageTransfer, thumbnailRenderQueue, thumbnailProvider, desktopController, desktopScanController,
            imageListActions, desktopFormProvider, desktopSubFormController)
    {
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ClientSize = new Size(1000, 600);
    }

    protected override void CreateToolbarsAndMenus()
    {
        Menu = new MenuBar
        {
            AboutItem = _aboutCommand
        };

        var toolbar = new NSToolbar("naps2.desktop.toolbar");
        toolbar.Delegate = new ToolbarDelegate(this);
        toolbar.AllowsUserCustomization = true;
        // toolbar.AutosavesConfiguration = true;
        toolbar.DisplayMode = NSToolbarDisplayMode.Icon;

        var window = this.ToNative();
        window.Toolbar = toolbar;
        window.ToolbarStyle = NSWindowToolbarStyle.Unified;
        // TODO: Subtitle based on active profile?
        window.Subtitle = "Not Another PDF Scanner 2";
        window.StyleMask |= NSWindowStyle.FullSizeContentView;
        window.StyleMask |= NSWindowStyle.UnifiedTitleAndToolbar;
    }

    private void ZoomUpdated(NSSlider sender)
    {
        Config.User.Set(c => c.ThumbnailSize, sender.IntValue);
    }

    public class ToolbarDelegate : NSToolbarDelegate
    {
        private readonly MacDesktopForm _form;

        public ToolbarDelegate(MacDesktopForm form)
        {
            _form = form;
        }

        public NativeHandle Handle { get; }

        public void Dispose()
        {
        }

        public override string[] AllowedItemIdentifiers(NSToolbar toolbar)
        {
            return new[] { "scan", "profiles", "viewer", "rotate", "moveUp", "moveDown", "zoom" };
        }

        public override string[] DefaultItemIdentifiers(NSToolbar toolbar)
        {
            return new[] { "scan", "profiles", "viewer", "rotate", "moveUp", "moveDown", "zoom" };
        }

        public override string[] SelectableItemIdentifiers(NSToolbar toolbar)
        {
            return new[] { "scan", "profiles", "viewer", "rotate", "moveUp", "moveDown", "zoom" };
        }

        public override NSToolbarItem? WillInsertItem(NSToolbar toolbar, string itemIdentifier, bool willBeInserted)
        {
            return itemIdentifier switch
            {
                "scan" => CreateToolbarItem(_form._scanCommand),
                "profiles" => CreateToolbarItem(_form._profilesCommand),
                "viewer" => CreateToolbarItem(_form._viewImageCommand),
                "rotate" => CreateNsToolbarMenu(_form._rotateMenuCommand, _form.GetRotateMenuProvider()),
                "moveUp" => CreateToolbarItem(_form._moveUpCommand),
                "moveDown" => CreateToolbarItem(_form._moveDownCommand),
                "zoom" => new NSToolbarItem
                {
                    View = new NSSlider
                    {
                        MinValue = ThumbnailSizes.MIN_SIZE,
                        MaxValue = ThumbnailSizes.MAX_SIZE,
                        IntValue = _form.Config.ThumbnailSize()
                    }.WithAction(_form.ZoomUpdated),
                    MaxSize = new CGSize(64, 999)
                },
                _ => null
            };
        }

        private NSToolbarItem CreateNsToolbarMenu(Command menuCommand, MenuProvider menuProvider)
        {
            return new NSMenuToolbarItem
            {
                Image = GetMacSymbol(menuCommand, true),
                Menu = CreateMenu(menuProvider)
            };
        }

        private NSMenu CreateMenu(MenuProvider menuProvider)
        {
            var menu = new NSMenu();
            menuProvider.Handle(items =>
            {
                menu.RemoveAllItems();
                foreach (var item in items)
                {
                    switch (item)
                    {
                        case MenuProvider.CommandItem { Command: var command }:
                            menu.AddItem(new NSMenuItem
                            {
                                Title = command.MenuText,
                                Image = GetMacSymbol(command, false)
                            }.WithAction(command.Execute));
                            break;
                        case MenuProvider.SeparatorItem:
                            menu.AddItem(NSMenuItem.SeparatorItem);
                            break;
                        case MenuProvider.SubMenuItem:
                            throw new NotImplementedException();
                    }
                }
            });
            return menu;
        }

        private NSToolbarItem CreateToolbarItem(Command command)
        {
            return new NSToolbarItem
            {
                Image = GetMacSymbol(command, true),
                Bordered = true
            }.WithAction(command.Execute);
        }

        private NSImage? GetMacSymbol(Command command, bool required)
        {
            var symbol = (command as ActionCommand)?.MacSymbol;
            if (symbol == null)
            {
                return required ? throw new InvalidOperationException() : null;
            }
            // Fall back to the embedded NAPS2 icon if on an older mac version
            return NSImage.GetSystemSymbol(symbol, null) ?? command.Image.ToNS();
        }
    }
}