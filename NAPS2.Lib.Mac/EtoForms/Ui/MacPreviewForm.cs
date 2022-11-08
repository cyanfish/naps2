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
        toolbar.Delegate = new MacToolbarDelegate(CreateMacToolbarEntries());
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

    private List<MacToolbarEntry> CreateMacToolbarEntries()
    {
        return new List<MacToolbarEntry>
        {
            new("prev", MacToolbarEntry.CreateItem(GoToPrevCommand, nav: true)),
            new("next", MacToolbarEntry.CreateItem(GoToNextCommand, nav: true)),
            new("rotate", MacToolbarEntry.CreateMenuItem(Commands.RotateMenu, new MenuProvider()
                .Append(Commands.RotateLeft)
                .Append(Commands.RotateRight)
                .Append(Commands.Flip)
                .Append(Commands.Deskew)
                .Append(Commands.CustomRotate))),
            new("crop", MacToolbarEntry.CreateItem(Commands.Crop)),
            new("brightcont", MacToolbarEntry.CreateItem(Commands.BrightCont)),
            new("huesat", MacToolbarEntry.CreateItem(Commands.HueSat)),
            new("blackwhite", MacToolbarEntry.CreateItem(Commands.BlackWhite)),
            new("sharpen", MacToolbarEntry.CreateItem(Commands.Sharpen)),
            new("sep0", MacToolbarEntry.CreateSeparator()),
            new("save", MacToolbarEntry.CreateItem(Commands.SaveSelected)),
            new("sep1", MacToolbarEntry.CreateSeparator()),
            new("delete", MacToolbarEntry.CreateItem(Commands.Delete)),
        };
    }
}