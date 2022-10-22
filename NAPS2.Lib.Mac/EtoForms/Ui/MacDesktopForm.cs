using System.Threading;
using Eto.Drawing;
using Eto.Forms;
using Eto.Mac;
using NAPS2.EtoForms.Desktop;
using NAPS2.ImportExport.Images;
using NAPS2.Scan;

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
        // For retina screens
        _thumbnailController.Oversample = 2.0;
    }

    protected override void OnLoad(EventArgs e)
    {
        // TODO: What's the best place to initialize this? It needs to happen from the UI event loop.
        Invoker.Current = new SyncContextInvoker(SynchronizationContext.Current);
        base.OnLoad(e);
        ClientSize = new Size(1000, 600);
    }

    protected override void UpdateTitle(ScanProfile? defaultProfile)
    {
        Title = UiStrings.Naps2;
        if (OperatingSystem.IsMacOSVersionAtLeast(11))
        {
            this.ToNative().Subtitle = defaultProfile?.DisplayName ?? UiStrings.Naps2FullName;
        }
    }

    protected override void CreateToolbarsAndMenus()
    {
        Commands.MoveDown.ToolBarText = "";
        Commands.MoveUp.ToolBarText = "";
        Commands.SaveAllPdf.Shortcut = Application.Instance.CommonModifier | Keys.S;
        Commands.SaveSelectedPdf.Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.S;
        Commands.SaveAllImages.Shortcut = Application.Instance.CommonModifier | Keys.M;
        Commands.SaveSelectedImages.Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.M;

        Menu = new MenuBar
        {
            AboutItem = Commands.About,
            ApplicationItems =
            {
                CreateSubMenu(Commands.LanguageMenu, GetLanguageMenuProvider())
            },
            Items =
            {
                new SubMenuItem
                {
                    Text = "File",
                    Items =
                    {
                        Commands.Import,
                        new SeparatorMenuItem(),
                        Commands.SaveAllPdf,
                        Commands.SaveSelectedPdf,
                        Commands.SaveAllImages,
                        Commands.SaveSelectedImages,
                        new SeparatorMenuItem(),
                        Commands.EmailAllPdf,
                        Commands.EmailSelectedPdf,
                        Commands.Print,
                        new SeparatorMenuItem(),
                        Commands.ClearAll
                    }
                },
                new SubMenuItem
                {
                    Text = "Edit"
                },
                new SubMenuItem
                {
                    Text = "Scan",
                    Items =
                    {
                        Commands.Scan,
                        Commands.NewProfile
                    }
                },
                new SubMenuItem
                {
                    Text = "Image",
                    Items =
                    {
                        Commands.ViewImage,
                        new SeparatorMenuItem(),
                        Commands.Crop,
                        Commands.BrightCont,
                        Commands.HueSat,
                        Commands.BlackWhite,
                        Commands.Sharpen,
                        Commands.DocumentCorrection,
                        new SeparatorMenuItem(),
                        Commands.ResetImage
                    }
                },
                new SubMenuItem
                {
                    Text = "Tools",
                    Items =
                    {
                        Commands.BatchScan,
                        Commands.Ocr
                    }
                }
            }
        };

        var toolbar = new NSToolbar("naps2.desktop.toolbar");
        toolbar.Delegate = new ToolbarDelegate(this);
        toolbar.AllowsUserCustomization = true;
        // toolbar.AutosavesConfiguration = true;
        toolbar.DisplayMode = NSToolbarDisplayMode.Icon;

        var window = this.ToNative();
        window.Toolbar = toolbar;
        if (OperatingSystem.IsMacOSVersionAtLeast(11))
        {
            window.ToolbarStyle = NSWindowToolbarStyle.Unified;
        }
        // TODO: Do we want full size content?
        window.StyleMask |= NSWindowStyle.FullSizeContentView;
        window.StyleMask |= NSWindowStyle.UnifiedTitleAndToolbar;
    }

    private void ZoomUpdated(NSSlider sender)
    {
        _thumbnailController.VisibleSize = ThumbnailSizes.CurveToSize(sender.DoubleValue);
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
            return new[] { "scan", "profiles", "import", "save", "viewer", "rotate", "moveUp", "moveDown", "zoom" };
        }

        public override string[] DefaultItemIdentifiers(NSToolbar toolbar)
        {
            return new[] { "scan", "profiles", "import", "save", "viewer", "rotate", "moveUp", "moveDown", "zoom" };
        }

        public override string[] SelectableItemIdentifiers(NSToolbar toolbar)
        {
            return new[] { "scan", "profiles", "import", "save", "viewer", "rotate", "moveUp", "moveDown", "zoom" };
        }

        public override NSToolbarItem? WillInsertItem(NSToolbar toolbar, string itemIdentifier, bool willBeInserted)
        {
            return itemIdentifier switch
            {
                "scan" => CreateToolbarItem(_form.Commands.Scan, UiStrings.Scan),
                "profiles" => CreateToolbarItem(_form.Commands.Profiles, UiStrings.Profiles),
                "import" => CreateToolbarItem(_form.Commands.Import, UiStrings.Import),
                "save" => CreateNsToolbarMenu(_form.Commands.Save, new MenuProvider()
                        .Append(_form.Commands.SaveAllPdf)
                        .Append(_form.Commands.SaveSelectedPdf)
                        .Append(_form.Commands.SaveAllImages)
                        .Append(_form.Commands.SaveSelectedImages),
                    UiStrings.Save),
                "viewer" => CreateToolbarItem(_form.Commands.ViewImage),
                "rotate" => CreateNsToolbarMenu(_form.Commands.RotateMenu, _form.GetRotateMenuProvider()),
                "moveUp" => CreateToolbarItem(_form.Commands.MoveUp, tooltip: UiStrings.MoveUp),
                "moveDown" => CreateToolbarItem(_form.Commands.MoveDown, tooltip: UiStrings.MoveDown),
                "zoom" => new NSToolbarItem
                {
                    View = new NSSlider
                    {
                        MinValue = 0,
                        MaxValue = 1,
                        DoubleValue = ThumbnailSizes.SizeToCurve(_form._thumbnailController.VisibleSize),
                        ToolTip = UiStrings.Zoom,
                        Title = UiStrings.Zoom
                    }.WithAction(_form.ZoomUpdated),
                    MaxSize = new CGSize(64, 999)
                },
                _ => null
            };
        }

        private NSToolbarItem CreateNsToolbarMenu(Command menuCommand, MenuProvider menuProvider,
            string? title = null, string? tooltip = null)
        {
            return new NSMenuToolbarItem
            {
                Image = GetMacSymbol(menuCommand, true),
                Label = menuCommand.ToolBarText ?? "",
                Title = title ?? "",
                ToolTip = tooltip ?? menuCommand.ToolBarText ?? "",
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

        private NSToolbarItem CreateToolbarItem(Command command, string? title = null, string? tooltip = null)
        {
            return new NSToolbarItem
            {
                Image = GetMacSymbol(command, true),
                Title = title ?? "",
                Label = command.ToolBarText ?? "",
                ToolTip = tooltip ?? command.ToolBarText ?? "",
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
            return OperatingSystem.IsMacOSVersionAtLeast(11)
                ? NSImage.GetSystemSymbol(symbol, null)
                : command.Image.ToNS();
        }
    }
}