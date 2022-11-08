using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Ui;

public class PreviewForm : EtoDialogBase
{
    private readonly DesktopCommands _desktopCommands;

    private readonly ImageView _imageView = new();
    private UiImage? _currentImage;

    public PreviewForm(Naps2Config config, DesktopCommands desktopCommands, UiImageList imageList,
        IIconProvider iconProvider) : base(config)
    {
        _desktopCommands = desktopCommands;
        ImageList = imageList;

        Title = UiStrings.PreviewFormTitle;
        Icon = Icons.picture.ToEtoIcon();

        FormStateController.AutoLayoutSize = false;
        FormStateController.DefaultClientSize = new Size(800, 600);
        LayoutController.RootPadding = 0;
        LayoutController.Content = _imageView;

        GoToPrevCommand = new ActionCommand(() => GoTo(ImageIndex - 1))
        {
            Text = UiStrings.Previous,
            Image = iconProvider.GetIcon("arrow_left")
        };
        GoToNextCommand = new ActionCommand(() => GoTo(ImageIndex + 1))
        {
            Text = UiStrings.Next,
            Image = iconProvider.GetIcon("arrow_right")
        };
    }

    protected DesktopCommands Commands { get; set; } = null!;
    protected ActionCommand GoToPrevCommand { get; }
    protected ActionCommand GoToNextCommand { get; }

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
            Commands = _desktopCommands.WithSelection(ListSelection.Of(_currentImage));
            _currentImage.ThumbnailInvalidated += ImageThumbnailInvalidated;
        }
    }

    private void ImageThumbnailInvalidated(object? sender, EventArgs e)
    {
        Invoker.Current.SafeInvoke(() => UpdateImage().AssertNoAwait());
    }

    protected int ImageIndex
    {
        get
        {
            var index = ImageList!.Images.IndexOf(CurrentImage);
            if (index == -1)
            {
                index = 0;
            }
            return index;
        }
    }

    protected override void OnLoad(EventArgs eventArgs)
    {
        base.OnLoad(eventArgs);
        // TODO: Implement
        // _tbPageCurrent.Visible = PlatformCompat.Runtime.IsToolbarTextboxSupported;
        // if (Config.Get(c => c.HiddenButtons).HasFlag(ToolbarButtons.SavePdf))
        // {
        //     _toolStrip1.Items.Remove(_tsSavePdf);
        // }
        // if (Config.Get(c => c.HiddenButtons).HasFlag(ToolbarButtons.SaveImages))
        // {
        //     _toolStrip1.Items.Remove(_tsSaveImage);
        // }

        // TODO: Implement mouse and keyboard controls
        // AssignKeyboardShortcuts();

        // TODO: We should definitely start with separate image forms, but it might be fairly trivial to, when opened
        // from the preview form, have the temporary rendering be propagated back to the viewer form and have the
        // dialog only show the editing controls. So it feels more like an image editor. Clicking e.g. "Crop" in the
        // desktop form should still open the full image editing form (for now at least).
        CreateToolbar();

        UpdatePage();
        UpdateImage().AssertNoAwait();
    }

    protected virtual void CreateToolbar()
    {
        ToolBar = new ToolBar
        {
            Items =
            {
                new DropDownToolItem
                {
                    Image = Commands.RotateMenu.Image,
                    Items =
                    {
                        Commands.RotateLeft,
                        Commands.RotateRight,
                        Commands.Flip,
                        Commands.Deskew,
                        Commands.CustomRotate
                    }
                },
                Commands.Crop,
                Commands.BrightCont,
                Commands.HueSat,
                Commands.BlackWhite,
                Commands.Sharpen,
                new SeparatorToolItem(),
                Commands.SaveSelectedPdf,
                Commands.SaveSelectedImages,
                new SeparatorToolItem(),
                Commands.Delete
            }
        };
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
        UpdatePage();
        await UpdateImage();
    }

    protected virtual void UpdatePage()
    {
        // TODO: Implement
        // _tbPageCurrent.Text = (ImageIndex + 1).ToString(CultureInfo.CurrentCulture);
        // _lblPageTotal.Text = string.Format(MiscResources.OfN, _imageList.Images.Count);
        // if (!PlatformCompat.Runtime.IsToolbarTextboxSupported)
        // {
        //     _lblPageTotal.Text = _tbPageCurrent.Text + ' ' + _lblPageTotal.Text;
        // }
    }

    private async Task UpdateImage()
    {
        // TODO: Implement
        // _tiffViewer1.Image?.Dispose();
        // _tiffViewer1.Image = null;
        using var imageToRender = CurrentImage.GetClonedImage();
        _imageView.Image = imageToRender.Render().ToEtoImage();
        // _tiffViewer1.Image = imageToRender.RenderToBitmap();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // TODO: Implement
            // _components?.Dispose();
            // _tiffViewer1?.Image?.Dispose();
            // _tiffViewer1?.Dispose();
        }
        base.Dispose(disposing);
    }
}