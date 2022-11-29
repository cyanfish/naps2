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
        Invoker.Current = new SyncContextInvoker(SynchronizationContext.Current);
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
        toolbar.Delegate = new MacToolbarDelegate(CreateMacToolbarItems());
        toolbar.AllowsUserCustomization = true;
        // toolbar.AutosavesConfiguration = true;
        toolbar.DisplayMode = NSToolbarDisplayMode.Icon;

        // TODO: Get rid of the borders/excessive padding on macOS 13
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

    private List<NSToolbarItem> CreateMacToolbarItems()
    {
        return new List<NSToolbarItem>
        {
            MacToolbarItems.Create("scan", Commands.Scan, UiStrings.Scan),
            MacToolbarItems.Create("profiles", Commands.Profiles, UiStrings.Profiles),
            MacToolbarItems.Create("import", Commands.Import, UiStrings.Import),
            MacToolbarItems.CreateMenu("save", Commands.Save, new MenuProvider()
                    .Append(Commands.SaveAllPdf)
                    .Append(Commands.SaveSelectedPdf)
                    .Append(Commands.SaveAllImages)
                    .Append(Commands.SaveSelectedImages),
                UiStrings.Save),
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
                    ToolTip = UiStrings.Zoom,
                    Title = UiStrings.Zoom
                }.WithAction(ZoomUpdated),
                MaxSize = new CGSize(64, 999)
            }
        };
    }

    protected override LayoutElement GetZoomButtons() => C.Spacer();

    private void ZoomUpdated(NSSlider sender)
    {
        _thumbnailController.VisibleSize = ThumbnailSizes.CurveToSize(sender.DoubleValue);
    }
}