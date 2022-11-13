using Eto.Forms;
using NAPS2.EtoForms.Mac;

namespace NAPS2.EtoForms.Ui;

public class MacPreviewForm : PreviewForm
{
    public MacPreviewForm(Naps2Config config, DesktopCommands desktopCommands, UiImageList imageList,
        IIconProvider iconProvider) : base(config, desktopCommands, imageList, iconProvider)
    {
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

    private List<NSToolbarItem> CreateMacToolbarItems()
    {
        return new List<NSToolbarItem>
        {
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
            MacToolbarItems.CreateSeparator("sep0"),
            MacToolbarItems.Create("save", Commands.SaveSelected),
            // TODO: Fix this
            // MacToolbarItems.CreateSeparator("sep1"),
            MacToolbarItems.Create("delete", Commands.Delete)
        };
    }
}