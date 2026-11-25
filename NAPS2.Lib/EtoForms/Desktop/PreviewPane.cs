using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Ui;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Desktop;

/// <summary>
/// Shows an image at full size with navigation, zoom and inline editing. This is hosted either docked in the desktop
/// form or in the preview window.
/// </summary>
public class PreviewPane : IDisposable
{
    public const int MinWidth = 420;

    private readonly Naps2Config _config;
    private readonly UiImageList _imageList;
    private readonly ColorScheme _colorScheme;
    private readonly EditWithController _editWithController;
    private readonly KeyboardShortcutManager _previewKsm = new();
    private readonly KeyboardShortcutManager _editKsm = new();
    private readonly Dictionary<Type, ActionCommand> _editCommands;

    private readonly LayoutVisibility _paneVis = new(false);
    private readonly LayoutVisibility _editVis = new(false);
    private readonly LayoutPlaceholder _headerPlaceholder = new();
    private readonly LayoutPlaceholder _editorPlaceholder = new();
    private readonly Label _pageLabel = new();
    private readonly LayoutControl _pageLabelControl;
    private readonly Panel _separator = new();
    private readonly LayoutControl _topInset = C.Spacer();
    private readonly Dictionary<Type, CheckToolItem> _editToolItems = new();
    private readonly Dictionary<Type, ToggleButton> _editToggles = new();
    private Window? _window;
    private bool _windowed;
    private UiImage? _currentImage;
    private Bitmap? _viewBitmap;
    private int _renderVersion;
    private ImageEditor? _editor;
    private bool _updatingEditTools;

    public PreviewPane(Naps2Config config, DesktopCommands desktopCommands, UiImageList imageList,
        ColorScheme colorScheme, EditWithController editWithController)
    {
        _config = config;
        _imageList = imageList;
        _colorScheme = colorScheme;
        _editWithController = editWithController;

        Commands = desktopCommands.WithSelection(
            () => _currentImage == null ? ListSelection.Empty<UiImage>() : ListSelection.Of(_currentImage), Edit);
        _editCommands = new Dictionary<Type, ActionCommand>
        {
            { typeof(RotateEditor), Commands.CustomRotate },
            { typeof(CropEditor), Commands.Crop },
            { typeof(BrightContEditor), Commands.BrightCont },
            { typeof(HueSatEditor), Commands.HueSat },
            { typeof(BlackWhiteEditor), Commands.BlackWhite },
            { typeof(SharpenEditor), Commands.Sharpen },
            { typeof(CompressEditor), Commands.Compress },
            { typeof(SplitEditor), Commands.Split },
            { typeof(CombineEditor), Commands.Combine }
        };

        _pageLabelControl = _pageLabel.AlignCenter().Padding(left: 4);
        ImageViewer.ColorScheme = colorScheme;
        _imageList.ImagesUpdated += ImageList_ImagesUpdated;
        _imageList.SelectionChanged += ImageList_SelectionChanged;
        _editWithController.AppChanged += EditWithController_AppChanged;
        _colorScheme.ColorSchemeChanged += ColorSchemeChanged;
        UpdateColors();

        GoToPrevCommand = new ActionCommand(() => GoTo(ImageIndex - 1))
        {
            Text = UiStrings.Previous,
            IconName = "arrow_left"
        };
        GoToNextCommand = new ActionCommand(() => GoTo(ImageIndex + 1))
        {
            Text = UiStrings.Next,
            IconName = "arrow_right"
        };
        ZoomInCommand = new ActionCommand(() => ImageViewer.ChangeZoom(1))
        {
            Text = UiStrings.ZoomIn,
            IconName = "zoom_in"
        };
        ZoomOutCommand = new ActionCommand(() => ImageViewer.ChangeZoom(-1))
        {
            Text = UiStrings.ZoomOut,
            IconName = "zoom_out"
        };
        ZoomWindowCommand = new ActionCommand(ImageViewer.ZoomToContainer)
        {
            // TODO: Update this string as it's now a button and not a toggle
            Text = UiStrings.ScaleWithWindow,
            IconName = "arrow_out"
        };
        ZoomActualCommand = new ActionCommand(ImageViewer.ZoomToActual)
        {
            Text = UiStrings.ZoomActual,
            IconName = "zoom_actual"
        };
        DeleteCurrentImageCommand = new ActionCommand(DeleteCurrentImage)
        {
            Text = UiStrings.Delete,
            IconName = "cross"
        };
        ViewInNewWindowCommand = new ActionCommand(SwitchView)
        {
            Text = UiStrings.ViewInNewWindow,
            IconName = "new_window"
        };
        ViewInPreviewPaneCommand = new ActionCommand(SwitchView)
        {
            Text = UiStrings.ViewInPreviewPane,
            IconName = "application_side_list_right"
        };
        CloseCommand = new ActionCommand(Hide)
        {
            Text = UiStrings.ClosePreviewPane,
            IconName = "cancel"
        };
    }

    public ScrollZoomImageViewer ImageViewer { get; } = new();

    public DesktopCommands Commands { get; }
    public ActionCommand DeleteCurrentImageCommand { get; }
    public ActionCommand GoToPrevCommand { get; }
    public ActionCommand GoToNextCommand { get; }
    public ActionCommand ZoomInCommand { get; }
    public ActionCommand ZoomOutCommand { get; }
    public ActionCommand ZoomWindowCommand { get; }
    public ActionCommand ZoomActualCommand { get; }
    public ActionCommand ViewInNewWindowCommand { get; }
    public ActionCommand ViewInPreviewPaneCommand { get; }
    public ActionCommand CloseCommand { get; }

    public bool IsVisible => _paneVis.IsVisible;

    public UiImage? CurrentImage => _currentImage;

    public int ImageIndex { get; private set; }

    public string PageText => _pageLabel.Text;

    public Type? ActiveEditorType => _editor?.GetType();

    public event EventHandler? Closed;
    public event EventHandler? ViewInPreviewPaneRequested;
    public event EventHandler? PageChanged;

    public LayoutElement CreateView(Window window, bool windowed)
    {
        _window = window;
        _windowed = windowed;
        AssignKeyboardShortcuts();
        // Gtk toolbar items need their icons before they're added to the toolbar, and those need the window scale
        window.LoadComplete += (_, _) => UpdateHeader();
        return L.Row(
            windowed ? C.None() : _separator.Width(1),
            L.Column(
                windowed ? C.None() : _topInset,
                _headerPlaceholder,
                ImageViewer.Scale(),
                L.Column(_editorPlaceholder).Padding(6).Visible(_editVis)
            ).Spacing(0).Scale()
        ).Spacing(0).Visible(_paneVis);
    }

    private void UpdateHeader()
    {
        _editToolItems.Clear();
        _editToggles.Clear();
        _headerPlaceholder.Content = CreateHeader();
        UpdateActiveEditTool();
    }

    private LayoutElement CreateHeader()
    {
        var edit = EtoPlatform.Current.SupportsInlinePreviewToolBar ? CreateEditToolBar() : CreateEditButtons();
        var centeredEdit = L.Row(C.Filler(), edit, C.Filler());
        if (_windowed && EtoPlatform.Current.IsMac)
        {
            // The rest of the header is in the window toolbar
            return centeredEdit.Padding(6);
        }
        var zoomOut = CreateHeaderButton(ZoomOutCommand);
        var zoomIn = CreateHeaderButton(ZoomInCommand);
        EtoPlatform.Current.ConfigureJoinedButtons(zoomOut, zoomIn);
        var left = L.Row(
            HeaderButton(CreateHeaderButton(GoToPrevCommand)),
            HeaderButton(CreateHeaderButton(GoToNextCommand)),
            _pageLabelControl
        ).Spacing(2);
        var right = L.Row(
            HeaderButton(CreateHeaderButton(ZoomWindowCommand)),
            HeaderButton(zoomOut).SpacingAfter(0),
            HeaderButton(zoomIn).SpacingAfter(12),
            _windowed && !_config.Get(c => c.EnablePreviewPane)
                ? C.None()
                : HeaderButton(CreateHeaderButton(_windowed ? ViewInPreviewPaneCommand : ViewInNewWindowCommand)),
            _windowed ? C.None() : HeaderButton(CreateHeaderButton(CloseCommand))
        ).Spacing(2);
        return L.Column(L.Adaptive(
            L.Row(left, centeredEdit.Scale(), right),
            L.Column(L.Row(left, C.Filler(), right), L.Adaptive(centeredEdit, edit))
        )).Padding(6);
    }

    private static Button CreateHeaderButton(ActionCommand command, string? iconName = null)
    {
        var button = C.Button(command, iconName ?? command.IconName, ButtonImagePosition.Overlay);
        button.ToolTip = command.Text;
        return button;
    }

    private static LayoutControl HeaderButton(Button button) => button.Width(EtoPlatform.Current.IconButtonSize);

    private IEnumerable<(ActionCommand Command, string? IconName)> GetSaveCommands()
    {
        if (PlatformCompat.System.CombinedPdfAndImageSaving)
        {
            yield return (Commands.SaveSelected, null);
            yield break;
        }
        var hiddenButtons = _config.Get(c => c.HiddenButtons);
        if (!hiddenButtons.HasFlag(ToolbarButtons.SavePdf))
        {
            yield return (Commands.SaveSelectedPdf, "file_extension_pdf");
        }
        if (!hiddenButtons.HasFlag(ToolbarButtons.SaveImages))
        {
            yield return (Commands.SaveSelectedImages, "picture");
        }
    }

    private LayoutElement CreateEditToolBar()
    {
        var toolBar = new ToolBar();
        toolBar.Items.AddRange(
        [
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
            }, Commands.RotateMenu),
            MakeEditToolButton<CropEditor>(),
            MakeEditToolButton<BrightContEditor>(),
            MakeEditToolButton<HueSatEditor>(),
            MakeEditToolButton<BlackWhiteEditor>(),
            MakeEditToolButton<SharpenEditor>(),
            MakeToolButton(Commands.DocumentCorrection),
            MakeEditToolButton<CompressEditor>(),
            MakeEditToolButton<SplitEditor>(),
            MakeEditToolButton<CombineEditor>()
        ]);
        if (_editWithController.HasApp)
        {
            toolBar.Items.Add(MakeToolButton(Commands.CreateEditWithApp()));
        }
        toolBar.Items.AddRange(
        [
            MakeToolButton(Commands.EditWithPick),
            new SeparatorToolItem(),
            MakeToolButton(Commands.Undo),
            MakeToolButton(Commands.Redo)
        ]);
        if (_windowed)
        {
            var saveCommands = GetSaveCommands().ToList();
            toolBar.Items.Add(new SeparatorToolItem());
            toolBar.Items.AddRange(saveCommands.Select(x => MakeToolButton(x.Command, x.IconName)));
            if (saveCommands.Count > 0)
            {
                toolBar.Items.Add(new SeparatorToolItem());
            }
            toolBar.Items.Add(MakeToolButton(DeleteCurrentImageCommand));
        }
        return EtoPlatform.Current.CreateInlinePreviewToolBar(toolBar);
    }

    private ToolItem MakeEditToolButton<T>() where T : ImageEditor
    {
        var command = _editCommands[typeof(T)];
        var item = new CheckToolItem
        {
            Command = command,
            Text = null,
            ToolTip = command.Text
        };
        _editToolItems[typeof(T)] = item;
        return WithToolItemIcon(item, command);
    }

    private ToolItem MakeToolButton(ActionCommand command, string? iconName = null) =>
        WithToolItemIcon(new ButtonToolItem
        {
            Command = command,
            Text = null,
            ToolTip = command.Text
        }, command, iconName);

    private ToolItem WithToolItemIcon(ToolItem toolItem, ActionCommand command, string? iconName = null)
    {
        EtoPlatform.Current.AttachDpiDependency(_window!,
            scale =>
            {
                EtoPlatform.Current.SetImageSize(toolItem, (int) (16 * scale));
                toolItem.Image = iconName == null
                    ? command.GetIconImage(IconScenario.SmallToolbar, scale)
                    : EtoPlatform.Current.IconProvider.GetIcon(iconName, IconScenario.SmallToolbar, scale);
            });
        return toolItem;
    }

    /// <summary>
    /// Creates the edit actions as a row of buttons, for platforms where toolbars can't be part of the layout.
    /// </summary>
    private LayoutElement CreateEditButtons()
    {
        var rotate = CreateHeaderButton(Commands.RotateMenu);
        var rotateMenu = C.ContextMenu(_window!, new MenuProvider()
            .Append(Commands.RotateLeft)
            .Append(Commands.RotateRight)
            .Append(Commands.Flip)
            .Append(Commands.Deskew)
            .Append(Commands.CustomRotate));
        rotate.Click += (_, _) => rotateMenu.Show(rotate);
        var buttons = new List<LayoutElement>
        {
            HeaderButton(rotate),
            EditToggle<CropEditor>(),
            EditToggle<BrightContEditor>(),
            EditToggle<HueSatEditor>(),
            EditToggle<BlackWhiteEditor>(),
            EditToggle<SharpenEditor>(),
            HeaderButton(CreateHeaderButton(Commands.DocumentCorrection)),
            EditToggle<CompressEditor>(),
            EditToggle<SplitEditor>(),
            EditToggle<CombineEditor>()
        };
        if (_editWithController.HasApp)
        {
            buttons.Add(HeaderButton(CreateHeaderButton(Commands.CreateEditWithApp())));
        }
        buttons.Add(HeaderButton(CreateHeaderButton(Commands.EditWithPick)).SpacingAfter(12));
        if (_windowed)
        {
            buttons.AddRange(GetSaveCommands().Select(x =>
                HeaderButton(CreateHeaderButton(x.Command, x.IconName)).SpacingAfter(12)));
            buttons.Add(HeaderButton(CreateHeaderButton(DeleteCurrentImageCommand)));
        }
        return L.Row(buttons.ToArray()).Spacing(2);
    }

    private LayoutControl EditToggle<T>() where T : ImageEditor
    {
        var command = _editCommands[typeof(T)];
        var toggle = new ToggleButton
        {
            ImagePosition = ButtonImagePosition.Overlay,
            ToolTip = command.Text,
            Enabled = command.Enabled
        };
        EtoPlatform.Current.SetAccessibleName(toggle, command.Text);
        EtoPlatform.Current.AttachDpiDependency(toggle,
            scale => toggle.Image = command.GetIconImage(IconScenario.Button, scale));
        EtoPlatform.Current.ConfigureImageButton(toggle, default);
        toggle.Click += (_, _) =>
        {
            command.Execute();
            UpdateActiveEditTool();
        };
        command.EnabledChanged += (_, _) => toggle.Enabled = command.Enabled;
        _editToggles[typeof(T)] = toggle;
        return HeaderButton(toggle);
    }

    private void EditWithController_AppChanged(object? sender, EventArgs e)
    {
        if (_window is { Loaded: true })
        {
            UpdateHeader();
        }
    }

    private void UpdateActiveEditTool()
    {
        _updatingEditTools = true;
        foreach (var (type, item) in _editToolItems)
        {
            item.Checked = type == ActiveEditorType;
        }
        foreach (var (type, toggle) in _editToggles)
        {
            toggle.Checked = type == ActiveEditorType;
        }
        _updatingEditTools = false;
    }

    private void ColorSchemeChanged(object? sender, EventArgs e) => Invoker.Current.Invoke(UpdateColors);

    private void UpdateColors()
    {
        _separator.BackgroundColor = _colorScheme.SeparatorColor;
    }

    public void Show(UiImage image, Type? editorType = null)
    {
        EndEdit();
        SetCurrentImage(image);
        Commands.UpdateState();
        _topInset.Height = EtoPlatform.Current.GetTopContentInset(_window!);
        _paneVis.IsVisible = true;
        UpdatePage();
        UpdateImage().AssertNoAwait();
        if (editorType != null)
        {
            _editCommands[editorType].Execute();
        }
    }

    public void Hide()
    {
        if (!IsVisible) return;
        EndEdit();
        _paneVis.IsVisible = false;
        SetCurrentImage(null);
        _renderVersion++;
        ImageViewer.SetImage(null);
        _viewBitmap?.Dispose();
        _viewBitmap = null;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    private void SwitchView()
    {
        if (_currentImage == null) return;
        _config.User.Set(c => c.PreviewLastShownInPane, _windowed);
        if (_windowed)
        {
            ViewInPreviewPaneRequested?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Commands.ViewInNewWindow.Execute();
        }
    }

    private void SetCurrentImage(UiImage? image)
    {
        if (_currentImage != null)
        {
            _currentImage.ThumbnailInvalidated -= ImageThumbnailInvalidated;
        }
        _currentImage = image;
        if (_currentImage != null)
        {
            _currentImage.ThumbnailInvalidated += ImageThumbnailInvalidated;
        }
    }

    private void ImageThumbnailInvalidated(object? sender, EventArgs e)
    {
        Invoker.Current.InvokeDispatch(() =>
        {
            EndEdit();
            UpdateImage().AssertNoAwait();
        });
    }

    private void ImageList_SelectionChanged(object? sender, EventArgs e)
    {
        Invoker.Current.InvokeDispatch(SyncToSelection);
    }

    private void SyncToSelection()
    {
        if (_windowed || !IsVisible || _editor != null) return;
        var selected = _imageList.Selection.FirstOrDefault();
        if (selected != null && selected != _currentImage)
        {
            Show(selected);
        }
    }

    private void ImageList_ImagesUpdated(object? sender, ImageListEventArgs e)
    {
        Invoker.Current.InvokeDispatch(() =>
        {
            if (_currentImage == null) return;
            UiImage? next;
            lock (_imageList)
            {
                if (_imageList.Images.Contains(_currentImage))
                {
                    UpdatePage();
                    return;
                }
                next = _imageList.Images.Any()
                    ? _imageList.Images[Math.Min(ImageIndex, _imageList.Images.Count - 1)]
                    : null;
                _imageList.UpdateSelection(next == null ? ListSelection.Empty<UiImage>() : ListSelection.Of(next));
            }
            if (next == null)
            {
                // No images left to display, so no point keeping the preview open
                Hide();
            }
            else
            {
                Show(next);
            }
        });
    }

    private void GoTo(int index)
    {
        UiImage image;
        lock (_imageList)
        {
            if (index == ImageIndex || index < 0 || index >= _imageList.Images.Count)
            {
                return;
            }
            image = _imageList.Images[index];
            _imageList.UpdateSelection(ListSelection.Of(image));
        }
        Show(image);
    }

    private void UpdatePage()
    {
        if (_currentImage == null) return;
        int count = _imageList.Images.Count;
        ImageIndex = Math.Max(_imageList.Images.IndexOf(_currentImage), 0);
        _pageLabel.Text = string.Format(UiStrings.XOfY, ImageIndex + 1, count);
        GoToPrevCommand.Enabled = ImageIndex > 0;
        GoToNextCommand.Enabled = ImageIndex < count - 1;
        PageChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task UpdateImage()
    {
        if (_currentImage is not { } image) return;
        int version = ++_renderVersion;
        using var imageToRender = image.GetClonedImage();
        using var rendered = await Task.Run(() => imageToRender.Render());
        if (version != _renderVersion) return;
        var oldBitmap = _viewBitmap;
        _viewBitmap = rendered.ToEtoImage();
        if (_editor == null)
        {
            ImageViewer.SetImage(_viewBitmap);
            ImageViewer.ZoomToContainer();
        }
        oldBitmap?.Dispose();
    }

    private void Edit(ImageEditor editor)
    {
        if (_updatingEditTools) return;
        if (_editor?.GetType() == editor.GetType())
        {
            _editor.CancelButton.PerformClick();
            return;
        }
        EndEdit();
        _editor = editor;
        editor.ParentWindow = _window;
        editor.Viewer = ImageViewer;
        editor.Initialize();
        editor.Finished += (_, _) =>
        {
            EndEdit();
            SyncToSelection();
        };
        editor.OkButton.Text = UiStrings.Apply;
        _editorPlaceholder.Content = editor.CreateLayout();
        _editVis.IsVisible = true;
        EtoPlatform.Current.SetDefaultButton(_window!, editor.OkButton);
        UpdateActiveEditTool();
        Invoker.Current.InvokeDispatch(() =>
        {
            if (_editor == editor)
            {
                editor.OnShown();
            }
        });
    }

    private void EndEdit()
    {
        if (_editor == null) return;
        var editor = _editor;
        _editor = null;
        _editorPlaceholder.Content = null;
        _editVis.IsVisible = false;
        EtoPlatform.Current.SetDefaultButton(_window!, null);
        UpdateActiveEditTool();
        editor.Dispose();
        ImageViewer.SetImage(_viewBitmap);
        ImageViewer.ZoomToContainer();
    }

    public bool HandleKeyDown(Keys keys)
    {
        if (!IsVisible) return false;
        if (_editor == null)
        {
            return _previewKsm.Perform(keys);
        }
        if (keys == Keys.Escape)
        {
            _editor.CancelButton.PerformClick();
            return true;
        }
        if (keys == Keys.Enter && !_windowed)
        {
            _editor.OkButton.PerformClick();
            return true;
        }
        var args = new KeyEventArgs(keys, KeyEventType.KeyDown);
        _editor.HandleKeyDown(args);
        return args.Handled || _editKsm.Perform(keys);
    }

    private void DeleteCurrentImage()
    {
        if (MessageDialog.Confirm(_window!, string.Format(UiStrings.ConfirmDeleteItems, 1), UiStrings.Delete,
                UiStrings.Delete, destructive: true))
        {
            // We don't want to run Commands.Delete as that runs on DesktopController and uses that selection.
            Commands.ImageListActions.DeleteSelected();
        }
    }

    private void AssignKeyboardShortcuts()
    {
        // Unconfigurable defaults

        _previewKsm.Assign("Esc", Hide);
        _previewKsm.Assign("Mod+0", ZoomActualCommand);
        _previewKsm.Assign("PageDown", GoToNextCommand);
        _previewKsm.Assign("PageUp", GoToPrevCommand);
        _editKsm.Assign("Mod+0", ZoomActualCommand);

        if (!_windowed)
        {
            // The desktop form has its own shortcuts for everything else
            return;
        }

        _previewKsm.Assign("Del", DeleteCurrentImageCommand);
        _previewKsm.Assign("Mod+Z", Commands.Undo);
        _previewKsm.Assign(EtoPlatform.Current.IsWindows ? "Mod+Y" : "Mod+Shift+Z", Commands.Redo);

        _previewKsm.Assign("Right", GoToNextCommand);
        _previewKsm.Assign("Down", GoToNextCommand);
        _previewKsm.Assign("Left", GoToPrevCommand);
        _previewKsm.Assign("Up", GoToPrevCommand);

        // Configured defaults

        var ks = _config.Get(c => c.KeyboardShortcuts);

        _previewKsm.Assign(ks.Delete, DeleteCurrentImageCommand);
        _previewKsm.Assign(ks.ImageBlackWhite, Commands.BlackWhite);
        _previewKsm.Assign(ks.ImageBrightness, Commands.BrightCont);
        _previewKsm.Assign(ks.ImageContrast, Commands.BrightCont);
        _previewKsm.Assign(ks.ImageCrop, Commands.Crop);
        _previewKsm.Assign(ks.ImageHue, Commands.HueSat);
        _previewKsm.Assign(ks.ImageSaturation, Commands.HueSat);
        _previewKsm.Assign(ks.ImageSharpen, Commands.Sharpen);
        _previewKsm.Assign(ks.ImageDocumentCorrection, Commands.DocumentCorrection);
        _previewKsm.Assign(ks.ImageCompress, Commands.Compress);
        _previewKsm.Assign(ks.ImageSplit, Commands.Split);
        _previewKsm.Assign(ks.ImageCombine, Commands.Combine);
        _previewKsm.Assign(ks.ImageEditWith, Commands.CreateEditWithApp());

        _previewKsm.Assign(ks.RotateCustom, Commands.CustomRotate);
        _previewKsm.Assign(ks.RotateDeskew, Commands.Deskew);
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

        _editKsm.Assign(ks.ZoomIn, ZoomInCommand);
        _editKsm.Assign(ks.ZoomOut, ZoomOutCommand);
    }

    public void Dispose()
    {
        EndEdit();
        SetCurrentImage(null);
        ImageViewer.SetImage(null);
        _viewBitmap?.Dispose();
        _imageList.ImagesUpdated -= ImageList_ImagesUpdated;
        _imageList.SelectionChanged -= ImageList_SelectionChanged;
        _editWithController.AppChanged -= EditWithController_AppChanged;
        _colorScheme.ColorSchemeChanged -= ColorSchemeChanged;
        Commands.Dispose();
    }
}
