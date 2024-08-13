using Eto.Forms;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Mac;
using NAPS2.EtoForms.Notifications;
using NAPS2.EtoForms.Widgets;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Ui;

public class MacDesktopForm : DesktopForm
{
    private NSSlider? _zoomSlider;

    public MacDesktopForm(
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
        Sidebar sidebar)
        : base(config, keyboardShortcuts, notificationManager, cultureHelper, colorScheme, profileManager, imageList,
            thumbnailController, thumbnailProvider, desktopController, desktopScanController, imageListActions,
            imageListViewBehavior, desktopFormProvider, desktopSubFormController, commands, sidebar)
    {
        // For retina screens
        _thumbnailController.Oversample = 2.0;
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

        Menu = new MenuBar
        {
            AboutItem = Commands.About,
            ApplicationItems =
            {
                Commands.Settings,
                CreateSubMenu(Commands.LanguageMenu, GetLanguageMenuProvider())
            },
            Items =
            {
                new SubMenuItem
                {
                    // TODO: Localize this?
                    Text = "File",
                    Items =
                    {
                        Commands.Import,
                        new SeparatorMenuItem(),
                        Commands.SaveAll,
                        Commands.SaveSelected,
                        new SeparatorMenuItem(),
                        Commands.EmailAll,
                        Commands.EmailSelected,
                        Commands.Print,
                        new SeparatorMenuItem(),
                        Commands.PdfSettings,
                        Commands.ImageSettings,
                        Commands.EmailSettings,
                        new SeparatorMenuItem(),
                        Commands.ClearAll
                    }
                },
                new SubMenuItem
                {
                    // The space makes sure this doesn't match the default "Edit" menu
                    Text = UiStrings.Edit + " ",
                    Items =
                    {
                        Commands.SelectAll,
                        Commands.Copy,
                        Commands.Paste,
                        new SeparatorMenuItem(),
                        Commands.Undo,
                        Commands.Redo,
                        new SeparatorMenuItem(),
                        Commands.Delete
                    }
                },
                CreateSubMenu(Commands.Scan, new MenuProvider()
                    .Dynamic(_scanMenuCommands)
                    .Separator()
                    .Append(Commands.NewProfile)
                    .Append(Commands.BatchScan)),
                new SubMenuItem
                {
                    Text = UiStrings.Image,
                    Items =
                    {
                        Commands.ViewImage,
                        Commands.ZoomIn,
                        Commands.ZoomOut,
                        new SeparatorMenuItem(),
                        Commands.Crop,
                        Commands.BrightCont,
                        Commands.HueSat,
                        Commands.BlackWhite,
                        Commands.Sharpen,
                        Commands.DocumentCorrection,
                        new SeparatorMenuItem(),
                        Commands.Split,
                        Commands.Combine,
                        new SeparatorMenuItem(),
                        Commands.RotateLeft,
                        Commands.RotateRight,
                        Commands.Flip,
                        Commands.Deskew,
                        Commands.CustomRotate,
                        new SeparatorMenuItem(),
                        Commands.ResetImage
                    }
                },
                new SubMenuItem
                {
                    Text = UiStrings.Reorder,
                    Items =
                    {
                        Commands.MoveUp,
                        Commands.MoveDown,
                        new SeparatorMenuItem(),
                        Commands.Interleave,
                        Commands.Deinterleave,
                        new SeparatorMenuItem(),
                        Commands.AltInterleave,
                        Commands.AltDeinterleave,
                        new SeparatorMenuItem(),
                        Commands.ReverseAll,
                        Commands.ReverseSelected
                    }
                },
                new SubMenuItem
                {
                    Text = UiStrings.Tools,
                    Items =
                    {
                        Commands.Profiles,
                        Commands.BatchScan,
                        Commands.ScannerSharing,
                        Commands.Ocr
                    }
                }
            }
        };

        // TODO: If we ever make significant changes to the toolbar layout, maybe add ".v2" so it resets saved config
        var toolbar = new NSToolbar("naps2.desktop.toolbar");
        toolbar.Delegate = new MacToolbarDelegate(CreateMacToolbarItems());
        toolbar.AllowsUserCustomization = true;
        toolbar.AutosavesConfiguration = true;

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
        _zoomSlider = new NSSlider
        {
            MinValue = 0,
            MaxValue = 1,
            DoubleValue = ThumbnailSizes.SizeToCurve(_thumbnailController.VisibleSize),
            ToolTip = UiStrings.Zoom
        };
        _zoomSlider.WithAction(() =>
            _thumbnailController.VisibleSize = ThumbnailSizes.CurveToSize(_zoomSlider.DoubleValue));
        _thumbnailController.ThumbnailSizeChanged += ThumbnailController_ThumbnailSizeChanged;

        return
        [
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
            MacToolbarItems.Create("sidebar", Commands.ToggleSidebar, tooltip: UiStrings.ToggleSidebar, nav: true),
            new NSToolbarItem("zoom")
            {
                View = _zoomSlider,
                // MaxSize still works even though it's deprecated
#pragma warning disable CA1416
#pragma warning disable CA1422
                MaxSize = new CGSize(64, 24)
#pragma warning restore CA1422
#pragma warning restore CA1416
            }
        ];
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _thumbnailController.ThumbnailSizeChanged -= ThumbnailController_ThumbnailSizeChanged;
    }

    private void ThumbnailController_ThumbnailSizeChanged(object? o, EventArgs eventArgs)
    {
        _zoomSlider!.DoubleValue = ThumbnailSizes.SizeToCurve(_thumbnailController.VisibleSize);
    }

    protected override LayoutElement GetZoomButtons() => C.Spacer();
}