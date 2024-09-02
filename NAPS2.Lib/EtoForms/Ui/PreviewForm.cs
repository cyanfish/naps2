using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

public class PreviewForm : EtoDialogBase
{
    private readonly DesktopCommands _desktopCommands;
    private readonly IIconProvider _iconProvider;
    private readonly KeyboardShortcutManager _previewKsm;

    private readonly ButtonToolItem _pageNumberButton = new();
    private readonly ButtonToolItem _zoomPercentButton = new();
    private UiImage? _currentImage;

    public PreviewForm(Naps2Config config, DesktopCommands desktopCommands, UiImageList imageList,
        IIconProvider iconProvider, ColorScheme colorScheme) : base(config)
    {
        Title = UiStrings.PreviewFormTitle;
        Icon = Icons.favicon.ToEtoIcon();

        _desktopCommands = desktopCommands;
        ImageList = imageList;
        _iconProvider = iconProvider;

        ImageViewer.ColorScheme = colorScheme;
        ImageViewer.ZoomChanged += ImageViewerZoomChanged;
        ImageList.ImagesUpdated += ImageList_ImagesUpdated;

        GoToPrevCommand = new ActionCommand(() => GoTo(ImageIndex - 1))
        {
            Text = UiStrings.Previous,
            IconName = "arrow_left_small"
        };
        GoToNextCommand = new ActionCommand(() => GoTo(ImageIndex + 1))
        {
            Text = UiStrings.Next,
            IconName = "arrow_right_small"
        };
        ZoomInCommand = new ActionCommand(() => ImageViewer.ChangeZoom(1))
        {
            Text = UiStrings.ZoomIn,
            IconName = "zoom_in_small"
        };
        ZoomOutCommand = new ActionCommand(() => ImageViewer.ChangeZoom(-1))
        {
            Text = UiStrings.ZoomOut,
            IconName = "zoom_out_small"
        };
        ZoomWindowCommand = new ActionCommand(ImageViewer.ZoomToContainer)
        {
            // TODO: Update this string as it's now a button and not a toggle
            Text = UiStrings.ScaleWithWindow,
            IconName = "arrow_out_small"
        };
        ZoomActualCommand = new ActionCommand(ImageViewer.ZoomToActual)
        {
            Text = UiStrings.ZoomActual,
            IconName = "zoom_actual_small"
        };
        DeleteCurrentImageCommand = new ActionCommand(DeleteCurrentImage)
        {
            Text = UiStrings.Delete,
            IconName = "cross_small"
        };

        _previewKsm = new KeyboardShortcutManager();
        EtoPlatform.Current.HandleKeyDown(this, _previewKsm.Perform);
    }

    private void ImageList_ImagesUpdated(object? sender, ImageListEventArgs e)
    {
        Invoker.Current.InvokeDispatch(async () =>
        {
            bool shouldClose = false;
            lock (ImageList)
            {
                if (ImageList.Images.Contains(CurrentImage))
                {
                    UpdateImageIndex();
                    UpdatePage();
                    return;
                }
                if (ImageList.Images.Any())
                {
                    // Update the GUI for the newly displayed image
                    var nextIndex = ImageIndex >= ImageList.Images.Count ? ImageList.Images.Count - 1 : ImageIndex;
                    CurrentImage = ImageList.Images[nextIndex];
                    ImageList.UpdateSelection(ListSelection.Of(CurrentImage));
                }
                else
                {
                    shouldClose = true;
                    ImageList.UpdateSelection(ListSelection.Empty<UiImage>());
                }
            }
            if (shouldClose)
            {
                // No images left to display, so no point keeping the form open
                Close();
            }
            else
            {
                UpdatePage();
                await UpdateImage();
            }
        });
    }

    private void UpdateImageIndex()
    {
        var index = ImageList.Images.IndexOf(CurrentImage);
        if (index == -1)
        {
            index = 0;
        }
        ImageIndex = index;
    }

    protected ScrollZoomImageViewer ImageViewer { get; } = new();

    private void ImageViewerZoomChanged(object? sender, ZoomChangedEventArgs e)
    {
        _zoomPercentButton.Text = e.Zoom.ToString("P0");
    }

    protected override void BuildLayout()
    {
        FormStateController.AutoLayoutSize = false;
        FormStateController.DefaultClientSize = new Size(800, 600);

        LayoutController.RootPadding = 0;
        LayoutController.Content = ImageViewer;
    }

    protected DesktopCommands Commands { get; set; } = null!;
    protected ActionCommand DeleteCurrentImageCommand { get; }
    protected ActionCommand GoToPrevCommand { get; }
    protected ActionCommand GoToNextCommand { get; }
    protected ActionCommand ZoomInCommand { get; }
    protected ActionCommand ZoomOutCommand { get; }
    protected ActionCommand ZoomWindowCommand { get; }
    protected ActionCommand ZoomActualCommand { get; }

    protected UiImageList ImageList { get; }

    public UiImage CurrentImage
    {
        get => _currentImage ?? throw new InvalidOperationException();
        set
        {
            if (_currentImage != null)
            {
                _currentImage.ThumbnailInvalidated -= ImageThumbnailInvalidated;
            }
            _currentImage = value;
            UpdateImageIndex();
            Commands = _desktopCommands.WithSelection(() => ListSelection.Of(_currentImage));
            _currentImage.ThumbnailInvalidated += ImageThumbnailInvalidated;
        }
    }

    private void ImageThumbnailInvalidated(object? sender, EventArgs e)
    {
        Invoker.Current.InvokeDispatch(() => UpdateImage().AssertNoAwait());
    }

    protected int ImageIndex { get; private set; }

    protected override async void OnLoad(EventArgs eventArgs)
    {
        base.OnLoad(eventArgs);

        AssignKeyboardShortcuts();

        // TODO: We should definitely start with separate image forms, but it might be fairly trivial to, when opened
        // from the preview form, have the temporary rendering be propagated back to the viewer form and have the
        // dialog only show the editing controls. So it feels more like an image editor. Clicking e.g. "Crop" in the
        // desktop form should still open the full image editing form (for now at least).
        CreateToolbar();

        await UpdateImage();
        UpdatePage();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        ImageViewer.ZoomToContainer();
    }

    protected virtual void CreateToolbar()
    {
        ToolBar = new ToolBar
        {
            Items =
            {
                // TODO: Either embed an editable textbox, or make this button pop up a window or widget to enter a page number
                _pageNumberButton,
                MakeToolButton(GoToPrevCommand),
                MakeToolButton(GoToNextCommand),
                new SeparatorToolItem(),
                MakeToolButton(ZoomWindowCommand),
                MakeToolButton(ZoomActualCommand),
                MakeToolButton(ZoomInCommand),
                MakeToolButton(ZoomOutCommand),
                _zoomPercentButton,
                new SeparatorToolItem(),
                WithToolItemIcon(new DropDownToolItem
                {
                    ToolTip = UiStrings.Rotate,
                    Items =
                    {
                        Commands.RotateLeft,
                        Commands.RotateRight,
                        Commands.Flip,
                        Commands.Deskew,
                        Commands.CustomRotate
                    }
                }, "arrow_rotate_anticlockwise_small"),
                MakeToolButton(Commands.Crop),
                MakeToolButton(Commands.BrightCont),
                MakeToolButton(Commands.HueSat),
                MakeToolButton(Commands.BlackWhite),
                MakeToolButton(Commands.Sharpen),
                MakeToolButton(Commands.DocumentCorrection),
                new SeparatorToolItem(),
                MakeToolButton(Commands.Split),
                MakeToolButton(Commands.Combine),
                new SeparatorToolItem(),
                MakeToolButton(Commands.SaveSelectedPdf, "file_extension_pdf"),
                MakeToolButton(Commands.SaveSelectedImages, "picture_small"),
                new SeparatorToolItem(),
                MakeToolButton(DeleteCurrentImageCommand),
            }
        };
        if (Config.Get(c => c.HiddenButtons).HasFlag(ToolbarButtons.SavePdf))
        {
            ToolBar.Items.Remove(ToolBar.Items.Single(x => x.Command == Commands.SaveSelectedPdf));
        }
        if (Config.Get(c => c.HiddenButtons).HasFlag(ToolbarButtons.SaveImages))
        {
            ToolBar.Items.Remove(ToolBar.Items.Single(x => x.Command == Commands.SaveSelectedImages));
        }
    }

    private ToolItem MakeToolButton(ActionCommand command, string? iconName = null) =>
        WithToolItemIcon(new ButtonToolItem
        {
            Command = command,
            Text = null,
            ToolTip = command.Text
        }, iconName ?? command.IconName!);

    private ToolItem WithToolItemIcon(ToolItem toolItem, string iconName)
    {
        EtoPlatform.Current.AttachDpiDependency(this,
            scale =>
            {
                EtoPlatform.Current.SetImageSize(toolItem, (int) (16 * scale));
                toolItem.Image = EtoPlatform.Current.IconProvider.GetIcon(iconName, scale);
            });
        return toolItem;
    }

    private async Task GoTo(int index)
    {
        lock (ImageList)
        {
            if (index == ImageIndex || index < 0 || index >= ImageList.Images.Count)
            {
                return;
            }
            CurrentImage = ImageList.Images[index];
            ImageList.UpdateSelection(ListSelection.Of(CurrentImage));
        }
        await UpdateImage();
        UpdatePage();
    }

    protected virtual void UpdatePage()
    {
        _pageNumberButton.Text = string.Format(UiStrings.XOfY, ImageIndex + 1, ImageList.Images.Count);
    }

    private async Task UpdateImage()
    {
        using var imageToRender = CurrentImage.GetClonedImage();
        using var rendered = await Task.Run(() => imageToRender.Render());
        ImageViewer.Image?.Dispose();
        ImageViewer.Image = rendered.ToEtoImage();
        ImageViewer.ZoomToContainer();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (EtoPlatform.Current.IsGtk)
        {
            // Gtk delays adjusting the imageview size after the container size changes, which messes this up unless
            // we queue it after the current UI thread options.
            // We don't want to do this on Windows/Mac as it results in the render size lagging the window size.
            Invoker.Current.InvokeDispatch(ImageViewer.ZoomToContainer);
        }
        else
        {
            ImageViewer.ZoomToContainer();
        }
    }

    private void DeleteCurrentImage()
    {
        if (MessageBox.Show(this,
                string.Format(MiscResources.ConfirmDeleteItems, 1),
                MiscResources.Delete, MessageBoxButtons.OKCancel,
                MessageBoxType.Question, MessageBoxDefaultButton.OK) == DialogResult.Ok)
        {
            // We don't want to run Commands.Delete as that runs on DesktopController and uses that selection.
            Commands.ImageListActions.DeleteSelected();
        }
    }

    private void AssignKeyboardShortcuts()
    {
        // Unconfigurable defaults

        _previewKsm.Assign("Esc", Close);
        _previewKsm.Assign("Del", DeleteCurrentImageCommand);
        _previewKsm.Assign("Mod+0", ZoomActualCommand);
        _previewKsm.Assign("Mod+Z", Commands.Undo);
        _previewKsm.Assign(EtoPlatform.Current.IsWinForms ? "Mod+Y" : "Mod+Shift+Z", Commands.Redo);

        _previewKsm.Assign("PageDown", () => _ = GoTo(ImageIndex + 1));
        _previewKsm.Assign("Right", () => _ = GoTo(ImageIndex + 1));
        _previewKsm.Assign("Down", () => _ = GoTo(ImageIndex + 1));

        _previewKsm.Assign("PageUp", () => _ = GoTo(ImageIndex - 1));
        _previewKsm.Assign("Left", () => _ = GoTo(ImageIndex - 1));
        _previewKsm.Assign("Up", () => _ = GoTo(ImageIndex - 1));

        // Configured defaults

        var ks = Config.Get(c => c.KeyboardShortcuts);

        _previewKsm.Assign(ks.Delete, DeleteCurrentImageCommand);
        _previewKsm.Assign(ks.ImageBlackWhite, Commands.BlackWhite);
        _previewKsm.Assign(ks.ImageBrightness, Commands.BrightCont);
        _previewKsm.Assign(ks.ImageContrast, Commands.BrightCont);
        _previewKsm.Assign(ks.ImageCrop, Commands.Crop);
        _previewKsm.Assign(ks.ImageHue, Commands.HueSat);
        _previewKsm.Assign(ks.ImageSaturation, Commands.HueSat);
        _previewKsm.Assign(ks.ImageSharpen, Commands.Sharpen);
        _previewKsm.Assign(ks.ImageDocumentCorrection, Commands.DocumentCorrection);
        _previewKsm.Assign(ks.ImageSplit, Commands.Split);
        _previewKsm.Assign(ks.ImageCombine, Commands.Combine);

        _previewKsm.Assign(ks.RotateCustom, Commands.CustomRotate);
        _previewKsm.Assign(ks.RotateFlip, Commands.Flip);
        _previewKsm.Assign(ks.RotateLeft, Commands.RotateLeft);
        _previewKsm.Assign(ks.RotateRight, Commands.RotateRight);

        if (PlatformCompat.System.CombinedPdfAndImageSaving)
        {
            _previewKsm.Assign(ks.SavePDFAll, Commands.SaveSelected);
        }
        else
        {
            _previewKsm.Assign(ks.SavePDF, Commands.SaveSelectedPdf);
            _previewKsm.Assign(ks.SaveImages, Commands.SaveSelectedImages);
            _previewKsm.Assign(ks.SavePDFAll, Commands.SaveSelectedPdf);
            _previewKsm.Assign(ks.SaveImagesAll, Commands.SaveSelectedImages);
        }

        _previewKsm.Assign(ks.ZoomIn, ZoomInCommand);
        _previewKsm.Assign(ks.ZoomOut, ZoomOutCommand);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ImageViewer.Image?.Dispose();
            ImageViewer.Image = null;
            ImageList.ImagesUpdated -= ImageList_ImagesUpdated;
        }
        base.Dispose(disposing);
    }
}