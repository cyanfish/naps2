using System.Threading;
using Eto.Forms;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Mac;
using NAPS2.ImportExport.Images;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Ui;

public class MacDesktopForm : DesktopForm
{
    public MacDesktopForm(
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
        // For retina screens
        _thumbnailController.Oversample = 2.0;
    }

    protected override void OnLoad(EventArgs e)
    {
        // TODO: What's the best place to initialize this? It needs to happen from the UI event loop.
        Invoker.Current = new SyncContextInvoker(SynchronizationContext.Current!);
        base.OnLoad(e);
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
                        Commands.SaveAll,
                        Commands.SaveSelected,
                        // TODO: Implement print/email on Mac/Linux
                        // new SeparatorMenuItem(),
                        // Commands.EmailAll,
                        // Commands.EmailSelected,
                        // Commands.Print,
                        new SeparatorMenuItem(),
                        Commands.PdfSettings,
                        Commands.ImageSettings,
                        // Commands.EmailSettings,
                        new SeparatorMenuItem(),
                        Commands.ClearAll
                    }
                },
                new SubMenuItem
                {
                    // The space makes sure this doesn't match the default "Edit" menu
                    Text = "Edit ",
                    Items =
                    {
                        Commands.SelectAll,
                        Commands.Copy,
                        Commands.Paste,
                        Commands.Delete
                    }
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
        toolbar.Delegate = new MacToolbarDelegate(CreateMacToolbarItems());
        toolbar.AllowsUserCustomization = true;
        // toolbar.AutosavesConfiguration = true;

        var window = this.ToNative();
        if (OperatingSystem.IsMacOSVersionAtLeast(11))
        {
            toolbar.DisplayMode = NSToolbarDisplayMode.Icon;
            window.ToolbarStyle = NSWindowToolbarStyle.Unified;
            window.StyleMask |= NSWindowStyle.FullSizeContentView;
            window.StyleMask |= NSWindowStyle.UnifiedTitleAndToolbar;
        }
        else
        {
            toolbar.DisplayMode = NSToolbarDisplayMode.IconAndLabel;
        }

        window.Toolbar = toolbar;
    }

    private List<NSToolbarItem?> CreateMacToolbarItems()
    {
        return new List<NSToolbarItem?>
        {
            MacToolbarItems.Create("scan", Commands.Scan),
            MacToolbarItems.Create("profiles", Commands.Profiles),
            MacToolbarItems.CreateSpace(),
            MacToolbarItems.Create("import", Commands.Import),
            MacToolbarItems.Create("save", Commands.SaveAll),
            MacToolbarItems.CreateSpace(),
            MacToolbarItems.Create("viewer", Commands.ViewImage),
            MacToolbarItems.CreateMenu("rotate", Commands.RotateMenu, GetRotateMenuProvider()),
            MacToolbarItems.Create("moveUp", Commands.MoveUp, tooltip: UiStrings.MoveUp),
            MacToolbarItems.Create("moveDown", Commands.MoveDown, tooltip: UiStrings.MoveDown),
            new NSToolbarItem("zoom")
            {
                View = new NSSlider
                {
                    MinValue = 0,
                    MaxValue = 1,
                    DoubleValue = ThumbnailSizes.SizeToCurve(_thumbnailController.VisibleSize),
                    ToolTip = UiStrings.Zoom
                }.WithAction(ZoomUpdated),
                // MaxSize still works even though it's deprecated
#pragma warning disable CA1416
#pragma warning disable CA1422
                MaxSize = new CGSize(64, 24)
#pragma warning restore CA1422
#pragma warning restore CA1416
            }
        };
    }

    protected override LayoutElement GetZoomButtons() => C.Spacer();

    private void ZoomUpdated(NSSlider sender)
    {
        _thumbnailController.VisibleSize = ThumbnailSizes.CurveToSize(sender.DoubleValue);
    }
}