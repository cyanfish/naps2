using Eto.Forms;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Mac;

namespace NAPS2.EtoForms.Ui;

public class MacPreviewForm : PreviewForm
{
    private readonly NSSlider _zoomSlider;

    public MacPreviewForm(Naps2Config config, DesktopCommands desktopCommands, UiImageList imageList,
        IIconProvider iconProvider, ColorScheme colorScheme) : base(config,
        desktopCommands, imageList, iconProvider, colorScheme)
    {
        _zoomSlider = new NSSlider
        {
            MinValue = -2,
            MaxValue = 1,
            DoubleValue = 0,
            ToolTip = UiStrings.Zoom
        }.WithAction(ZoomUpdated);
        ImageViewer.ZoomChanged += (_, _) =>
        {
            _zoomSlider.DoubleValue = Math.Log10(ImageViewer.ZoomFactor);
            _zoomSlider.MaxValue = Math.Log10(ImageViewer.MaxZoom);
        };
    }

    protected override void CreateToolbar()
    {
        var toolbar = new NSToolbar("naps2.preview.toolbar");
        toolbar.Delegate = new MacToolbarDelegate(CreateMacToolbarItems());
        toolbar.AllowsUserCustomization = true;
        // toolbar.AutosavesConfiguration = true;
        toolbar.DisplayMode = NSToolbarDisplayMode.Icon;

        var window = this.ToNative();
        window.Toolbar = toolbar;
        if (OperatingSystem.IsMacOSVersionAtLeast(11))
        {
            window.ToolbarStyle = NSWindowToolbarStyle.Unified;
        }
        window.StyleMask |= NSWindowStyle.UnifiedTitleAndToolbar;
    }

    protected override void UpdatePage()
    {
        var window = this.ToNative();
        if (OperatingSystem.IsMacOSVersionAtLeast(11))
        {
            window.Subtitle = string.Format(UiStrings.XOfY, ImageIndex + 1, ImageList.Images.Count);
        }
    }

    private List<NSToolbarItem?> CreateMacToolbarItems()
    {
        return
        [
            MacToolbarItems.Create("prev", GoToPrevCommand, nav: true),
            MacToolbarItems.Create("next", GoToNextCommand, nav: true),
            MacToolbarItems.CreateMenu("rotate", Commands.RotateMenu, new MenuProvider()
                .Append(Commands.RotateLeft)
                .Append(Commands.RotateRight)
                .Append(Commands.Flip)
                .Append(Commands.Deskew)
                .Append(Commands.CustomRotate)),
            MacToolbarItems.Create("crop", Commands.Crop),
            MacToolbarItems.Create("brightcont", Commands.BrightCont),
            MacToolbarItems.Create("huesat", Commands.HueSat),
            MacToolbarItems.Create("blackwhite", Commands.BlackWhite),
            MacToolbarItems.Create("sharpen", Commands.Sharpen),
            MacToolbarItems.Create("documentcorrection", Commands.DocumentCorrection),
            MacToolbarItems.Create("split", Commands.Split),
            MacToolbarItems.Create("combine", Commands.Combine),
            MacToolbarItems.CreateSeparator("sep0"),
            MacToolbarItems.Create("save", Commands.SaveSelected),
            // TODO: Fix this
            // MacToolbarItems.CreateSeparator("sep1"),
            MacToolbarItems.Create("delete", DeleteCurrentImageCommand),
            // TODO: Using the slider is a bit janky
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

    private void ZoomUpdated(NSSlider sender)
    {
        ImageViewer.SetZoom((float) Math.Pow(10, sender.DoubleValue));
    }
}