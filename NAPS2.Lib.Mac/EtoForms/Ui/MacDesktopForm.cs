using System.Threading;
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

    protected override void SetContent(Control content)
    {
        var scrollView = new NSScrollView();
        scrollView.DocumentView = content.ToNative();
        Content = scrollView.ToEto();
    }

    protected override void OnLoad(EventArgs e)
    {
        // TODO: What's the best place to initialize this? It needs to happen from the UI event loop.
        Invoker.Current = new SyncContextInvoker(SynchronizationContext.Current);
        base.OnLoad(e);
        ClientSize = new Size(1000, 600);
        // TODO: Initialize everything that needs to be initialized where it's best
        ResizeThumbnails(Config.ThumbnailSize());
    }

    protected override void CreateToolbarsAndMenus()
    {
        _moveDownCommand.ToolBarText = "";
        _moveUpCommand.ToolBarText = "";
        _saveAllPdfCommand.Shortcut = Application.Instance.CommonModifier | Keys.S;
        _saveSelectedPdfCommand.Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.S;
        _saveAllImagesCommand.Shortcut = Application.Instance.CommonModifier | Keys.M;
        _saveSelectedImagesCommand.Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.M;

        Menu = new MenuBar
        {
            AboutItem = _aboutCommand,
            ApplicationItems =
            {
                CreateSubMenu(_languageMenuCommand, GetLanguageMenuProvider())
            },
            Items =
            {
                new SubMenuItem
                {
                    Text = "File",
                    Items =
                    {
                        _importCommand,
                        new SeparatorMenuItem(),
                        _saveAllPdfCommand,
                        _saveSelectedPdfCommand,
                        _saveAllImagesCommand,
                        _saveSelectedImagesCommand,
                        new SeparatorMenuItem(),
                        _emailAllPdfCommand,
                        _emailSelectedPdfCommand,
                        _printCommand,
                        new SeparatorMenuItem(),
                        _clearAllCommand
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
                        _scanCommand,
                        _newProfileCommand
                    }
                },
                new SubMenuItem
                {
                    Text = "Image",
                    Items =
                    {
                        _viewImageCommand,
                        new SeparatorMenuItem(),
                        _cropCommand,
                        _brightContCommand,
                        _hueSatCommand,
                        _blackWhiteCommand,
                        _sharpenCommand,
                        new SeparatorMenuItem(),
                        _resetImageCommand
                    }
                },
                new SubMenuItem
                {
                    Text = "Tools",
                    Items =
                    {
                        _batchScanCommand,
                        _ocrCommand
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
        window.ToolbarStyle = NSWindowToolbarStyle.Unified;
        // TODO: Subtitle based on active profile?
        window.Subtitle = "Not Another PDF Scanner 2";
        // TODO: Do we want full size content?
        window.StyleMask |= NSWindowStyle.FullSizeContentView;
        window.StyleMask |= NSWindowStyle.UnifiedTitleAndToolbar;
    }

    private void ZoomUpdated(NSSlider sender)
    {
        var size = ThumbnailSizes.CurveToSize(sender.DoubleValue);
        ResizeThumbnails(size);
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
                "scan" => CreateToolbarItem(_form._scanCommand, UiStrings.Scan),
                "profiles" => CreateToolbarItem(_form._profilesCommand, UiStrings.Profiles),
                "import" => CreateToolbarItem(_form._importCommand, UiStrings.Import),
                "save" => CreateNsToolbarMenu(_form._saveCommand, new MenuProvider()
                        .Append(_form._saveAllPdfCommand)
                        .Append(_form._saveSelectedPdfCommand)
                        .Append(_form._saveAllImagesCommand)
                        .Append(_form._saveSelectedImagesCommand),
                    UiStrings.Save),
                "viewer" => CreateToolbarItem(_form._viewImageCommand),
                "rotate" => CreateNsToolbarMenu(_form._rotateMenuCommand, _form.GetRotateMenuProvider()),
                "moveUp" => CreateToolbarItem(_form._moveUpCommand, tooltip: UiStrings.MoveUp),
                "moveDown" => CreateToolbarItem(_form._moveDownCommand, tooltip: UiStrings.MoveDown),
                "zoom" => new NSToolbarItem
                {
                    View = new NSSlider
                    {
                        MinValue = 0,
                        MaxValue = 1,
                        DoubleValue = ThumbnailSizes.SizeToCurve(_form.Config.ThumbnailSize()),
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
            return NSImage.GetSystemSymbol(symbol, null) ?? command.Image.ToNS();
        }
    }
}