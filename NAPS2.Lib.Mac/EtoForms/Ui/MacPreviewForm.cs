using Eto.Forms;
using Eto.Mac;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Mac;

namespace NAPS2.EtoForms.Ui;

public class MacPreviewForm : PreviewForm
{
    public MacPreviewForm(Naps2Config config, PreviewPane pane) : base(config, pane)
    {
        Pane.PageChanged += (_, _) => UpdatePage();
    }

    protected override void CreateToolbar()
    {
        var toolbar = new NSToolbar("naps2.preview.toolbar");
        toolbar.Delegate = new MacToolbarDelegate(
        [
            MacToolbarItems.Create("prev", Pane.GoToPrevCommand, nav: true),
            MacToolbarItems.Create("next", Pane.GoToNextCommand, nav: true),
            MacToolbarItems.Create("zoomfit", Pane.ZoomWindowCommand),
            MacToolbarItems.Create("zoomout", Pane.ZoomOutCommand),
            MacToolbarItems.Create("zoomin", Pane.ZoomInCommand)
        ]);
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

    private void UpdatePage()
    {
        var window = this.ToNative();
        if (OperatingSystem.IsMacOSVersionAtLeast(11))
        {
            window.Subtitle = Pane.PageText;
        }
    }
}
