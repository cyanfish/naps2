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
        Icon = new Icon(1f, Icons.picture.ToEtoImage());

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

    // private async void tbPageCurrent_TextChanged(object sender, EventArgs e)
    // {
    //     if (int.TryParse(_tbPageCurrent.Text, out int indexOffBy1))
    //     {
    //         await GoTo(indexOffBy1 - 1);
    //     }
    // }

    // private async Task DeleteCurrentImage()
    // {
    //     // TODO: Are the file access issues still a thing?
    //     // Need to dispose the bitmap first to avoid file access issues
    //     _tiffViewer1.Image?.Dispose();
    //
    //     var lastIndex = ImageIndex;
    //     await _imageList.MutateAsync(new ImageListMutation.DeleteSelected(),
    //         ListSelection.Of(CurrentImage));
    //
    //     bool shouldClose = false;
    //     lock (_imageList)
    //     {
    //         if (_imageList.Images.Any())
    //         {
    //             // Update the GUI for the newly displayed image
    //             var nextIndex = lastIndex >= _imageList.Images.Count ? _imageList.Images.Count - 1 : lastIndex;
    //             CurrentImage = _imageList.Images[nextIndex];
    //         }
    //         else
    //         {
    //             shouldClose = true;
    //         }
    //     }
    //     if (shouldClose)
    //     {
    //         // No images left to display, so no point keeping the form open
    //         Close();
    //     }
    //     else
    //     {
    //         UpdatePage();
    //         await UpdateImage();
    //     }
    // }

    // private async void tiffViewer1_KeyDown(object sender, KeyEventArgs e)
    // {
    //     if (!(e.Control || e.Shift || e.Alt))
    //     {
    //         switch (e.KeyCode)
    //         {
    //             case Keys.Escape:
    //                 Close();
    //                 return;
    //             case Keys.PageDown:
    //             case Keys.Right:
    //             case Keys.Down:
    //                 await GoTo(ImageIndex + 1);
    //                 return;
    //             case Keys.PageUp:
    //             case Keys.Left:
    //             case Keys.Up:
    //                 await GoTo(ImageIndex - 1);
    //                 return;
    //         }
    //     }
    //
    //     e.Handled = _ksm.Perform(e.KeyData);
    // }
    //
    // private async void tbPageCurrent_KeyDown(object sender, KeyEventArgs e)
    // {
    //     if (!(e.Control || e.Shift || e.Alt))
    //     {
    //         switch (e.KeyCode)
    //         {
    //             case Keys.PageDown:
    //             case Keys.Right:
    //             case Keys.Down:
    //                 await GoTo(ImageIndex + 1);
    //                 return;
    //             case Keys.PageUp:
    //             case Keys.Left:
    //             case Keys.Up:
    //                 await GoTo(ImageIndex - 1);
    //                 return;
    //         }
    //     }
    //
    //     e.Handled = _ksm.Perform(e.KeyData);
    // }
    //
    // private void AssignKeyboardShortcuts()
    // {
    //     // Defaults
    //
    //     _ksm.Assign("Del", _tsDelete);
    //
    //     // Configured
    //
    //     // TODO: Granular
    //     var ks = Config.Get(c => c.KeyboardShortcuts);
    //
    //     _ksm.Assign(ks.Delete, _tsDelete);
    //     _ksm.Assign(ks.ImageBlackWhite, _tsBlackWhite);
    //     _ksm.Assign(ks.ImageBrightness, _tsBrightnessContrast);
    //     _ksm.Assign(ks.ImageContrast, _tsBrightnessContrast);
    //     _ksm.Assign(ks.ImageCrop, _tsCrop);
    //     _ksm.Assign(ks.ImageHue, _tsHueSaturation);
    //     _ksm.Assign(ks.ImageSaturation, _tsHueSaturation);
    //     _ksm.Assign(ks.ImageSharpen, _tsSharpen);
    //
    //     _ksm.Assign(ks.RotateCustom, _tsCustomRotation);
    //     _ksm.Assign(ks.RotateFlip, _tsFlip);
    //     _ksm.Assign(ks.RotateLeft, _tsRotateLeft);
    //     _ksm.Assign(ks.RotateRight, _tsRotateRight);
    //     _ksm.Assign(ks.SaveImages, _tsSaveImage);
    //     _ksm.Assign(ks.SavePDF, _tsSavePdf);
    // }
}