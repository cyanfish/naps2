using Eto.Forms;
using Eto.Mac;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Mac;

namespace NAPS2.EtoForms.Ui;

public class MacPreviewForm : PreviewForm
{
    private readonly Dictionary<Type, NSButton> _editToolButtons = new();

    private readonly NSSlider _zoomSlider;

    public MacPreviewForm(Naps2Config config, DesktopCommands desktopCommands, UiImageList imageList,
        IIconProvider iconProvider, ColorScheme colorScheme, EditWithController editWithController) : base(config,
        desktopCommands, imageList, iconProvider, colorScheme, editWithController)
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

    protected override void UpdateActiveEditTool(Type? editorType)
    {
        foreach (var (type, button) in _editToolButtons)
        {
            button.State = type == editorType ? NSCellStateValue.On : NSCellStateValue.Off;
        }
    }

    private NSToolbarItem CreateEditToolItem<T>(string id, ActionCommand command) where T : ImageEditor
    {
        var button = new NSButton
        {
            Image = command.GetIconImage(IconScenario.Toolbar, 1)?.ToNS(),
            BezelStyle = NSBezelStyle.TexturedRounded,
            ToolTip = command.ToolBarText ?? "",
            Enabled = command.Enabled
        };
        button.SetButtonType(NSButtonType.PushOnPushOff);
        button.Activated += (_, _) => command.Execute();
        command.EnabledChanged += (_, _) => button.Enabled = command.Enabled;
        _editToolButtons[typeof(T)] = button;
        return new NSToolbarItem(id) { View = button, Label = command.ToolBarText ?? "" };
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
            CreateEditToolItem<CropEditor>("crop", Commands.Crop),
            CreateEditToolItem<BrightContEditor>("brightcont", Commands.BrightCont),
            CreateEditToolItem<HueSatEditor>("huesat", Commands.HueSat),
            CreateEditToolItem<BlackWhiteEditor>("blackwhite", Commands.BlackWhite),
            CreateEditToolItem<SharpenEditor>("sharpen", Commands.Sharpen),
            MacToolbarItems.Create("documentcorrection", Commands.DocumentCorrection),
            CreateEditToolItem<CompressEditor>("compress", Commands.Compress),
            CreateEditToolItem<SplitEditor>("split", Commands.Split),
            CreateEditToolItem<CombineEditor>("combine", Commands.Combine),
            MacToolbarItems.Create("editwith", Commands.CreateEditWithApp()),
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